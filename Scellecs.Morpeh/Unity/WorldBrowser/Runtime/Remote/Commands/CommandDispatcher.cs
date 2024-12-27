#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe class CommandDispatcher {
        private readonly NetworkTransport transport;
        private readonly CommandHandlerRegistry handlers;
        private readonly Queue<Command> pendingCommands;
        private bool isCollectingBatch;

        internal CommandDispatcher(NetworkTransport transport, CommandHandlerRegistry handlers) {
            this.transport = transport;
            this.handlers = handlers;
            this.pendingCommands = new Queue<Command>();
        }

        internal void CollectCommands() {
            try {
                while (this.transport.TryDequeue(out byte* data, out int length)) {
                    ProcessIncomingMessage(data, length);
                }
            }
            catch (Exception e) {
                this.transport.EndReceive();
                throw e;
            }
        }

        internal void BeginBatch() {
            this.transport.PushSend(default(BatchStartCommand).Serialize(transport.SendAllocator, out var startLength), startLength);
        }

        internal void EndBatch() {
            this.transport.PushSend(default(BatchEndCommand).Serialize(transport.SendAllocator, out var endLength), endLength);
            this.transport.EndSend();
        }

        private void ProcessIncomingMessage(byte* data, int length) {
            if (length < 2) {
                return;
            }

            var command = Command.FromPtr(data, length);
            if (command.CommandType == CommandTypeId.System) {
                ProcessSystemCommand(command);
                return;
            }

            if (!this.isCollectingBatch) {
                return;
            }
            this.transport.Log($"Collect command {command.CommandType}, {command.CommandId}");
            this.pendingCommands.Enqueue(command);
        }

        private void ProcessSystemCommand(Command command) {
            switch (command.CommandId) {
                case SystemCommand.BatchStart:
                    if (isCollectingBatch) {
                        transport.Log("Receive error: batch already started");
                        return;
                    }
                    this.isCollectingBatch = true;
                    break;

                case SystemCommand.BatchEnd:
                    if (!this.isCollectingBatch) {
                        transport.Log("Receive error: batch already ended");
                        return;
                    }
                    this.isCollectingBatch = false;
                    BeginBatch();
                    ExecutePendingCommands();
                    EndBatch();
                    this.transport.EndReceive();
                    break;
            }
        }

        private void ExecutePendingCommands() {
            if (this.pendingCommands.Count == 0) {
                return;
            }

            while (pendingCommands.Count > 0) {
                var command = pendingCommands.Dequeue();
                if (!this.handlers.TryGetHandler(command.CommandType, out var handler)) {
                    continue;
                }
                this.transport.Log($"Processing: {command.CommandType}, {command.CommandId}");
                handler.Handle(command, transport);
            }
        }
    }
}
#endif

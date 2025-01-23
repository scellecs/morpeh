#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;
using System.Threading;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe sealed class MessagePipe {
        private readonly object pipeLock;
        private readonly Queue<Message> queue;
        private readonly NetworkAllocator allocator;
        private readonly ManualResetEvent pending;

        internal NetworkAllocator Allocator => allocator;
        internal ManualResetEvent WaitHandle => pending;

        internal MessagePipe() {
            this.pipeLock = new object();
            this.queue = new Queue<Message>();
            this.allocator = new NetworkAllocator(this.pipeLock);
            this.pending = new ManualResetEvent(false);
        }

        internal void Push(byte* data, int length) {
            lock (this.pipeLock) {
                this.queue.Enqueue(new Message() {
                    data = data,
                    length = length
                });

                this.pending.Set();
            }
        }

        internal bool TryDequeue(out Message message) {
            lock (this.pipeLock) {
                if (this.queue.Count > 0) {
                    message = queue.Dequeue();
                    return true;
                }

                message = default;
                return false;
            }
        }

        internal void Clear() {
            lock (this.pipeLock) {
                this.queue.Clear();
                this.pending.Reset();
                this.allocator.Update();
            }
        }
    }
}
#endif

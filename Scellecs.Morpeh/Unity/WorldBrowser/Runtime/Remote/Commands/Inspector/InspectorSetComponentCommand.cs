﻿#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands
{
    [Serializable]
    internal struct InspectorSetComponentCommand : ICommand {
        public ComponentDataBoxed componentData;

        public byte CommandType() => CommandTypeId.Inspector;
        public byte CommandId() => InspectorCommand.SetComponent;
    }
}
#endif

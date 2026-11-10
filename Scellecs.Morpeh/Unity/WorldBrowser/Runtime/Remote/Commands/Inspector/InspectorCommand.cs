#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal sealed class InspectorCommand {
        internal const byte FetchRequest = 1;
        internal const byte FetchResponse = 2;
        internal const byte AddComponent = 3;
        internal const byte RemoveComponent = 4;
        internal const byte SetComponent = 5;
        internal const byte SetAddComponentSearchString = 6;
    }
}
#endif

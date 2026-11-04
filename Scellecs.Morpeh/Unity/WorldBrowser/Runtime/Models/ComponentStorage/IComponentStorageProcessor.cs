#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IComponentStorageProcessor {
        public ComponentStorageModel GetModel();
    }
}
#endif

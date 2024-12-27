#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IComponentStorageProcessor {
        public ComponentStorageModel GetModel();
    }
}
#endif

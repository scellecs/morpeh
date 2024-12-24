#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IComponentStorageProcessor {
        public ComponentStorageModel GetModel();
    }
}
#endif

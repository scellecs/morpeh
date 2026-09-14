#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser {
    internal abstract class BaseModel {
        public long version;

        public void IncrementVersion() {
            unchecked {
                this.version++;
            }
        }
    }
}
#endif
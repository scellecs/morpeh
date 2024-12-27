#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
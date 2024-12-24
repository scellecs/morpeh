#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
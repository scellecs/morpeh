#if UNITY_EDITOR
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal abstract class BaseViewModel : IViewModel {
        protected long version;

        public long GetVersion() => this.version;

        protected void IncrementVersion() {
            unchecked {
                this.version++;
            }
        }
    }
}
#endif

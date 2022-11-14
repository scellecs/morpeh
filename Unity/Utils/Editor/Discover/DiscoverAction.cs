namespace Scellecs.Morpeh.Utils.Editor.Discover {
    using UnityEngine;

    public abstract class DiscoverAction : ScriptableObject {
        public abstract string ActionName { get; }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button(Name = "@ActionName")]
#endif
        public abstract void DoAction();
    }
}
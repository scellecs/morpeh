namespace Scellecs.Morpeh.Utils.Editor.Discover {
    using UnityEngine;
    using Sirenix.OdinInspector;

    public abstract class DiscoverAction : ScriptableObject {
        public abstract string ActionName { get; }

        [Button(Name = "$ActionName")]
        public abstract void DoAction();
    }
}
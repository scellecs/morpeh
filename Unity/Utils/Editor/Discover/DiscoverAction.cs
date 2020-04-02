namespace Morpeh.Utils.Editor {
    using UnityEngine;

    public abstract class DiscoverAction : ScriptableObject {
        public abstract string ActionName { get; }

        public abstract void DoAction();
    }
}
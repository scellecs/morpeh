namespace Morpeh.Globals {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Events/Event")]
    public class GlobalEvent : GlobalEventInt {
        [ContextMenu("Publish")]
        public virtual void Publish() {
            base.Publish(-1);
        }
        
        [ContextMenu("NextFrame")]
        public virtual void NextFrame() {
            base.NextFrame(-1);
        }
    }
}
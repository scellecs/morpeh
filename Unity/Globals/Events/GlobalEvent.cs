namespace Morpeh.Globals {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Event")]
    public class GlobalEvent : GlobalEventInt {
        [ContextMenu("Publish")]
        public void Publish() {
            base.Publish(-1);
        }
        
        [ContextMenu("NextFrame")]
        public void NextFrame() {
            base.NextFrame(-1);
        }
    }
}
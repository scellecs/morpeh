namespace Morpeh.Globals {
    using UnityEngine;

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
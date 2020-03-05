namespace Morpeh.Tasks.Actions {
    using Globals;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Actions/" + nameof(FilterListByEventActionSystem))]
    public sealed class FilterListByEventActionSystem : ActionSystem {
        [Required]
        public GlobalEventObject evnt;
        
        [Required]
        public GlobalVariableListObject baseList;
        
        [Required]
        public GlobalVariableListObject filteredList;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            this.filteredList.Value.Clear();
            
            foreach (var obj in this.evnt.BatchedChanges) {
                if (this.baseList.Value.Contains(obj)) {
                    this.filteredList.Value.Add(obj);
                }
            }
        }
    }
}

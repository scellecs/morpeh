namespace Morpeh.Globals {
    using Sirenix.OdinInspector;
    using Tasks;
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
            var fl = this.filteredList.Value;
            fl.Clear();
            
            foreach (var obj in this.evnt.BatchedChanges) {
                if (obj is GameObject go) {
                    foreach (var baseObj in this.baseList.Value) {
                        switch (baseObj) {
                            case GameObject baseGo when baseGo == go:
                            case Component baseComponent when baseComponent.gameObject == go:
                                fl.Add(baseObj);
                                break;
                        }
                    }
                }
                else if (obj is Component component) {
                    foreach (var baseObj in this.baseList.Value) {
                        switch (baseObj) {
                            case GameObject baseGo when baseGo == component.gameObject:
                            case Component baseComponent when baseComponent == component:
                                fl.Add(baseObj);
                                break;
                        }
                    }
                }
            }
        }
    }
}

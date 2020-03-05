namespace Morpeh.Globals {
    using Sirenix.OdinInspector;
    using Tasks;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Actions/" + nameof(SetActiveGameObjectActionSystem))]
    public sealed class SetActiveGameObjectActionSystem : ActionSystem {
        [Required]
        public GlobalVariableListObject gameObjectVariable;
        [Required]
        public GlobalVariableBool isActive;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            foreach (var obj in this.gameObjectVariable.Value) {
                switch (obj) {
                    case GameObject go:
                        go.SetActive(this.isActive.Value);
                        break;
                    case Component component:
                        component.gameObject.SetActive(this.isActive.Value);
                        break;
                }
            }
        }
    }
}
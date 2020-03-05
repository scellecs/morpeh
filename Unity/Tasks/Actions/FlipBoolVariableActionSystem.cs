namespace Morpeh.Tasks.Actions {
    using Globals;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Actions/" + nameof(FlipBoolVariableActionSystem))]
    public sealed class FlipBoolVariableActionSystem : ActionSystem {
        [Required]
        public GlobalVariableBool variable;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            this.variable.Value = !this.variable.Value;
        }
    }
}
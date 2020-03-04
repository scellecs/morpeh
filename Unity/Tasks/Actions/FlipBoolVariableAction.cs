namespace Morpeh.Tasks.Actions {
    using Globals;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Actions/" + nameof(FlipBoolVariableAction))]
    public sealed class FlipBoolVariableAction : BaseAction {
        [Required]
        public GlobalVariableBool variable;

        public override void Execute() {
            this.variable.Value = !this.variable.Value;
        }
    }
}
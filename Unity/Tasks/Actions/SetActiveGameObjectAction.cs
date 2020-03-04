namespace Morpeh.Tasks.Actions {
    using Globals;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Actions/" + nameof(SetActiveGameObjectAction))]
    public sealed class SetActiveGameObjectAction : BaseAction {
        [Required]
        public GlobalVariableGameObject gameObjectVariable;
        [Required]
        public GlobalVariableBool isActive;

        public override void Execute() {
            this.gameObjectVariable.Value.SetActive(this.isActive.Value);
        }
    }
}
namespace Morpeh.Tasks.Actions {
    using Globals;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu]
    public class SetActiveGameObjectAction : BaseAction {
        [Required]
        public GlobalVariableGameObject gameObjectVariable;
        [Required]
        public GlobalVariableBool isActive;

        public override void Execute() {
            this.gameObjectVariable.Value.SetActive(this.isActive.Value);
        }
    }
}
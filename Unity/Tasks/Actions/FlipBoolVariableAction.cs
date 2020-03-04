namespace Morpeh.Tasks.Actions {
    using Globals;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu]
    public class FlipBoolVariableAction : BaseAction {
        [Required]
        public GlobalVariableBool variable;

        public override void Execute() {
            this.variable.Value = !this.variable.Value;
        }
    }
}
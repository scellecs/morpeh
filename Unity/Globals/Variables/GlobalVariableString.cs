namespace Morpeh.Globals {
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Globals/Variable String")]
    public class GlobalVariableString : BaseGlobalVariable<string> {
        protected override string Load(string serializedData) => serializedData;

        protected override string Save() => this.value;
    }
}
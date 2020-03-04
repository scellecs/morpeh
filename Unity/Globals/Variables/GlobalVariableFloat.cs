namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
    
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Float")]
    public class GlobalVariableFloat : BaseGlobalVariable<float> {
        protected override float Load(string serializedData) => float.Parse(serializedData, CultureInfo.InvariantCulture);

        protected override string Save() => this.value.ToString(CultureInfo.InvariantCulture);
    }
}
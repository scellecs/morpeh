namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Bool")]
    public class GlobalVariableBool : BaseGlobalVariable<bool> {
        protected override bool Load(string serializedData) => bool.Parse(serializedData);

        protected override string Save() => this.value.ToString(CultureInfo.InvariantCulture);
    }
}
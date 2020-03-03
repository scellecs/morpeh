namespace Morpeh.Globals {
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable String")]
    public class GlobalVariableString : BaseGlobalVariable<string> {
        protected override string Load(string serializedData) => serializedData;

        protected override string Save() => this.value;
    }
}
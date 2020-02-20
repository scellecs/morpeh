namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List String")]
    public class GlobalVariableListString : BaseGlobalVariable<List<string>> {
        protected override List<string> Load(string serializedData)
            => JsonUtility.FromJson<List<string>>(serializedData);

        protected override string Save() => JsonUtility.ToJson(this.value);
    }
}
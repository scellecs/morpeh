namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Float")]
    public class GlobalVariableListFloat : BaseGlobalVariable<List<float>> {
        protected override List<float> Load(string serializedData)
            => JsonUtility.FromJson<List<float>>(serializedData);

        protected override string Save() => JsonUtility.ToJson(this.value);
    }
}
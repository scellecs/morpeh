namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Bool")]
    public class GlobalVariableListBool : BaseGlobalVariable<List<bool>> {
        protected override List<bool> Load(string serializedData)
            => JsonUtility.FromJson<List<bool>>(serializedData);

        protected override string Save() => JsonUtility.ToJson(this.value);
    }
}
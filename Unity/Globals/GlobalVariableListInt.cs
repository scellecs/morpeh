namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Int")]
    public class GlobalVariableListInt : BaseGlobalVariable<List<int>> {
        protected override List<int> Load(string serializedData)
            => JsonUtility.FromJson<List<int>>(serializedData);

        protected override string Save() => JsonUtility.ToJson(this.value);
    }
}
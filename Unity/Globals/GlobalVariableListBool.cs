namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Bool")]
    public class GlobalVariableListBool : BaseGlobalVariable<List<bool>> {
        protected override List<bool> Load(string serializedData)
            => JsonUtility.FromJson<ListBoolWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListBoolWrapper(this.value));
    }

    [Serializable]
    struct ListBoolWrapper
    {
        public List<bool> list;

        public ListBoolWrapper(List<bool> list)
        {
            this.list = list;
        }
    }
}
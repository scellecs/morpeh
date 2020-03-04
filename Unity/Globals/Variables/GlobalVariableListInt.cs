namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Int")]
    public class GlobalVariableListInt : BaseGlobalVariable<List<int>> {
        protected override List<int> Load(string serializedData)
            => JsonUtility.FromJson<ListIntWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListIntWrapper(this.value));
    }

    [Serializable]
    struct ListIntWrapper
    {
        public List<int> list;

        public ListIntWrapper(List<int> list)
        {
            this.list = list;
        }
    }
}
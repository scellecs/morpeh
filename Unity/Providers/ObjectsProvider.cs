namespace Morpeh.Providers {
    using System.Collections.Generic;
    using Globals;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class ObjectsProvider : MonoBehaviour {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [TableList(AlwaysExpanded = true)]
        [HideLabel]
#endif
        public List<ObjectPair> table;

        [System.Serializable]
        public class ObjectPair {
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Variables")]
            [HideLabel]
#endif
            public GlobalVariableListObject variable;
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Objects")]
            [HideLabel]
#endif
            public Object obj;
        }

        private void OnEnable() {
            foreach (var pair in this.table) {
                pair.variable.Value.Add(pair.obj);
            }
        }

        private void OnDisable() {
            foreach (var pair in this.table) {
                pair.variable.Value.Remove(pair.obj);
            }
        }
    }
}
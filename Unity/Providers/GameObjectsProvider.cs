namespace Morpeh.Providers {
    using System;
    using System.Collections.Generic;
    using Globals;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using UnityEngine;

    public class GameObjectsProvider : MonoBehaviour {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [TableList]
        [HideLabel]
#endif
        public List<GameObjectPair> table;

        [System.Serializable]
        public class GameObjectPair {
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Variables")]
            [HideLabel]
#endif
            public GlobalVariableGameObject variable;
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("GameObjects")]
            [HideLabel]
#endif
            public GameObject gameObject;
        }

        private void Start() {
            foreach (var pair in this.table) {
                pair.variable.Value = pair.gameObject;
            }
        }
    }
}
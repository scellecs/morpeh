namespace Morpeh.Providers {
    using System.Collections.Generic;
    using Globals;
    using Unity.IL2CPP.CompilerServices;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using UnityEngine;
    using Object = UnityEngine.Object;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(ObjectsProvider))]
    public sealed class ObjectsProvider : MonoBehaviour {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Header("Global Variables")]
        [TableList(AlwaysExpanded = true)]
        [HideLabel]
#endif
        public List<ObjectPair> table;
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Space]
        [Header("Global List Variables")]
        [TableList(AlwaysExpanded = true)]
        [HideLabel]
#endif
        public List<ObjectListPair> tableList;

        [System.Serializable]
        public class ObjectListPair {
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
        
        [System.Serializable]
        public class ObjectPair {
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Variables")]
            [HideLabel]
#endif
            public GlobalVariableObject variable;
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Objects")]
            [HideLabel]
#endif
            public Object obj;
        }

        private void OnEnable() {
            this.Add();
        }

        private void OnDisable() {
            this.Remove();
        }

        private void Add() {
            foreach (var pair in this.tableList) {
                var list = pair.variable.Value;
                if (pair.obj != null && !list.Contains(pair.obj)) {
                    list.Add(pair.obj);
                }
            }
            foreach (var pair in this.table) {
                if (pair.obj != null) {
                    pair.variable.Value = pair.obj;
                }
            }
        }

        private void Remove() {
            foreach (var pair in this.tableList) {
                if (pair.obj != null) {
                    pair.variable.Value.Remove(pair.obj);
                }
            }
        }
    }
}
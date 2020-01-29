namespace Morpeh {
    using Utils;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    //TODO refactor for Reorder in Runtime
    public class Installer : WorldViewer {
        [Space]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-3)]
#endif
        public UpdateSystemPair[] updateSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-2)]
#endif
        public FixedSystemPair[] fixedUpdateSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-1)]
#endif
        public LateSystemPair[] lateUpdateSystems;

        private void OnEnable() {
            this.AddSystems(this.updateSystems);
            this.AddSystems(this.fixedUpdateSystems);
            this.AddSystems(this.lateUpdateSystems);
        }

        private void OnDisable() {
            this.RemoveSystems(this.updateSystems);
            this.RemoveSystems(this.fixedUpdateSystems);
            this.RemoveSystems(this.lateUpdateSystems);
        }

        private void AddSystems<T>(BasePair<T>[] pairs) where T : class, ISystem {
            for (int i = 0, length = pairs.Length; i < length; i++) {
                var pair   = pairs[i];
                var system = pair.System;
                if (pair.Enabled && system != null) {
                    World.Default.AddSystem(i, system);
                }
            }
        }

        private void RemoveSystems<T>(BasePair<T>[] pairs) where T : class, ISystem {
            for (int i = 0, length = pairs.Length; i < length; i++) {
                var system = pairs[i].System;
                if (system != null) {
                    World.Default.RemoveSystem(system);
                }
            }
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnInspectorGUI]
        private void OnEditoGUI() {
            gameObject.transform.hideFlags = HideFlags.HideInInspector;
        }
#endif
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/ECS/", true, 10)]
        private static bool OrderECS() => true;

        [UnityEditor.MenuItem("GameObject/ECS/Installer", false, 1)]
        private static void CreateInstaller(UnityEditor.MenuCommand menuCommand) {
            var go = new GameObject("[Installer]");
            go.AddComponent<Installer>();
            UnityEditor.GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            UnityEditor.Selection.activeObject = go;
        }
#endif
//        private void Reorder() {
//            this.InternalReorder(this.updateSystems);
//        }
//        private void ReorderFixed() {
//            this.InternalReorder(this.fixedUpdateSystems);
//        }
//        private void ReorderLate() {
//            this.InternalReorder(this.lateUpdateSystems);
//        }
//
//        private void InternalReorder<T>(BasePair<T>[] collection) where T : class, ISystem {
//            var temp = collection.Where(p => p.Added).Select(p => p.System).ToList();
//            World.ReorderSystems(temp);
//        }
    }

    namespace Utils {
        using System;
        using JetBrains.Annotations;
        using UnityEngine;

#if UNITY_EDITOR && ODIN_INSPECTOR
        using Sirenix.OdinInspector;
#endif
        [Serializable]
        public abstract class BasePair<T> where T : class, ISystem {
            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Pair", 10)]
            [HideLabel]
            [OnValueChanged(nameof(OnChange))]
#endif
            private bool enabled;

#pragma warning disable CS0649
            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HorizontalGroup("Pair")]
            [HideLabel]
#endif
            [CanBeNull]
            private T system;
#pragma warning restore CS0649

            public bool Enabled {
                get => this.enabled;
                set => this.enabled = value;
            }

            [CanBeNull]
            public T System => this.system;

            public BasePair() => this.enabled = true;

            private void OnChange() {
#if UNITY_EDITOR

                if (Application.isPlaying) {
                    if (this.enabled) {
                        World.Default.EnableSystem(this.system);
                    }
                    else {
                        World.Default.DisableSystem(this.system);
                    }
                }
#endif
            }
        }

        [Serializable]
        public class UpdateSystemPair : BasePair<UpdateSystem> {
        }

        [Serializable]
        public class FixedSystemPair : BasePair<FixedUpdateSystem> {
        }

        [Serializable]
        public class LateSystemPair : BasePair<LateUpdateSystem> {
        }
        
        
    }
}
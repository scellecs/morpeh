namespace Morpeh {
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Linq;
    using UnityEngine;
    using Utils;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(Installer))]
    public sealed class Installer : BaseInstaller {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Required]
        [InfoBox("Order collision with other installer!", InfoMessageType.Error, nameof(IsCollisionWithOtherInstaller))]
        [PropertyOrder(-5)]
#endif
        public int order;
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        private bool IsCollisionWithOtherInstaller 
            => FindObjectsOfType<Installer>().Where(i => i != this).Any(i => i.order == this.order);
#endif
        
        [Space]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-5)]
#endif
        public Initializer[] initializers;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-4)]
        [OnValueChanged(nameof(OnValueChangedUpdate))]
#endif
        public UpdateSystemPair[] updateSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-3)]
        [OnValueChanged(nameof(OnValueChangedFixedUpdate))]
#endif
        public FixedSystemPair[] fixedUpdateSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-2)]
        [OnValueChanged(nameof(OnValueChangedLateUpdate))]
#endif
        public LateSystemPair[] lateUpdateSystems;

        private SystemsGroup group;

        private void OnValueChangedUpdate() {
            if (Application.isPlaying) {
                this.RemoveSystems(this.updateSystems);
                this.AddSystems(this.updateSystems);
            }
        }
        
        private void OnValueChangedFixedUpdate() {
            if (Application.isPlaying) {
                this.RemoveSystems(this.fixedUpdateSystems);
                this.AddSystems(this.fixedUpdateSystems);
            }
        }
        
        private void OnValueChangedLateUpdate() {
            if (Application.isPlaying) {
                this.RemoveSystems(this.lateUpdateSystems);
                this.AddSystems(this.lateUpdateSystems);
            }
        }
        

        protected override void OnEnable() {
            this.group = World.Default.CreateSystemsGroup();
            
            for (int i = 0, length = this.initializers.Length; i < length; i++) {
                var initializer = this.initializers[i];
                this.group.AddInitializer(initializer);
            }

            this.AddSystems(this.updateSystems);
            this.AddSystems(this.fixedUpdateSystems);
            this.AddSystems(this.lateUpdateSystems);
            
            World.Default.AddSystemsGroup(this.order, this.group);
        }

        protected override void OnDisable() {
            this.RemoveSystems(this.updateSystems);
            this.RemoveSystems(this.fixedUpdateSystems);
            this.RemoveSystems(this.lateUpdateSystems);
            
            World.Default.RemoveSystemsGroup(this.group);
        }

        private void AddSystems<T>(BasePair<T>[] pairs) where T : class, ISystem {
            for (int i = 0, length = pairs.Length; i < length; i++) {
                var pair   = pairs[i];
                var system = pair.System;
                pair.group = this.group;
                if (system != null) {
                    this.group.AddSystem(system, pair.Enabled);
                }
            }
        }

        private void RemoveSystems<T>(BasePair<T>[] pairs) where T : class, ISystem {
            for (int i = 0, length = pairs.Length; i < length; i++) {
                var system = pairs[i].System;
                if (system != null) {
                    this.group.RemoveSystem(system);
                }
            }
        }
        
#if UNITY_EDITOR
        [MenuItem("GameObject/ECS/", true, 10)]
        private static bool OrderECS() => true;

        [MenuItem("GameObject/ECS/Installer", false, 1)]
        private static void CreateInstaller(MenuCommand menuCommand) {
            var go = new GameObject("[Installer]");
            go.AddComponent<Installer>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif
    }

    namespace Utils {
        using System;
        using JetBrains.Annotations;
#if UNITY_EDITOR && ODIN_INSPECTOR
        using Sirenix.OdinInspector;
#endif
        [Serializable]
        public abstract class BasePair<T> where T : class, ISystem {
            internal SystemsGroup group;
            
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
                        this.group.EnableSystem(this.system);
                    }
                    else {
                        this.group.DisableSystem(this.system);
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
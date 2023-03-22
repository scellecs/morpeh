namespace Scellecs.Morpeh {
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using Systems;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Utils;
    
#if UNITY_EDITOR
    using UnityEditor;
    using Sirenix.OdinInspector;
#endif

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(Installer))]
    public class Installer : BaseInstaller {
#if UNITY_EDITOR
        // TODO check is Required attribute really required
        // [Required]
        [InfoBox("Order collision with other installer!", InfoMessageType.Error, nameof(IsCollisionWithOtherInstaller))]
        [PropertyOrder(-7)]
#endif
        public int order;
        
#if UNITY_EDITOR
        private bool IsCollisionWithOtherInstaller 
            => this.IsPrefab() == false && FindObjectsOfType<Installer>().Where(i => i != this).Any(i => i.order == this.order);
        
        private bool IsPrefab() => this.gameObject.scene.name == null;
#endif
        
        [Space]
#if UNITY_EDITOR
        [PropertyOrder(-6)]
        [Required]
#endif
        public Initializer[] initializers;
#if UNITY_EDITOR
        [PropertyOrder(-5)]
        [OnValueChanged(nameof(OnValueChangedUpdate))]
#endif
        public UpdateSystemPair[] updateSystems;
#if UNITY_EDITOR
        [PropertyOrder(-4)]
        [OnValueChanged(nameof(OnValueChangedFixedUpdate))]
#endif
        public FixedSystemPair[] fixedUpdateSystems;
#if UNITY_EDITOR
        [PropertyOrder(-3)]
        [OnValueChanged(nameof(OnValueChangedLateUpdate))]
#endif
        public LateSystemPair[] lateUpdateSystems;
        
#if UNITY_EDITOR
        [PropertyOrder(-2)]
        [OnValueChanged(nameof(OnValueChangedCleanup))]
#endif
        public CleanupSystemPair[] cleanupSystems;

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
        
        private void OnValueChangedCleanup() {
            if (Application.isPlaying) {
                this.RemoveSystems(this.cleanupSystems);
                this.AddSystems(this.cleanupSystems);
            }
        }

        protected override void OnEnable() {
            if (World.Default != null) {
                this.group = World.Default.CreateSystemsGroup();

                for (int i = 0, length = this.initializers.Length; i < length; i++) {
                    var initializer = this.initializers[i];
                    this.group.AddInitializer(initializer);
                }

                this.AddSystems(this.updateSystems);
                this.AddSystems(this.fixedUpdateSystems);
                this.AddSystems(this.lateUpdateSystems);
                this.AddSystems(this.cleanupSystems);

                World.Default.AddSystemsGroup(this.order, this.group);
            }
        }

        protected override void OnDisable() {
            if (World.Default != null) {
                this.RemoveSystems(this.updateSystems);
                this.RemoveSystems(this.fixedUpdateSystems);
                this.RemoveSystems(this.lateUpdateSystems);
                this.RemoveSystems(this.cleanupSystems);

                World.Default.RemoveSystemsGroup(this.group);
            }
            this.group = null;
        }

        private void AddSystems<T>(BasePair<T>[] pairs) where T : class, ISystem {
            for (int i = 0, length = pairs.Length; i < length; i++) {
                var pair   = pairs[i];
                var system = pair.System;
                pair.group = this.group;
                if (system != null) {
                    this.group.AddSystem(system, pair.Enabled);
                }
                else {
                    this.SystemNullError();
                }
            }
        }

        private void SystemNullError() {
            var go = this.gameObject;
            Debug.LogError($"[MORPEH] System null in installer {go.name} on scene {go.scene.name}", go);
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
        using Systems;
        
        [Serializable]
#if TRI_INSPECTOR
        [TriInspector.DeclareHorizontalGroup("Pair", Sizes = new[] { 15f })]
#endif
        public abstract class BasePair<T> where T : class, ISystem {
            internal SystemsGroup group;
            
            [SerializeField]
#if UNITY_EDITOR
            [HorizontalGroup("Pair", 15)]
            [HideLabel]
            [OnValueChanged(nameof(OnChange))]
#endif
            private bool enabled;

#pragma warning disable CS0649
            [SerializeField]
#if UNITY_EDITOR
            [HorizontalGroup("Pair")]
            [HideLabel]
            [Required]
#endif
            [CanBeNull]
            private T system;
#pragma warning restore CS0649

            public bool Enabled {
                get => this.enabled;
                set => this.enabled = value;
            }

            [CanBeNull]
            public T System {
                get => this.system;
                set => this.system = value;
            }

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
        
        [Serializable]
        public class CleanupSystemPair : BasePair<CleanupSystem> {
        }
    }
}
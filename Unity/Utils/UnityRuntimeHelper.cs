namespace Morpeh {
    using System;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
#if UNITY_EDITOR && ODIN_INSPECTOR
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using Sirenix.Serialization;
    using System.Reflection;
    using Globals.ECS;
#endif
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
#if UNITY_EDITOR && ODIN_INSPECTOR
    internal class UnityRuntimeHelper : SerializedMonoBehaviour {
#else
    internal class UnityRuntimeHelper : MonoBehaviour {
#endif
        internal static Action OnApplicationFocusLost = () => { };
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OdinSerialize]
        private List<World> worldsSerialized = null;
        [OdinSerialize]
        private List<string> types = null;
        private bool hotReloaded = false;
#endif

#if UNITY_EDITOR
        private void OnEnable() {
            EditorApplication.playModeStateChanged += OnEditorApplicationOnplayModeStateChanged;
        }

        private void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredEditMode) {
                for (var i = World.worlds.Count - 1; i >= 0; i--) {
                    var world = World.worlds[i];
                    world?.Dispose();
                }

                World.worlds.Clear();
                World.worlds.Add(null);

                if (this != null && this.gameObject != null) {
                    DestroyImmediate(this.gameObject);
                }

                EditorApplication.playModeStateChanged -= this.OnEditorApplicationOnplayModeStateChanged;
            }
        }
#endif

        private void Update() {
            World.GlobalUpdate(Time.deltaTime);
#if UNITY_EDITOR && ODIN_INSPECTOR
            if (this.hotReloaded) {
                foreach (var world in World.worlds) {
                    for (var index = 0; index < world.entitiesCount; index++) {
                        var entity = world.entities[index];
                        world.Filter.entities.Add(entity.internalID);
                    }
                }

                this.hotReloaded = false;
            }
#endif
        }

        private void FixedUpdate() => World.GlobalFixedUpdate(Time.fixedDeltaTime);
        private void LateUpdate()  => World.GlobalLateUpdate(Time.deltaTime);

        internal void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                OnApplicationFocusLost.Invoke();
                GC.Collect();
            }
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        protected override void OnBeforeSerialize() {
            this.worldsSerialized = World.worlds;
            if (this.types == null) {
                this.types = new List<string>();
            }

            this.types.Clear();
            foreach (var info in CommonCacheTypeIdentifier.editorTypeAssociation.Values) {
                this.types.Add(info.type.AssemblyQualifiedName);
            }
        }


        protected override void OnAfterDeserialize() {
            if (this.worldsSerialized != null) {
                foreach (var t in this.types) {
                    var genType = Type.GetType(t);
                    if (genType != null) {
                        var openGeneric   = typeof(CacheTypeIdentifier<>);
                        var closedGeneric = openGeneric.MakeGenericType(genType);
                        var infoFI        = closedGeneric.GetField("info", BindingFlags.Static | BindingFlags.NonPublic);
                        infoFI.GetValue(null);
                    }
                    else {
                        CommonCacheTypeIdentifier.GetID();
                    }
                }

                foreach (var world in this.worldsSerialized) {
                    world?.Ctor();
                }

                World.worlds = this.worldsSerialized;
                this.hotReloaded = true;
            }
        }
#endif
    }
}
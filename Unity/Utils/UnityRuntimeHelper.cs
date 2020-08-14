namespace Morpeh {
    using System;
    using Collections;
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
    using Utils;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
#if UNITY_EDITOR && ODIN_INSPECTOR
    internal class UnityRuntimeHelper : SerializedMonoBehaviour {
#else
    internal class UnityRuntimeHelper : MonoBehaviour {
#endif
        internal static Action             onApplicationFocusLost = () => { };
        internal static UnityRuntimeHelper instance;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OdinSerialize]
        private FastList<World> worldsSerialized = null;
        [OdinSerialize]
        private FastList<string> types = null;
#endif

#if UNITY_EDITOR
        private void OnEnable() {
            if (instance == null) {
                instance                               =  this;
                EditorApplication.playModeStateChanged += this.OnEditorApplicationOnplayModeStateChanged;
            }
            else {
                Destroy(this);
            }
        }

        private void OnDisable() {
            if (instance == this) {
                instance = null;
            }
        }

        private void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredEditMode) {
                for (var i = World.worlds.length - 1; i >= 0; i--) {
                    var world = World.worlds.data[i];
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

        private void Update() => WorldExtensions.GlobalUpdate(Time.deltaTime);

        private void FixedUpdate() => WorldExtensions.GlobalFixedUpdate(Time.fixedDeltaTime);
        private void LateUpdate()  => WorldExtensions.GlobalLateUpdate(Time.deltaTime);
        
        internal void OnApplicationPause(bool pauseStatus) {
            if (pauseStatus) {
                onApplicationFocusLost.Invoke();
                GC.Collect();
            }
        }   

        internal void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                onApplicationFocusLost.Invoke();
                GC.Collect();
            }
        }

        internal void OnApplicationQuit() {
            onApplicationFocusLost.Invoke();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        protected override void OnBeforeSerialize() {
            this.worldsSerialized = World.worlds;
            if (this.types == null) {
                this.types = new FastList<string>();
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
                    if (world != null && world.entities != null) {
                        for (int i = 0, length = world.entities.Length; i < length; i++) {
                            var e = world.entities[i];
                            if (e == null) {
                                continue;
                            }

                            if (e.componentsIds == null) {
                                world.entities[i] = null;
                            }
                        }

                        world.Ctor();
                    }
                }

                World.worlds = this.worldsSerialized;
            }
        }
#endif
    }
}


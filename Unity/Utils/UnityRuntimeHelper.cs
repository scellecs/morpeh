namespace Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Globals.ECS;
    using Sirenix.Serialization;
    using UnityEditor;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if ODIN_INSPECTOR
    internal class UnityRuntimeHelper : SerializedMonoBehaviour {
#else
    internal class UnityRuntimeHelper : MonoBehaviour, ISerializationCallbackReceiver {
#endif
        internal static Action OnApplicationFocusLost = () => { };

        [OdinSerialize]
        private List<World> worldsSerialized = null;
        [OdinSerialize]
        private List<string> types = null;

        private bool hotReloaded = false;

#if UNITY_EDITOR
        private void OnEnable() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {
                    if (this != null && this.gameObject != null) {
                        DestroyImmediate(this.gameObject);
                    }
                }
            };
        }
#endif


        private void Update() {
            World.PlayerLoopUpdate();
#if UNITY_EDITOR && ODIN_INSPECTOR
            if (this.hotReloaded) {
                foreach (var world in World.Worlds) {
                    for (var index = 0; index < world.EntitiesCount; index++) {
                        var entity = world.Entities[index];
                        world.Filter.Entities.Add(entity.InternalID);
                    }
                }

                this.hotReloaded = false;
            }
#endif
        }

        private void FixedUpdate() => World.PlayerLoopFixedUpdate();
        private void LateUpdate()  => World.PlayerLoopLateUpdate();

        internal void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                OnApplicationFocusLost.Invoke();
                GC.Collect();
            }
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        protected override void OnBeforeSerialize() {
            this.worldsSerialized = World.Worlds;
            if (this.types == null) {
                this.types = new List<string>();
            }

            this.types.Clear();
            foreach (var info in CommonCacheTypeIdentifier.editorTypeAssociation.Values) {
                this.types.Add(info.Type.AssemblyQualifiedName);
            }
        }
#endif


#if UNITY_EDITOR && ODIN_INSPECTOR
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
                    world.Ctor();
                }

                World.Worlds = this.worldsSerialized;
                InitializerECS.Initialize();
                this.hotReloaded = true;
            }
        }
#endif
    }
}
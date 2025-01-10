#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary;
using UnityEngine;
using static Scellecs.Morpeh.WorldBrowser.Serialization.SerializationUtility;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe sealed class UnityObjectEditorAdapter : IContravariantBinaryAdapter<UnityEngine.Object> {
        private readonly List<UnityEngine.Object> createdObjects;

        internal UnityObjectEditorAdapter() {
            this.createdObjects = new List<UnityEngine.Object>();
        }

        public void Serialize(IBinarySerializationContext context, UnityEngine.Object value) {
            context.Writer->Add(-1);
        }

        public object Deserialize(IBinaryDeserializationContext context) {
            var instanceId = context.Reader->ReadNext<int>();
            if (instanceId == -1) {
                return null;
            }

            var name = ReadString(context);
            var typeName = ReadString(context);
            var type = Type.GetType(typeName);

            if (type != null) {
                var debugName = $"({type.Name}):({instanceId}) {name}";
                return CreateFakeUnityObject(type, debugName);
            }

            return null;
        }

        private UnityEngine.Object CreateFakeUnityObject(Type targetType, string name) {
            var scriptable = ScriptableObject.CreateInstance<ScriptableObject>();
            scriptable.name = name;
            var ptr = UnsafeUtility.As<ScriptableObject, IntPtr>(ref scriptable);
            var targetVTable = targetType.TypeHandle.Value;
            *(IntPtr*)ptr = targetVTable;
            this.createdObjects.Add(scriptable);
            return scriptable;
        }

        internal void Cleanup() {
            foreach (var obj in this.createdObjects) {
                if (obj != null) {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            this.createdObjects.Clear();
        }
    }

    internal unsafe sealed class UnityObjectBuildRuntimeAdapter : IContravariantBinaryAdapter<UnityEngine.Object> {
        public void Serialize(IBinarySerializationContext context, UnityEngine.Object value) {
            if (value == null) {
                context.Writer->Add(-1);
                return;
            }
            context.Writer->Add(value.GetInstanceID());
            WriteString(context, value.name);
            WriteString(context, value.GetType().AssemblyQualifiedName);
        }

        public object Deserialize(IBinaryDeserializationContext context) {
            context.Reader->ReadNext<int>();
            return null;
        }
    }
}
#endif

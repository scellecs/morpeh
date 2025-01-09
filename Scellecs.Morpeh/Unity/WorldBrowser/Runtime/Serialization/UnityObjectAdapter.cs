#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary;
using UnityEngine;
using static Scellecs.Morpeh.WorldBrowser.Serialization.UnityObjectAdapterUtility;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe static class UnityObjectAdapterUtility {
        private const int MAX_STRING_LENGTH = 256;

        internal static void WriteString(IBinarySerializationContext context, string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            context.Writer->Add(bytes.Length);
            fixed (byte* ptr = bytes) {
                context.Writer->Add(ptr, bytes.Length);
            }
        }

        internal static string ReadString(IBinaryDeserializationContext context) {
            var length = context.Reader->ReadNext<int>();
            if (length <= 0 || length > MAX_STRING_LENGTH) {
                return string.Empty;
            }
            byte* bytes = (byte*)context.Reader->ReadNext(length);
            return Encoding.UTF8.GetString(bytes, length);
        }
    }

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
            var debugName = $"({type.Name}):({instanceId}) {name}";
            return CreateFakeUnityObject(type, debugName);
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
        private const int MAX_STRING_LENGTH = 512;
        private readonly List<UnityEngine.Object> createdObjects;

        internal UnityObjectBuildRuntimeAdapter() {
            this.createdObjects = new List<UnityEngine.Object>();
        }

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

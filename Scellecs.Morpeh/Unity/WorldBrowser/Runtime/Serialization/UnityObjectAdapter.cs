#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Serialization.Binary;
using UnityEngine;
using static Scellecs.Morpeh.WorldBrowser.Serialization.SerializationUtility;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe sealed class UnityObjectEditorAdapter : IContravariantBinaryAdapter<UnityEngine.Object> {
        private readonly Stack<ScriptableObject> pool;
        private readonly List<ScriptableObject> activeObjects;

        private static FieldInfo unityObjectNativePtr = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.Instance | BindingFlags.NonPublic);

        internal UnityObjectEditorAdapter() {
            this.pool = new Stack<ScriptableObject>(32);
            this.activeObjects = new List<ScriptableObject>();
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
                var debugName = $"[{type.Name}:{instanceId}] {name}";
                return CreateFakeUnityObject(type, debugName);
            }

            return null;
        }

        private UnityEngine.Object CreateFakeUnityObject(Type targetType, string name) {
            var obj = (UnityEngine.Object)RuntimeHelpers.GetUninitializedObject(targetType);
            var scriptable = Rent();
            scriptable.name = name;
            var ptrValue = unityObjectNativePtr.GetValue(scriptable);
            unityObjectNativePtr.SetValue(obj, ptrValue);
            this.activeObjects.Add(scriptable);
            return obj;
        }

        private ScriptableObject Rent() {
            if (this.pool.Count > 0) {
                return this.pool.Pop();
            }
            return ScriptableObject.CreateInstance<ScriptableObject>();
        }

        private void Return(ScriptableObject obj) {
            if (obj != null) {
                obj.name = string.Empty;
                this.pool.Push(obj);
            }
        }

        internal void Refresh() {
            foreach (var obj in this.activeObjects) {
                if (obj != null) {
                    Return(obj);
                }
            }

            this.activeObjects.Clear();
        }

        internal void Cleanup() {
            Refresh();

            while (this.pool.Count > 0) {
                var obj = this.pool.Pop();
                if (obj != null) {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
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
            throw new NotImplementedException(); //BoxedComponentAdapter handle this
        }
    }
}
#endif

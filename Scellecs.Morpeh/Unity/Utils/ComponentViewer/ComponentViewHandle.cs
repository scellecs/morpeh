#if UNITY_EDITOR
using System;
using UnityEngine;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class ComponentViewHandle : IDisposable {
        private ComponentViewWrapper wrapper;
        private UnityEditor.Editor wrapperEditor;

        private ComponentViewHandle(ComponentViewWrapper wrapper, UnityEditor.Editor wrapperEditor) {
            this.wrapper = wrapper;
            this.wrapperEditor = wrapperEditor;
        }

        internal static ComponentViewHandle Create() {
            var gameObject = new GameObject("MORPEH__COMPONENT_VIEWER") { hideFlags = HideFlags.HideAndDontSave };
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            var wrapper = gameObject.AddComponent<ComponentViewWrapper>();
            var wrapperEditor = UnityEditor.Editor.CreateEditor(wrapper);
            return new ComponentViewHandle(wrapper, wrapperEditor);
        }

        internal void HandleOnGUI(ComponentData componentData) {
            this.wrapper.component = componentData;
            this.wrapperEditor.OnInspectorGUI();
        }

        public void Dispose() {
            UnityEngine.Object.DestroyImmediate(this.wrapper);
            this.wrapper = null;

            UnityEngine.Object.DestroyImmediate(this.wrapperEditor);
            this.wrapperEditor = null;
        }
    }
}
#endif
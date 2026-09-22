#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer {
    internal sealed class ComponentViewHandle : IDisposable {
        private ComponentViewWrapper wrapper;
        private UnityEditor.Editor wrapperEditor;

        private ComponentViewHandle(ComponentViewWrapper wrapper, UnityEditor.Editor wrapperEditor) {
            this.wrapper = wrapper;
            this.wrapperEditor = wrapperEditor;
        }

        internal static ComponentViewHandle Create() {
            var wrapper = ScriptableObject.CreateInstance<ComponentViewWrapper>();
            wrapper.hideFlags = HideFlags.HideAndDontSave;
            var wrapperEditor = UnityEditor.Editor.CreateEditor(wrapper);
            return new ComponentViewHandle(wrapper, wrapperEditor);
        }

        internal VisualElement CreateInspector() {
            return this.wrapperEditor.CreateInspectorGUI();
        }

        internal void HandleOnGUI(ComponentData componentData) {
            this.wrapper.component = componentData;
            this.wrapperEditor.OnInspectorGUI();
        }

        internal void SetComponent(ComponentData componentData) {
            this.wrapper.component = componentData;
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser {
    internal static class InspectorModelExtensions {
        internal static SerializableInspectorModel ToSerializable(this InspectorModel model) {
            var components = new ComponentDataBoxed[model.components.Count];
            for (int i = 0; i < model.components.Count; i++) {
                components[i] = model.components[i];
            }

            return new SerializableInspectorModel() {
                version = model.version,
                components = components,
                selectedEntity = model.selectedEntity,
            };
        }

        internal static void FromSerializable(this InspectorModel model, ref SerializableInspectorModel serializable)
        {
            model.components.Clear();
            model.version = serializable.version;
            model.selectedEntity = serializable.selectedEntity;

            for (int i = 0; i < serializable.components.Length; i++) {
                model.components.Add(serializable.components[i]);
            }
        }
    }
}
#endif
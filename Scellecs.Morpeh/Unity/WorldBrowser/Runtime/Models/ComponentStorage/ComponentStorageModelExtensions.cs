#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser {
    internal static class ComponentStorageModelExtensions {
        internal static SerializableComponentStorageModel ToSerializable(this ComponentStorageModel model) {
            var componentNames = new string[model.componentNames.Count];
            model.componentNames.CopyTo(componentNames, 0);

            var keys = new int[model.typeIdToInternalId.Count];
            var values = new int[model.typeIdToInternalId.Count];
            int index = 0;
            foreach (var kvp in model.typeIdToInternalId) {
                keys[index] = kvp.Key;
                values[index] = kvp.Value;
                index++;
            }

            return new SerializableComponentStorageModel() {
                version = model.version,
                componentNames = componentNames,
                keys = keys,
                values = values
            };
        }

        internal static void FromSerializable(this ComponentStorageModel model, ref SerializableComponentStorageModel serializable) {
            model.componentNames.Clear();
            model.typeIdToInternalId.Clear();
            model.version = serializable.version;

            for (int i = 0; i < serializable.componentNames.Length; i++) {
                model.componentNames.Add(serializable.componentNames[i]);
            }

            for (int i = 0; i < serializable.keys.Length; i++) {
                model.typeIdToInternalId[serializable.keys[i]] = serializable.values[i];
            }
        }
    }
}
#endif

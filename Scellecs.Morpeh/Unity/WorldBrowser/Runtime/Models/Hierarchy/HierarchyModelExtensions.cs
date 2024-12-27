#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser {
    internal static class HierarchyModelExtensions {
        internal static SerializableHierarchyModel ToSerializable(this HierarchyModel model) {
            var worldIds = new int[model.worldIds.Count];
            var selectedWorldIds = new int[model.selectedWorldIds.Count];
            var entities = new Entity[model.entities.Count];

            model.worldIds.CopyTo(worldIds, 0);
            model.selectedWorldIds.CopyTo(selectedWorldIds, 0);
            model.entities.CopyTo(entities, 0);

            return new SerializableHierarchyModel() {
                version = model.version,
                worldIds = worldIds,
                selectedWorldIds = selectedWorldIds,
                entities = entities,
            };
        }

        internal static void FromSerializable(this HierarchyModel model, ref SerializableHierarchyModel serializable) {
            model.worldIds.Clear();
            model.selectedWorldIds.Clear();
            model.entities.Clear();

            model.version = serializable.version;

            for (int i = 0; i < serializable.worldIds.Length; i++) {
                model.worldIds.Add(serializable.worldIds[i]);
            }

            for (int i = 0; i < serializable.selectedWorldIds.Length; i++) {
                model.selectedWorldIds.Add(serializable.selectedWorldIds[i]);
            }

            for (int i = 0; i < serializable.entities.Length; i++) {
                model.entities.Add(serializable.entities[i]);
            }
        }
    }
}
#endif

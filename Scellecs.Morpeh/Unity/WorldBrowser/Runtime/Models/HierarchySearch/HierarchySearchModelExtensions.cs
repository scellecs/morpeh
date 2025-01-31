#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser {
    internal static class HierarchySearchModelExtensions {
        internal static SerializableHierarchySearchModel ToSerializable(this HierarchySearchModel model) {
            var withSource = new int[model.withSource.Count];
            var withoutSource = new int[model.withoutSource.Count];
            var includedIds = new int[model.includedIds.Count];
            var excludedIds = new int[model.excludedIds.Count];

            model.withSource.CopyTo(withSource, 0);
            model.withoutSource.CopyTo(withoutSource, 0);
            model.includedIds.CopyTo(includedIds, 0);
            model.excludedIds.CopyTo(excludedIds, 0);

            return new SerializableHierarchySearchModel() {
                version = model.version,
                searchString = model.searchString,
                withSource = withSource,
                withoutSource = withoutSource,
                includedIds = includedIds,
                excludedIds = excludedIds
            };
        }

        internal static void FromSerializable(this HierarchySearchModel model, ref SerializableHierarchySearchModel serializable) {
            model.withSource.Clear();
            model.withoutSource.Clear();
            model.includedIds.Clear();
            model.excludedIds.Clear();

            model.version = serializable.version;
            model.searchString = serializable.searchString;

            for (int i = 0; i < serializable.withSource.Length; i++) {
                model.withSource.Add(serializable.withSource[i]);
            }

            for (int i = 0; i < serializable.withoutSource.Length; i++) {
                model.withoutSource.Add(serializable.withoutSource[i]);
            }

            for (int i = 0; i < serializable.includedIds.Length; i++) {
                model.includedIds.Add(serializable.includedIds[i]);
            }

            for (int i = 0; i < serializable.excludedIds.Length; i++) {
                model.excludedIds.Add(serializable.excludedIds[i]);
            }
        }
    }
}
#endif
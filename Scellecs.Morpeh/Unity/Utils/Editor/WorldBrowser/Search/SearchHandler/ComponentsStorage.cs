#if UNITY_EDITOR
using System;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class ComponentsStorage {
        internal readonly HashSet<Type> componentTypes;
        internal readonly List<string> componentNames;
        internal readonly List<int> componentTypeIds;
        internal readonly List<int> componentIds;
        internal readonly Trie componentsSearchTree;

        internal ComponentsStorage() {
            this.componentTypes = new HashSet<Type>();
            this.componentNames = new List<string>();
            this.componentTypeIds = new List<int>();
            this.componentIds = new List<int>();
            this.componentsSearchTree = new Trie();
        }

        internal bool ValidateUpdateCache() {
            var associations = ComponentId.typeAssociation;
            var update = associations.Count != this.componentTypes.Count;
            if (update) {
                foreach (var kvp in associations) {
                    var type = kvp.Key;
                    if (!this.componentTypes.Contains(type)) {
                        AddComponent(type, kvp.Value.id);
                    }
                }
            }

            return update;
        }

        private void AddComponent(Type type, int typeId) {
            var name = type.Name;
            var id = componentIds.Count;

            this.componentTypes.Add(type);
            this.componentNames.Add(name);
            this.componentTypeIds.Add(typeId);
            this.componentIds.Add(id);
            this.componentsSearchTree.Insert(name, id);
        }
    }
}
#endif

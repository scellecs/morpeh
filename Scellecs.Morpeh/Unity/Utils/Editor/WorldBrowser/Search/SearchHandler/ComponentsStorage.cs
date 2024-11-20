#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class ComponentsStorage {
        internal readonly Dictionary<int, int> typeIdToInternalId;
        internal readonly HashSet<Type> componentTypes;
        internal readonly List<string> componentNames;
        internal readonly List<int> componentTypeIds;
        internal readonly List<int> componentIds;
        internal readonly Trie componentsSearchTree;

        internal ComponentsStorage() {
            this.typeIdToInternalId = new Dictionary<int, int>();
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
            var name = this.GetComponentName(type);
            var id = componentIds.Count;

            this.typeIdToInternalId.Add(typeId, id);
            this.componentTypes.Add(type);
            this.componentNames.Add(name);
            this.componentTypeIds.Add(typeId);
            this.componentIds.Add(id);
            this.componentsSearchTree.Insert(name, id);
        }

        private string GetComponentName(Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }

            var genericTypeName = type.Name;
            var backTickIndex = genericTypeName.IndexOf('`');

            if (backTickIndex <= 0) {
                return genericTypeName;
            }

            var baseTypeLength = backTickIndex;
            var genericArgs = type.GetGenericArguments();
            var totalLength = baseTypeLength + 2;

            for (int i = 0; i < genericArgs.Length; i++) {
                totalLength += GetComponentName(genericArgs[i]).Length;
                if (i < genericArgs.Length - 1)
                    totalLength += 1;
            }

            return string.Create(totalLength, (type, backTickIndex), (span, state) => {
                var (currentType, tickIndex) = state;
                var position = 0;
                var arguments = currentType.GetGenericArguments();

                genericTypeName.AsSpan(0, tickIndex).CopyTo(span);
                position += tickIndex;
                span[position++] = '<';

                for (var i = 0; i < arguments.Length; i++) {
                    if (i > 0) {
                        span[position++] = ',';
                    }
                    var argName = GetComponentName(arguments[i]);
                    argName.AsSpan().CopyTo(span.Slice(position));
                    position += argName.Length;
                }
                span[position] = '>';
            });
        }
    }
}
#endif

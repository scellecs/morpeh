#if UNITY_EDITOR
using System;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class ComponentsStorage {
        private readonly Dictionary<int, int> typeIdToInternalId;
        private readonly HashSet<Type> componentTypes;
        private readonly List<string> componentNames;
        private readonly List<int> componentTypeIds;
        private readonly List<int> componentIds;
        private readonly Trie componentsSearchTree;

        private long version;

        internal ComponentsStorage() {
            this.typeIdToInternalId = new Dictionary<int, int>();
            this.componentTypes = new HashSet<Type>();
            this.componentNames = new List<string>();
            this.componentTypeIds = new List<int>();
            this.componentIds = new List<int>();
            this.componentsSearchTree = new Trie();
        }

        internal string GetComponentNameById(int id) { 
            return this.componentNames[id];
        }

        internal string GetComponentNameByTypeId(int typeId) {
            return this.componentNames[this.typeIdToInternalId[typeId]];
        }

        internal int GetTypeIdByComponentId(int id) { 
            return this.componentTypeIds[id];
        }

        internal List<int> GetComponentIds() {
            return this.componentIds;
        }

        internal List<int> GetComponentIdsMatchesWithPrefix(ReadOnlySpan<char> text, List<int> resultBuffer = null) { 
            resultBuffer ??= new List<int>();
            resultBuffer.Clear();
            return this.componentsSearchTree.GetWordIndicesWithPrefix(text, resultBuffer);
        }

        internal long GetVersion() {
            return this.version;
        }

        internal void ValidateUpdateCache() {
            var associations = ComponentId.typeAssociation;
            if (associations.Count != this.componentTypes.Count) {
                foreach (var kvp in associations) {
                    var type = kvp.Key;
                    if (!this.componentTypes.Contains(type)) {
                        AddComponent(type, kvp.Value.id);
                    }
                }
            }
        }

        private void AddComponent(Type type, int typeId) {
            var name = this.CreateComponentName(type);
            var id = componentIds.Count;
            this.typeIdToInternalId.Add(typeId, id);
            this.componentTypes.Add(type);
            this.componentNames.Add(name);
            this.componentTypeIds.Add(typeId);
            this.componentIds.Add(id);
            this.componentsSearchTree.Insert(name, id);
            this.IncrementVersion();
        }

        private string CreateComponentName(Type type) {
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
                totalLength += CreateComponentName(genericArgs[i]).Length;
                if (i < genericArgs.Length - 1) {
                    totalLength += 1;
                }
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
                    var argName = CreateComponentName(arguments[i]);
                    argName.AsSpan().CopyTo(span.Slice(position));
                    position += argName.Length;
                }
                span[position] = '>';
            });
        }

        private void IncrementVersion() {
            unchecked {
                this.version++;
            }
        }
    }
}
#endif

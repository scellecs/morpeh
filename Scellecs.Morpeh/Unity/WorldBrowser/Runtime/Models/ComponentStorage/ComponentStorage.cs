#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Filter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class ComponentStorage : IComponentStorageProcessor {
        internal readonly ComponentStorageModel model;
        internal readonly HashSet<Type> componentTypes;
        internal readonly List<int> componentTypeIds;
        internal readonly List<int> componentIds;
        internal readonly Trie componentsSearchTree;

        internal ComponentStorage() {
            this.model = new ComponentStorageModel();
            this.model.componentNames = new List<string>();
            this.model.typeIdToInternalId = new Dictionary<int, int>();
            this.componentTypes = new HashSet<Type>();
            this.componentTypeIds = new List<int>();
            this.componentIds = new List<int>();
            this.componentsSearchTree = new Trie();
        }

        public string GetComponentNameById(int id) { 
            return this.model.componentNames[id];
        }

        internal IList<ComponentDataBoxed> FetchEntityComponents(Entity entity, IList<ComponentDataBoxed> buffer) {
            buffer.Clear();
            var handle = new EntityHandle(entity);
            if (!handle.IsValid) {
                return buffer;
            }
            
            var world = handle.World;
            var archetype = handle.Archetype;
            var components = archetype.components;

            foreach (var typeId in components) {
                if (!ComponentId.TryGet(typeId, out var type)) {
                    continue;
                }

                var stash = world.GetReflectionStash(type);
                    
                buffer.Add(new ComponentDataBoxed {
                    data = stash.GetBoxed(entity),
                    typeId = typeId,
                    isMarker = stash is TagStash,
                });
            }

            return buffer;
        }

        public void AddComponentData(int typeId, Entity entity) {
            if (!this.TryGetDefinition(entity, typeId, out var type)) {
                return;
            }
            
            var world = entity.GetWorld();
            var stash = world.GetReflectionStash(type);

            if (stash.Has(entity)) {
                return;
            }
            
            stash.Set(entity);
        }

        public void RemoveComponentData(int typeId, Entity entity) {
            if (!this.TryGetDefinition(entity, typeId, out var type)) {
                return;
            }
            
            var world = entity.GetWorld();
            var stash = world.GetReflectionStash(type);
            
            stash.Remove(entity);
        }

        public void SetComponentData(IComponent data, int typeId, Entity entity) {
            if (!this.TryGetDefinition(entity, typeId, out var def)) {
                return;
            }
            
            var world = entity.GetWorld();
            var stash = world.GetReflectionStash(def);
            
            if (stash is TagStash) {
                return;
            }
            
            stash.SetBoxed(entity, data);
        }

        public ComponentStorageModel GetModel() {
            return this.model;
        }

        public long GetVersion() { 
            return this.model.version;
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

        private bool TryGetDefinition(Entity entity, int typeId, out Type type) {
            type = null;
            
            if (Application.isPlaying == false) {
                return false;
            }

            var isValid = !entity.GetWorld().IsNullOrDisposed() && !entity.GetWorld().IsDisposed(entity);

            if (!isValid) {
                return false;
            }
            
            return ComponentId.TryGet(typeId, out type);
        }

        private void AddComponent(Type type, int typeId) {
            var name = this.CreateComponentName(type);
            var id = componentIds.Count;
            this.model.typeIdToInternalId.Add(typeId, id);
            this.model.componentNames.Add(name);
            this.componentTypes.Add(type);
            this.componentTypeIds.Add(typeId);
            this.componentIds.Add(id);
            this.componentsSearchTree.Insert(name, id);
            this.model.IncrementVersion();
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
    }
}
#endif

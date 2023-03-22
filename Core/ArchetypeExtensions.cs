namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ArchetypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Dispose(this Archetype archetype) {
            archetype.usedInNative = false;
            
            archetype.id      = -1;
            archetype.length  = -1;

            archetype.typeIds = null;
            archetype.world   = null;

            archetype.entities?.Clear();
            archetype.entities = null;
            
            archetype.entitiesNative?.Clear();
            archetype.entitiesNative = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Archetype archetype, Entity entity) {
            archetype.length++;
            
            if (archetype.usedInNative) {
                entity.indexInCurrentArchetype = archetype.entitiesNative.Add(entity.entityId.id);
            }
            else {
                archetype.entities.Set(entity.entityId.id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetype archetype, Entity entity) {
            archetype.length--;

            if (archetype.usedInNative) {
                var index = entity.indexInCurrentArchetype;
                if (archetype.entitiesNative.RemoveAtSwap(index, out int newValue)) {
                    archetype.world.entities[newValue].indexInCurrentArchetype = index;
                }
            }
            else {
                archetype.entities.Unset(entity.entityId.id);
            }
        }
        
        
        internal static unsafe VirtualArchetype AddTransfer(this VirtualArchetype node, int addTypeId, World world) {
            Span<int> ids = stackalloc int[node.level];
            var counter = 0;
            var currentNode = node;
            
            while (currentNode.typeId > addTypeId) {
                ids[counter++] = currentNode.typeId;
                currentNode = currentNode.parent;
            }
            
            var lastNode = currentNode;
            if (!currentNode.map.TryGetValue(addTypeId, out currentNode)) {
                currentNode = new VirtualArchetype {
                    level = lastNode.level + 1,
                    map = new IntHashMap<VirtualArchetype>(),
                    parent = lastNode,
                    typeId = addTypeId
                };
                world.virtualArchetypesCount++;
                lastNode.map.Add(addTypeId, currentNode, out _);
            }
            
            for (int i = counter - 1; i >= 0; i--) {
                var typeId = ids[i];
                lastNode = currentNode;
                if (!lastNode.map.TryGetValue(typeId, out currentNode)) {
                    currentNode = new VirtualArchetype {
                        level = lastNode.level + 1,
                        map = new IntHashMap<VirtualArchetype>(),
                        parent = lastNode,
                        typeId = typeId
                    };
                    world.virtualArchetypesCount++;
                    lastNode.map.Add(typeId, currentNode, out _);
                }
            }
            return currentNode;
        }
        
        internal static unsafe VirtualArchetype RemoveTransfer(this VirtualArchetype node, int removeTypeId, World world) {
            Span<int> ids = stackalloc int[node.level];
            var counter = 0;
            var currentNode = node;
            while (currentNode.typeId > removeTypeId) {
                ids[counter++] = currentNode.typeId;
                currentNode = currentNode.parent;
            }
            currentNode = currentNode.parent;

            for (int i = counter - 1; i >= 0; i--) {
                var typeId = ids[i];
                var lastNode = currentNode;
                if (!lastNode.map.TryGetValue(typeId, out currentNode)) {
                    currentNode = new VirtualArchetype {
                        level = lastNode.level + 1,
                        map = new IntHashMap<VirtualArchetype>(),
                        parent = lastNode,
                        typeId = typeId
                    };
                    world.virtualArchetypesCount++;
                    lastNode.map.Add(typeId, currentNode, out _);
                }
            }
            return currentNode;
        }
    }
}

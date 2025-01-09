namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;
    
    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ExtendedComponentId {
        private static Dictionary<Type, InternalTypeDefinition> typeAssociation = new Dictionary<Type, InternalTypeDefinition>();
        private static Dictionary<int, InternalTypeDefinition> typeIdAssociation = new Dictionary<int, InternalTypeDefinition>();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static InternalTypeDefinition Get<T>() where T : struct, IComponent {
            if (!typeAssociation.TryGetValue(typeof(T), out var typeDefinition)) {
                typeDefinition = Generate<T>();
            }
            
            return typeDefinition;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static InternalTypeDefinition Get(Type type) {
            if (!typeAssociation.TryGetValue(type, out var typeDefinition)) {
                ComponentUnknownException.ThrowTypeNotFound(type);
            }
            
            return typeDefinition;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static InternalTypeDefinition Get(int typeId) {
            if (typeIdAssociation.TryGetValue(typeId, out var typeDefinition)) {
                return typeDefinition;
            }
            
            if (!ComponentId.TryGet(typeId, out var type)) {
                ComponentUnknownException.ThrowTypeIdNotFound(typeId);
            }

            return Get(type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InternalTypeDefinition Generate<T>() where T : struct, IComponent {
            var typeDefinition = new InternalTypeDefinition {
                typeInfo = ComponentId<T>.info,
                type = typeof(T),
                entityAddComponent = (entity) => {
                    var stash = entity.GetWorld().GetStash<T>();
                    if (!stash.Has(entity)) {
                        stash.Add(entity);
                    }
                },
                entityGetComponentBoxed = (entity) => {
                    var stash = entity.GetWorld().GetStash<T>();
                    return stash.Has(entity) ? stash.Get(entity) : default;
                },
                entitySetComponentBoxed = (entity, component) => entity.GetWorld().GetStash<T>().Set(entity, (T)component),
                entityRemoveComponent = (entity) => entity.GetWorld().GetStash<T>().Remove(entity),
                isMarker = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0,
            };

            typeAssociation.Add(typeof(T), typeDefinition);
            typeIdAssociation.Add(typeDefinition.typeInfo.id, typeDefinition);
            
            return typeDefinition;
        }

        internal struct InternalTypeDefinition {
            public TypeInfo typeInfo;
            public Type type;
            public Action<Entity> entityAddComponent;
            public Func<Entity, object> entityGetComponentBoxed;
            public Action<Entity, object> entitySetComponentBoxed;
            public Action<Entity> entityRemoveComponent;
            public bool isMarker;
        }
    }
}
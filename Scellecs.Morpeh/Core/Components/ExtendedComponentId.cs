namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ExtendedComponentId {
        internal static Dictionary<TypeHash, InternalTypeDefinition> typeHashAssociation = new Dictionary<TypeHash, InternalTypeDefinition>();
        internal static Dictionary<int, InternalTypeDefinition> typeIdAssociation = new Dictionary<int, InternalTypeDefinition>();
        internal static Dictionary<Type, InternalTypeDefinition> typeAssociation    = new Dictionary<Type, InternalTypeDefinition>();

        internal static void Add<T>(TypeInfo typeInfo) where T : struct, IComponent {
            var info = new InternalTypeDefinition {
                typeInfo = typeInfo,
                type = typeof(T),
                entityGetComponentBoxed = (entity) => entity.GetWorld().GetStash<T>().Get(entity),
                entitySetComponentBoxed = (entity, component) => entity.GetWorld().GetStash<T>().Set(entity, (T)component),
                entityRemoveComponent = (entity) => entity.GetWorld().GetStash<T>().Remove(entity),
                isMarker = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0,
            };
            
            typeHashAssociation.Add(typeInfo.hash, info);
            typeIdAssociation.Add(typeInfo.id, info);
            typeAssociation.Add(typeof(T), info);
        }

        internal struct InternalTypeDefinition {
            public TypeInfo typeInfo;
            public Type type;
            public Func<Entity, object> entityGetComponentBoxed;
            public Action<Entity, object> entitySetComponentBoxed;
            public Action<Entity> entityRemoveComponent;
            public bool isMarker;
        }
    }
}
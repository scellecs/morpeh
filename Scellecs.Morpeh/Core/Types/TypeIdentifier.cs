#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class TypeIdentifier {
        internal static int counter;

        internal static Dictionary<Type, TypeInfo> typeAssociation = new Dictionary<Type, TypeInfo>();
        
        static TypeIdentifier() {
            counter = 1;
        }

        internal static void InitializeAssociation<T>(TypeInfo typeInfo) where T : struct, IComponent {
            typeAssociation.Add(typeof(T), typeInfo);
        }
    }

    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class ExtendedTypeIdentifier {
        internal static Dictionary<TypeHash, InternalTypeDefinition> typeHashAssociation = new Dictionary<TypeHash, InternalTypeDefinition>();
        internal static Dictionary<int, InternalTypeDefinition> typeIdAssociation = new Dictionary<int, InternalTypeDefinition>();
        internal static Dictionary<Type, InternalTypeDefinition> typeAssociation    = new Dictionary<Type, InternalTypeDefinition>();

        internal static void InitializeAssociation<T>(TypeInfo typeInfo) where T : struct, IComponent {
            var info = new InternalTypeDefinition {
                typeInfo = typeInfo,
                type = typeof(T),
                entityGetComponentBoxed = (entity) => entity.GetWorld().GetStash<T>().Get(entity),
                entitySetComponentBoxed = (entity, component) => entity.GetWorld().GetStash<T>().Set(entity, (T)component),
                entityRemoveComponent   = (entity) => entity.GetWorld().GetStash<T>().Remove(entity),
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

    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class TypeIdentifier<T> where T : struct, IComponent {
        internal static TypeInfo info;
        internal static bool initialized;
        
        static TypeIdentifier() {
            Warmup();
        }

        public static void Warmup() {
            if (initialized) {
                return;
            }
            
            initialized = true;
            
            var typeId = Interlocked.Increment(ref TypeIdentifier.counter);
            var typeHash = Math.Abs(7_777_777_777_777_777_773L * typeId);
            
            info = new TypeInfo(new TypeHash(typeHash), typeId);
            
            TypeIdentifier.InitializeAssociation<T>(info);
            
            // TODO: Required only for Editor
            ExtendedTypeIdentifier.InitializeAssociation<T>(info);
        }
    }
}

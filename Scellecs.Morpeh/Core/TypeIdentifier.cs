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

        internal static Dictionary<TypeId, TypeInfo> idTypeAssociation = new Dictionary<TypeId, TypeInfo>();
        internal static Dictionary<TypeOffset, TypeInfo> offsetTypeAssociation = new Dictionary<TypeOffset, TypeInfo>();
        internal static Dictionary<Type, TypeInfo> typeAssociation = new Dictionary<Type, TypeInfo>();
        
        static TypeIdentifier() {
            counter = 1;
        }

        internal static void InitializeAssociation<T>(TypeInfo typeInfo) where T : struct, IComponent {
            idTypeAssociation.Add(typeInfo.id, typeInfo);
            offsetTypeAssociation.Add(typeInfo.offset, typeInfo);
            typeAssociation.Add(typeof(T), typeInfo);
        }
    }

    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class ExtendedTypeIdentifier {
        internal static Dictionary<long, InternalTypeDefinition> idTypeAssociation = new Dictionary<long, InternalTypeDefinition>();
        internal static Dictionary<long, InternalTypeDefinition> offsetTypeAssociation = new Dictionary<long, InternalTypeDefinition>();
        internal static Dictionary<Type, InternalTypeDefinition> typeAssociation    = new Dictionary<Type, InternalTypeDefinition>();

        internal static void InitializeAssociation<T>(TypeInfo typeInfo) where T : struct, IComponent {
            var info = new InternalTypeDefinition {
                typeInfo = typeInfo,
                type = typeof(T),
                entityGetComponentBoxed = (entity) => entity.world.GetStash<T>().Get(entity),
                entitySetComponentBoxed = (entity, component) => entity.world.GetStash<T>().Set(entity, (T)component),
                entityRemoveComponent   = (entity) => entity.world.GetStash<T>().Remove(entity),
                isMarker = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0,
            };
            
            idTypeAssociation.Add(typeInfo.id.GetValue(), info);
            offsetTypeAssociation.Add(typeInfo.offset.GetValue(), info);
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
            
            var offsetValue = Interlocked.Increment(ref TypeIdentifier.counter);
            var idValue = Math.Abs(7_777_777_777_777_777_773L * offsetValue);
            
            info = new TypeInfo(new TypeOffset(offsetValue), new TypeId(idValue));
            
            TypeIdentifier.InitializeAssociation<T>(info);
            
            // TODO: Required only for Editor
            ExtendedTypeIdentifier.InitializeAssociation<T>(info);
        }
    }
}

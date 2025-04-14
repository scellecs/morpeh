#if UNITY_EDITOR || (DEBUG && !DEVELOPMENT_BUILD) || (MORPEH_REMOTE_BROWSER  && DEVELOPMENT_BUILD)
#define MORPEH_GENERATE_ALL_EXTENDED_IDS
#endif

#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Experimental;
    using System.Reflection;

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ComponentId {
        internal static Dictionary<Type, TypeInfo> typeAssociation = new Dictionary<Type, TypeInfo>();
        internal static Dictionary<int, ChunkComponentTypeInfo> typeAssociationNative = new Dictionary<int, ChunkComponentTypeInfo>();
        internal static Dictionary<int, Type> idAssociation = new Dictionary<int, Type>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Add(Type type, TypeInfo typeInfo) {
            typeAssociation.Add(type, typeInfo);
            idAssociation.Add(typeInfo.id, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddNative(int typeId, ChunkComponentTypeInfo typeInfo) {
            typeAssociationNative.Add(typeId, typeInfo);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeInfo Get(Type type) {
            return typeAssociation[type];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(Type type, out TypeInfo typeInfo) {
            return typeAssociation.TryGetValue(type, out typeInfo);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(int typeId, out Type type) {
            return idAssociation.TryGetValue(typeId, out type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetNative(int typeId, out ChunkComponentTypeInfo typeInfo) { 
            return typeAssociationNative.TryGetValue(typeId, out typeInfo);
        }
    }

    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class ComponentId<T> where T : struct, IComponent {
        internal static TypeInfo info;
        internal static ChunkComponentTypeInfo infoNative;
        internal static bool initialized;

        public static int StashSize;
        
        static ComponentId() {
            Warmup();
        }

        public static void Warmup() {
            if (initialized) {
                return;
            }
            
            initialized = true;

            StashSize = StashConstants.DEFAULT_COMPONENTS_CAPACITY;

            var typeId = ComponentsCounter.Increment();
            var typeHash = Math.Abs(7_777_777_777_777_777_773L * typeId);
            var type = typeof(T);

            info = new TypeInfo(new TypeHash(typeHash), typeId);
            
            ComponentId.Add(type, info);

#if MORPEH_UNITY
            if (type.IsAssignableFrom(typeof(IChunkComponent))) {
                var size = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<T>();

                infoNative = new ChunkComponentTypeInfo() {
                    sizeOf = size,
                    sizeInChunk = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0 ? 0 : size,
                };
            }
#endif

#if MORPEH_GENERATE_ALL_EXTENDED_IDS
            ExtendedComponentId.Generate<T>();
#endif
        }
    }
}
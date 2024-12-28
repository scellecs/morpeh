#if UNITY_EDITOR || (DEBUG && !DEVELOPMENT_BUILD) || (MORPEH_REMOTE_BROWSER  && DEVELOPMENT_BUILD)
#define MORPEH_GENERATE_ALL_EXTENDED_IDS
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ComponentId {
        internal static Dictionary<Type, TypeInfo> typeAssociation = new Dictionary<Type, TypeInfo>();
        internal static Dictionary<int, Type> idAssociation = new Dictionary<int, Type>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Add(Type type, TypeInfo typeInfo) {
            typeAssociation.Add(type, typeInfo);
            idAssociation.Add(typeInfo.id, type);
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
    }

    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class ComponentId<T> where T : struct, IComponent {
        internal static TypeInfo info;
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
            
            info = new TypeInfo(new TypeHash(typeHash), typeId);
            
            ComponentId.Add(typeof(T), info);

#if MORPEH_GENERATE_ALL_EXTENDED_IDS
            ExtendedComponentId.Generate<T>();
#endif
        }
    }
}
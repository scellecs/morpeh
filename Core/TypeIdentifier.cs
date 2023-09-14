#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class CommonTypeIdentifier {
        internal static int counter;

        internal static Dictionary<long, InternalTypeDefinition>  idTypeAssociation = new Dictionary<long, InternalTypeDefinition>();
        internal static Dictionary<long, InternalTypeDefinition>  offsetTypeAssociation = new Dictionary<long, InternalTypeDefinition>();
        internal static Dictionary<Type, InternalTypeDefinition> typeAssociation    = new Dictionary<Type, InternalTypeDefinition>();
        
        static CommonTypeIdentifier() {
            counter = 1;
        }

        internal static InternalTypeDefinition Get(Type type) {
            if (typeAssociation.TryGetValue(type, out var definition) == false) {
                Warmup(type);
                definition = typeAssociation[type];
            }
            return definition;
        }

        private static void Warmup(Type type) {
            try {
                var typeId = typeof(TypeIdentifier<>).MakeGenericType(type);
                var warm = typeId.GetMethod("Warmup", BindingFlags.Static | BindingFlags.Public);
                warm.Invoke(null, null);
            }
            catch {
                MLogger.LogError($"[MORPEH] For using {type.Name} you must warmup it or IL2CPP will strip it from the build.\nCall <b>TypeIdentifier<{type.Name}>.Warmup();</b> before access this UniversalProvider.");
            }
        }

        
        #pragma warning disable 0612
        internal static void GetID<T>(out long id, out int offset) where T : struct, IComponent {
            offset = Interlocked.Increment(ref counter);
            id   = Math.Abs(7_777_777_777_777_777_773L * offset);
            var type = typeof(T);

            var info = new InternalTypeDefinition {
                id                      = id,
                offset                  = offset,
                type                    = type,
                entityGetComponentBoxed = (entity) => entity.world.GetStash<T>().Get(entity),
                entitySetComponentBoxed = (entity, component) => entity.world.GetStash<T>().Set(entity, (T)component),
                entityRemoveComponent   = (entity) => entity.world.GetStash<T>().Remove(entity),
                typeInfo                = TypeIdentifier<T>.info
            };
            idTypeAssociation.Add(id, info);
            offsetTypeAssociation.Add(offset, info);
            typeAssociation.Add(type, info);
        }
        #pragma warning restore 0612

        internal struct InternalTypeDefinition {
            public long                   id;
            public int                    offset;
            public Type                   type;
            public Func<Entity, object>   entityGetComponentBoxed;
            public Action<Entity, object> entitySetComponentBoxed;
            public Action<Entity>         entityRemoveComponent;
            public TypeInfo               typeInfo;
        }

        internal class TypeInfo {
            internal long id;
            internal int offset;
            internal bool isMarker;
            internal int stashSize;

            public TypeInfo(bool isMarker, int stashSize) {
                this.isMarker  = isMarker;
                this.stashSize = stashSize;
            }

            public void SetID(long id, int offset) {
                this.id = id;
                this.offset = offset;
            }
        }
    }

    [Il2CppEagerStaticClassConstruction]
    [UnityEngine.Scripting.Preserve]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class TypeIdentifier<T> where T : struct, IComponent {
        internal static CommonTypeIdentifier.TypeInfo info;
        
        static TypeIdentifier() {
            Warmup();
        }

        public static void Warmup() {
            if (info != null) {
                return;
            }

            var type = typeof(T);

            var stashSize  = Constants.DEFAULT_STASH_COMPONENTS_CAPACITY;
            var attributes = type.GetCustomAttributes(typeof(StashSizeAttribute)).ToArray();
            if (attributes.Length > 0) {
                var att = (StashSizeAttribute)attributes[0];
                stashSize = att.size;
            }

            var typeFieldsLength = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length;
            info = new CommonTypeIdentifier.TypeInfo(typeFieldsLength == 0, stashSize);
            CommonTypeIdentifier.GetID<T>(out var id, out var offset);
            info.SetID(id, offset);
        }
    }
}

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

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class CommonTypeIdentifier {
        private static int counter;

        internal static Dictionary<int, InternalTypeDefinition>  intTypeAssociation = new Dictionary<int, InternalTypeDefinition>();
        internal static Dictionary<Type, InternalTypeDefinition> typeAssociation    = new Dictionary<Type, InternalTypeDefinition>();

        #pragma warning disable 0612
        internal static int GetID<T>() where T : struct, IComponent {
            var id   = Interlocked.Increment(ref counter);
            var type = typeof(T);

            var info = new InternalTypeDefinition {
                id                      = id,
                type                    = type,
                entityGetComponentBoxed = (entity) => entity.world.GetStash<T>().Get(entity),
                entitySetComponentBoxed = (entity, component) => entity.world.GetStash<T>().Set(entity, (T)component),
                entityRemoveComponent   = (entity) => entity.world.GetStash<T>().Remove(entity),
                typeInfo                = TypeIdentifier<T>.info
            };
            intTypeAssociation.Add(id, info);
            typeAssociation.Add(type, info);
            return id;
        }
        #pragma warning restore 0612

        internal struct InternalTypeDefinition {
            public int                    id;
            public Type                   type;
            public Func<Entity, object>   entityGetComponentBoxed;
            public Action<Entity, object> entitySetComponentBoxed;
            public Action<Entity>         entityRemoveComponent;
            public TypeInfo               typeInfo;
        }

        [Serializable]
        internal class TypeInfo {
            [SerializeField]
            internal int id;
            [SerializeField]
            internal bool isMarker;
            [SerializeField]
            internal int stashSize;

            public TypeInfo(bool isMarker, int stashSize) {
                this.isMarker  = isMarker;
                this.stashSize = stashSize;
            }

            public void SetID(int id) {
                this.id = id;
            }
        }
    }

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
            var id = CommonTypeIdentifier.GetID<T>();
            info.SetID(id);
        }
    }
}

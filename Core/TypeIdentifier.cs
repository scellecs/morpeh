namespace Morpeh {
    using System;
    using System.Reflection;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine.Scripting;
    
    [Preserve]
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

            var typeFieldsLength = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length;
            info = new CommonTypeIdentifier.TypeInfo(typeFieldsLength == 0, typeof(IDisposable).IsAssignableFrom(typeof(T)));
            var id = CommonTypeIdentifier.GetID<T>();
            info.SetID(id);
        }
    }
}

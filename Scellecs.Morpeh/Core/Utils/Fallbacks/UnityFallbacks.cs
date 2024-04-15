#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

namespace Unity.IL2CPP.CompilerServices {
    using System;

#if !EXTERNAL_IL2CPP_ATTRS
    public enum Option {
#else
    internal enum Option {
#endif
        NullChecks = 1,
        ArrayBoundsChecks = 2,
        DivideByZeroChecks = 3
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
#if !MORPEH_EXTERNAL_IL2CPP_ATTRS
    public class Il2CppSetOptionAttribute : Attribute {
#else
    internal class Il2CppSetOptionAttribute : Attribute {
#endif
        public Option Option { get; }
        public object Value  { get; }

        public Il2CppSetOptionAttribute(Option option, object value) {
            this.Option = option;
            this.Value  = value;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
#if !MORPEH_EXTERNAL_IL2CPP_ATTRS
    public class Il2CppEagerStaticClassConstructionAttribute : Attribute
#else
    internal class Il2CppEagerStaticClassConstructionAttribute : Attribute {
#endif
    {
    }
}

namespace Unity.Collections.LowLevel.Unsafe {
#if !MORPEH_UNITY
    using System;
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NativeDisableUnsafePtrRestrictionAttributeAttribute : Attribute { }
    
    public sealed class NativeDisableUnsafePtrRestrictionAttribute : System.Attribute { }
#endif
}

namespace UnityEngine {
#if !MORPEH_UNITY
    public sealed class SerializeField : System.Attribute { }
    public sealed class GameObject : System.Object { }
#endif
}

namespace UnityEngine.Scripting {
#if !MORPEH_UNITY
    public sealed class Preserve : System.Attribute { }
#endif
}

namespace Unity.Collections {
#if !MORPEH_UNITY
    using System;
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class ReadOnlyAttribute : System.Attribute { }
#endif
}

namespace Scellecs.Morpeh {
    
}
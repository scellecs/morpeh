namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

public class SomeGenericClass<T> {
    
}

[GenericInjectionProvider(typeof(SomeGenericClass<>))]
public class TestGenericResolver {
    public SomeGenericClass<T> Provide<T>() {
        return new SomeGenericClass<T>();
    }
}

public class SomeComplexGenericClass<T1, T2> {
    
}

[GenericInjectionProvider(typeof(SomeComplexGenericClass<,>))]
public class TestComplexGenericResolver {
    public SomeComplexGenericClass<T1, T2> Provide<T1, T2>() {
        return new SomeComplexGenericClass<T1, T2>();
    }
}
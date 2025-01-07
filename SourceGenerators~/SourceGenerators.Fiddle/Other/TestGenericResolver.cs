namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

public class SomeGenericClass<T> {
    
}

[GenericInjectionResolver(typeof(SomeGenericClass<>))]
public class TestGenericResolver {
    public SomeGenericClass<T> Resolve<T>() {
        return new SomeGenericClass<T>();
    }
}

public class SomeComplexGenericClass<T1, T2> {
    
}

[GenericInjectionResolver(typeof(SomeComplexGenericClass<,>))]
public class TestComplexGenericResolver {
    public SomeComplexGenericClass<T1, T2> Resolve<T1, T2>() {
        return new SomeComplexGenericClass<T1, T2>();
    }
}
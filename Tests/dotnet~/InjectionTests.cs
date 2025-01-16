namespace Tests;

using Scellecs.Morpeh;
using Xunit.Abstractions;

[Collection("Sequential")]
public class InjectionTests {
    private readonly ITestOutputHelper output;
    private readonly InjectionTable    injectionTable;
    
    public InjectionTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
        
        this.injectionTable = new InjectionTable();
        this.injectionTable.Register(new BasicDisposableClass(), typeof(IDisposable));
    }

    [Fact]
    public void Get_ExistingResolves() {
        var instance = this.injectionTable.Get(typeof(IDisposable));
        Assert.NotNull(instance);
    }
    
    [Fact]
    public void Get_NonExistingThrows() {
        Assert.Throws<InvalidOperationException>(() => this.injectionTable.Get(typeof(BasicDisposableClass)));
    }
    
    [Fact]
    public void Register_ExistingThrows() {
        Assert.Throws<InvalidOperationException>(() => this.injectionTable.Register(new BasicDisposableClass(), typeof(IDisposable)));
    }
    
    [Fact]
    public void Register_NonExistingRegisters() {
        this.injectionTable.Register(new BasicDisposableClass());
        var instance = this.injectionTable.Get(typeof(BasicDisposableClass));
        Assert.NotNull(instance);
    }

    [Fact]
    public void Register_TwiceThrows() {
        Assert.Throws<InvalidOperationException>(() => this.injectionTable.Register(new BasicDisposableClass(), typeof(IDisposable)));
    }

    [Fact]
    public void UnRegister_ExistingUnregisters() {
        this.injectionTable.UnRegister(typeof(IDisposable));
        Assert.Throws<InvalidOperationException>(() => this.injectionTable.Get(typeof(IDisposable)));
    }
    
    [Fact]
    public void UnRegister_NonExistingIsSilent() {
        this.injectionTable.UnRegister(typeof(BasicDisposableClass));
    }

    [Fact]
    public void MultipleFields_InjectsCorrectly() => this.injectionTable.New<MultipleFieldsBaseClass>().Validate();
    
    [Fact]
    public void ParentClass_InjectsIncludingChildrenFields() => this.injectionTable.New<ParentClass>().Validate();

    [Fact]
    public void InjectableNonMiddlewareClass_InjectsIncludingChildrenFields() => this.injectionTable.New<NonInjectableMiddlewareClass>().Validate();

    [Fact]
    public void InjectableMiddlewareClass_InjectsIncludingChildrenFields() => this.injectionTable.New<InjectableMiddlewareClass>().Validate();
}

public class BasicDisposableClass : IDisposable {
    public void Dispose() {
    }
}

[Injectable]
public partial class MultipleFieldsBaseClass {
    [Injectable]
    private IDisposable disposable1;
    
    [Injectable]
    private IDisposable disposable2;
    
    public virtual void Validate() {
        Assert.NotNull(this.disposable1);
        Assert.NotNull(this.disposable2);
    }
}

[Injectable]
public partial class ParentClass : MultipleFieldsBaseClass {
    [Injectable]
    private IDisposable disposable3;
    
    [Injectable]
    private IDisposable disposable4;
    
    public override void Validate() {
        base.Validate();
        
        Assert.NotNull(this.disposable3);
        Assert.NotNull(this.disposable4);
    }
}

public class NonInjectableMiddlewareClass : ParentClass {
    
}

[Injectable]
public partial class InjectableMiddlewareClass : NonInjectableMiddlewareClass {
    [Injectable]
    private IDisposable disposable5;
    
    [Injectable]
    private IDisposable disposable6;
    
    public override void Validate() {
        base.Validate();
        
        Assert.NotNull(this.disposable5);
        Assert.NotNull(this.disposable6);
    }
}
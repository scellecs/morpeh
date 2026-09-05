// ReSharper disable once CheckNamespace
namespace Test.Namespace;

using Scellecs.Morpeh;
using UnityEngine;

[EcsComponent(initialCapacity: 64)]
public partial struct DisposableComponent : IValidatable, IValidatableWithGameObject {
    public int value;

    public void OnValidate() {
        
    }
    
    public void OnValidate(GameObject gameObject) {
        
    }
    
    public void Dispose() {
        throw new NotImplementedException();
    }
}

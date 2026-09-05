using Scellecs.Morpeh;
using UnityEngine;

[EcsComponent]
// ReSharper disable once CheckNamespace
public partial struct GlobalNamespaceComponent : IValidatableWithGameObject {
    public void OnValidate(GameObject gameObject) {
        throw new NotImplementedException();
    }
}
namespace Scellecs.Morpeh {
    using UnityEngine;
    
    public interface IValidatableWithGameObject {
        void OnValidate(GameObject gameObject);
    }
}
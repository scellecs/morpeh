namespace Scellecs.Morpeh {
    using System;
    using UnityEngine;
    
    [Obsolete("Use [MonoProvider] attribute for providers and implement OnValidate method instead.")]
    public interface IValidatableWithGameObject {
        void OnValidate(GameObject gameObject);
    }
}
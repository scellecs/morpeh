namespace Scellecs.Morpeh {
    using System;

    [Obsolete("Use [MonoProvider] attribute for providers and implement OnValidate method instead.")]
    public interface IValidatable {
        void OnValidate();
    }
}
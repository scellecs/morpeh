namespace Morpeh {
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public class UniversalProvider : EntityProvider {
        [Space]
        [SerializeReference]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideIf(nameof(ShowSerializedComponents))]
#endif
        public IComponent[] serializedComponents = new IComponent[0];
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        private bool ShowSerializedComponents => this.internalEntityID > -1;
#endif
        
        protected virtual void OnValidate() {
            foreach (var component in this.serializedComponents) {
                if (component is IValidatable validatable) {
                    validatable.OnValidate();
                }
                if (component is IValidatableWithGameObject validatableWithGameObject) {
                    validatableWithGameObject.OnValidate(this.gameObject);
                }
            }
        }
        protected sealed override void PreInitialize() {
            var ent = this.Entity;
            if (ent != null && !ent.isDisposed) {
                foreach (var component in this.serializedComponents) {
                    var type = component.GetType();
                    if (CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                        definition.entitySetComponentBoxed(ent, component);
                    }
                    else {
                        Debug.LogError(
                            $"[MORPEH] For using {type.Name} in a UniversalProvider you must warmup it or IL2CPP will strip it from the build.\nCall <b>TypeIdentifier<{type.Name}>.Warmup();</b> before access this UniversalProvider.");
                    }
                }
            }
        }

        protected override void OnDisable() {
            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                foreach (var component in this.serializedComponents) {
                    var type = component.GetType();
                    if (CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                        definition.entityRemoveComponent(ent);
                    }
                    else {
                        Debug.LogError(
                            $"[MORPEH] For using {type.Name} in a UniversalProvider you must warmup it or IL2CPP will strip it from the build.\nCall <b>TypeIdentifier<{type.Name}>.Warmup();</b> before access this UniversalProvider.");
                    }
                }
            }
            base.OnDisable();
        }
    }
}
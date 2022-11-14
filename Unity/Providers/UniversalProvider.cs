namespace Scellecs.Morpeh.Providers {
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public class UniversalProvider : EntityProvider {
        private static TypeComponentEqualityComparer comparer = new TypeComponentEqualityComparer();
        
        [Space]
        [SerializeReference]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideIf(nameof(ShowSerializedComponents))]
#endif
        public IComponent[] serializedComponents = new IComponent[0];
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        private bool ShowSerializedComponents => this.Entity.IsNullOrDisposed() == false;
#endif

        protected virtual void OnValidate() {
            for (var i = 0; i < this.serializedComponents.Length; i++) {
                var component = this.serializedComponents[i];
                if (component is IValidatable validatable) {
                    validatable.OnValidate();
                    this.serializedComponents[i] = (IComponent) validatable;
                }

                if (component is IValidatableWithGameObject validatableWithGameObject) {
                    validatableWithGameObject.OnValidate(this.gameObject);
                    this.serializedComponents[i] = (IComponent) validatableWithGameObject;
                }
            }

            this.serializedComponents = this.serializedComponents.Distinct(comparer).ToArray();
        }
        protected sealed override void PreInitialize() {
            var entity = this.Entity;
            if (entity.IsNullOrDisposed() == false) {
                foreach (var component in this.serializedComponents) {
                    var type = component.GetType();
                    if (CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                        definition.entitySetComponentBoxed(entity, component);
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
        
        private class TypeComponentEqualityComparer : IEqualityComparer<IComponent> {
            public bool Equals(IComponent x, IComponent y) => x != null && y != null && x.GetType() == y.GetType();

            public int GetHashCode(IComponent obj) => obj.GetType().GetHashCode();
        }
    }
}
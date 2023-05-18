namespace Scellecs.Morpeh.Providers {
#if UNITY_EDITOR
    using Sirenix.OdinInspector;
#endif
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class UniversalProvider : EntityProvider {
        private static TypeComponentEqualityComparer comparer = new TypeComponentEqualityComparer();
        
        [Space]
        [SerializeReference]
#if UNITY_EDITOR
        [HideIf(nameof(ShowSerializedComponents))]
#endif
        public IComponent[] serializedComponents = new IComponent[0];
        
#if UNITY_EDITOR
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
                    var definition = CommonTypeIdentifier.Get(type);
                    definition.entitySetComponentBoxed(entity, component);
                }
            }
        }

        protected override void PreDeinitialize() {
            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                foreach (var component in this.serializedComponents) {
                    var type = component.GetType();
                    var definition = CommonTypeIdentifier.Get(type);
                    definition.entityRemoveComponent(ent);
                }
            }
        }
        
        private class TypeComponentEqualityComparer : IEqualityComparer<IComponent> {
            public bool Equals(IComponent x, IComponent y) => x != null && y != null && x.GetType() == y.GetType();

            public int GetHashCode(IComponent obj) => obj.GetType().GetHashCode();
        }
    }
}
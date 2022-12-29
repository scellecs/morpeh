namespace Scellecs.Morpeh.Providers {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public abstract class MonoProvider<T> : EntityProvider where T : struct, IComponent {
        [SerializeField]
        [HideInInspector]
        private T serializedData;
        private Stash<T> stash;
#if UNITY_EDITOR && ODIN_INSPECTOR
        private string typeName = typeof(T).Name;

        [PropertySpace]
        [ShowInInspector]
        [PropertyOrder(1)]
        [HideLabel]
        [InlineProperty]
#endif
        private T Data {
            get {
                if (this.Entity.IsNullOrDisposed() == false) {
                    return this.Stash.Get(this.Entity);
                }

                return this.serializedData;
            }
            set {
                if (this.Entity.IsNullOrDisposed() == false) {
                    this.Stash.Set(this.Entity, value);
                }
                else {
                    this.serializedData = value;
                }
            }
        }

        public Stash<T> Stash {
            get {
                if (this.stash == null) {
                    this.stash = World.Default.GetStash<T>();
                }
                return this.stash;
            }
        }

        public ref T GetSerializedData() => ref this.serializedData;

        public ref T GetData() {
            if (this.Entity.IsNullOrDisposed() == false) {
                if (this.Stash.Has(this.Entity)) {
                    return ref this.Stash.Get(this.Entity);
                }
            }

            return ref this.serializedData;
        }

        public ref T GetData(out bool existOnEntity) {
            if (this.Entity.IsNullOrDisposed() == false) {
                return ref this.Stash.Get(this.Entity, out existOnEntity);
            }

            existOnEntity = false;
            return ref this.serializedData;
        }
        
        protected virtual void OnValidate() {
            if (this.serializedData is IValidatable validatable) {
                validatable.OnValidate();
                this.serializedData = (T)validatable;
            }
            if (this.serializedData is IValidatableWithGameObject validatableWithGameObject) {
                validatableWithGameObject.OnValidate(this.gameObject);
                this.serializedData = (T)validatableWithGameObject;
            }
        }

        protected sealed override void PreInitialize() {
            this.Stash.Set(this.Entity, this.serializedData);
        }

        protected override void OnDisable() {
            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                this.Stash.Remove(ent);
            }
            base.OnDisable();
        }
    }
}
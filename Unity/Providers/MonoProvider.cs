namespace Scellecs.Morpeh.Providers {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public abstract class MonoProvider<T> : EntityProvider where T : struct, IComponent {
        [SerializeField]
        [HideInInspector]
        private T serializedData;
        private Stash<T> stash;
#if UNITY_EDITOR
        private string typeName = typeof(T).Name;

        [PropertySpace]
        [ShowInInspector]
        [PropertyOrder(1)]
        [HideLabel]
        [InlineProperty]
#endif
        private T Data {
            get {
                if (this.cachedEntity.IsNullOrDisposed() == false) {
                    var data = this.Stash.Get(this.cachedEntity, out var exist);
                    if (exist) {
                        return data;
                    }
                }

                return this.serializedData;
            }
            set {
                if (this.cachedEntity.IsNullOrDisposed() == false) {
                    this.Stash.Set(this.cachedEntity, value);
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
            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                if (this.Stash.Has(ent)) {
                    return ref this.Stash.Get(ent);
                }
            }

            return ref this.serializedData;
        }

        public ref T GetData(out bool existOnEntity) {
            if (this.cachedEntity.IsNullOrDisposed() == false) {
                return ref this.Stash.Get(this.cachedEntity, out existOnEntity);
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
            this.Stash.Set(this.cachedEntity, this.serializedData);
        }

        protected sealed override void PreDeinitialize() {
            var ent = this.cachedEntity;
            if (ent.IsNullOrDisposed() == false) {
                this.Stash.Remove(ent);
            }
        }
    }
}
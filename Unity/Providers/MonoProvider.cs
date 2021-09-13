namespace Morpeh {
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class MonoProvider<T> : EntityProvider where T : struct, IComponent {
        [SerializeField]
        [HideInInspector]
        private T serializedData;
        private ComponentsCache<T> cache;
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
                if (this.Entity != null) {
                    return this.cache.GetComponent(this.Entity);
                }

                return this.serializedData;
            }
            set {
                if (this.Entity != null) {
                    this.cache.SetComponent(this.Entity, value);
                }
                else {
                    this.serializedData = value;
                }
            }
        }
        
        public ref T GetSerializedData() => ref this.serializedData;

        public ref T GetData() {
            if (this.Entity != null) {
                if (this.cache.Has(this.Entity)) {
                    return ref this.cache.GetComponent(this.Entity);
                }
            }

            return ref this.serializedData;
        }

        public ref T GetData(out bool existOnEntity) {
            if (this.Entity != null) {
                return ref this.cache.TryGetComponent(this.Entity, out existOnEntity);
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
            this.cache = World.Default.GetCache<T>();

            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                this.cache.SetComponent(ent, this.serializedData);
            }
        }

        protected override void OnDisable() {
            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                this.cache.RemoveComponent(ent);
            }
            base.OnDisable();
        }
    }
}
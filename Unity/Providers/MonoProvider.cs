namespace Morpeh {
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class MonoProvider<T> : EntityProvider where T : struct, IComponent {
        [SerializeField]
        [HideInInspector]
        private T serializedData;
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
                    return this.Entity.GetComponent<T>(out _);
                }

                return this.serializedData;
            }
            set {
                if (this.Entity != null) {
                    this.Entity.SetComponent(value);
                }
                else {
                    this.serializedData = value;
                }
            }
        }
        
        public ref T GetSerializedData() => ref this.serializedData;

        public ref T GetData() {
            if (this.Entity != null) {
                if (this.Entity.Has<T>()) {
                    return ref this.Entity.GetComponent<T>();
                }
            }

            return ref this.serializedData;
        }

        public ref T GetData(out bool existOnEntity) {
            if (this.Entity != null) {
                return ref this.Entity.GetComponent<T>(out existOnEntity);
            }

            existOnEntity = false;
            return ref this.serializedData;
        }

        protected sealed override void PreInitialize() {
            var ent = this.Entity;
            if (ent != null && !ent.isDisposed) {
                ent.SetComponent(this.serializedData);
            }
        }

        protected override void OnDisable() {
            var ent = this.Entity;
            if (ent.IsNullOrDisposed() == false) {
                ent.RemoveComponent<T>();
            }
            base.OnDisable();
        }
    }
}
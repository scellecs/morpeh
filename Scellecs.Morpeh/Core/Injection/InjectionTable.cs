namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public sealed class InjectionTable {
        internal readonly Dictionary<Type, object> mapping = new Dictionary<Type, object>();

        public InjectionTable() {
            this.Register(this);
        }

        public object Get(Type type) {
            if (this.mapping.TryGetValue(type, out var instance)) {
                return instance;
            }

            this.ThrowUnknownRequirement(type);
            return null;
        }
        
        public T Get<T>() where T : class {
            return (T)this.Get(typeof(T));
        }

        public void Register<T>(T obj) where T : class {
            var type = typeof(T);

            if (!this.mapping.TryAdd(type, obj)) {
                this.ThrowRequirementExists(type);
            }
        }

        public void Register(object obj, Type type) {
            if (!this.mapping.TryAdd(type, obj)) {
                this.ThrowRequirementExists(type);
            }
        }

        public void UnRegister<T>(T obj) where T : class {
            this.mapping.Remove(typeof(T));
        }

        public void UnRegister(Type type) {
            this.mapping.Remove(type);
        }

        public T New<T>() where T : class, new() {
            var obj = new T();
            
            if (obj is IInjectable injectable) {
                injectable.Inject(this);
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowUnknownRequirement(Type type) {
            throw new InvalidOperationException($"InjectionTable: Unknown dependency {type}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowRequirementExists(Type type) {
            throw new InvalidOperationException($"InjectionTable: {type} already exists");
        }
    }
}
namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct FilterBuilder {
        internal World    world;
        internal TypeHash includeHash;
        internal TypeHash excludeHash;
        internal Context  context;
        internal int      contextVersion;

        internal static FilterBuilder Create(World world) {
            var ctx = ContextPool.Get();
            
            return new FilterBuilder {
                world          = world,
                includeHash    = default,
                excludeHash    = default,
                context        = ctx,
                contextVersion = ctx.version,
            };
        }
        
        public FilterBuilder With<T>() where T : struct, IComponent {
            if (this.contextVersion != this.context.version) {
                FilterBuilderReuseException.Throw();
            }
            
            var info = ComponentId<T>.info;
            
            for (var i = this.context.withCount - 1; i >= 0; i--) {
                if (this.context.with[i] == info.id) {
                    ComponentExistsInFilterException.Throw(typeof(T));
                }
            }
            
            for (var i = this.context.withoutCount - 1; i >= 0; i--) {
                if (this.context.without[i] == info.id) {
                    ComponentExistsInFilterException.Throw(typeof(T));
                }
            }

            if (this.context.withCount == this.context.with.Length) {
                ArrayHelpers.Grow(ref this.context.with, this.context.with.Length << 1);
            }
            
            this.context.with[this.context.withCount++] = info.id;
            unchecked { this.context.version++; }

            return new FilterBuilder {
                world          = this.world,
                includeHash    = this.includeHash.Combine(info.hash),
                excludeHash    = this.excludeHash,
                context        = this.context,
                contextVersion = this.context.version,
            };
        }

        public FilterBuilder Without<T>() where T : struct, IComponent {
            if (this.contextVersion != this.context.version) {
                FilterBuilderReuseException.Throw();
            }
            
            var info = ComponentId<T>.info;
            
            for (var i = this.context.withCount - 1; i >= 0; i--) {
                if (this.context.with[i] == info.id) {
                    ComponentExistsInFilterException.Throw(typeof(T));
                }
            }
            
            for (var i = this.context.withoutCount - 1; i >= 0; i--) {
                if (this.context.without[i] == info.id) {
                    ComponentExistsInFilterException.Throw(typeof(T));
                }
            }

            if (this.context.withoutCount == this.context.without.Length) {
                ArrayHelpers.Grow(ref this.context.without, this.context.without.Length << 1);
            }
            
            this.context.without[this.context.withoutCount++] = info.id;
            unchecked { this.context.version++; }
            
            return new FilterBuilder {
                world          = this.world,
                includeHash    = this.includeHash,
                excludeHash    = this.excludeHash.Combine(info.hash),
                context        = this.context,
                contextVersion = this.context.version,
            };
        }
        
        public FilterBuilder Copy() {
            if (this.contextVersion != this.context.version) {
                FilterBuilderReuseException.Throw();
            }
            
            var ctx = ContextPool.Get();
            
            if (this.context.withCount > ctx.with.Length) {
                ArrayHelpers.Grow(ref ctx.with, this.context.withCount);
            }
            Array.Copy(this.context.with, ctx.with, this.context.withCount);
            ctx.withCount = this.context.withCount;
            
            if (this.context.withoutCount > ctx.without.Length) {
                Array.Resize(ref ctx.without, this.context.withoutCount);
            }
            Array.Copy(this.context.without, ctx.without, this.context.withoutCount);
            ctx.withoutCount = this.context.withoutCount;
            
            return new FilterBuilder {
                world          = this.world,
                includeHash    = this.includeHash,
                excludeHash    = this.excludeHash,
                context        = ctx,
                contextVersion = ctx.version,
            };
        }

        public FilterBuilder Extend<T>() where T : struct, IFilterExtension {
            var newFilter = default(T).Extend(this);
            return newFilter;
        }

        public Filter Build() {
            if (this.contextVersion != this.context.version) {
                FilterBuilderReuseException.Throw();
            }
            
            Filter filter;
            
            var lookup = this.world.filtersLookup;

            if (lookup.TryGetValue(this.includeHash.GetValue(), out var excludeMap)) {
                if (!excludeMap.TryGetValue(this.excludeHash.GetValue(), out filter)) {
                    filter = this.CompleteBuild();
                    excludeMap.Add(this.excludeHash.GetValue(), filter, out _);
                }
            } else {
                filter = this.CompleteBuild();
                var newMap = new LongHashMap<Filter>();
                newMap.Add(this.excludeHash.GetValue(), filter, out _);
                lookup.Add(this.includeHash.GetValue(), newMap, out _);
            }
            
            this.context.withCount    = 0;
            this.context.withoutCount = 0;
            unchecked { this.context.version++; }
            
            ContextPool.Return(this.context);
            
            return filter;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal Filter CompleteBuild() {
            var includedTypeIds = new int[this.context.withCount];
            for (var i = 0; i < this.context.withCount; i++) {
                includedTypeIds[i] = this.context.with[i];
            }
            Array.Sort(includedTypeIds);
            
            var excludedTypeIds = new int[this.context.withoutCount];
            for (var i = 0; i < this.context.withoutCount; i++) {
                excludedTypeIds[i] = this.context.without[i];
            }
            Array.Sort(excludedTypeIds);
            
            return new Filter(this.world, includedTypeIds, excludedTypeIds);
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        internal class Context {
            internal int[] with;
            internal int[] without;
            
            internal int   withCount;
            internal int   withoutCount;
            
            internal int   version;
            
            internal static Context CreateDefault() {
                return new Context {
                    with         = new int[16],
                    without      = new int[16],
                    withCount    = 0,
                    withoutCount = 0,
                    version      = 0,
                };
            }
        }
        
        [Il2CppEagerStaticClassConstruction]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        internal static class ContextPool {
            internal static Context[] pool;
            internal static int       poolLength;
            internal static object    @lock;
            
            static ContextPool() {
                pool = new Context[2];
                for (var i = 0; i < pool.Length; i++) {
                    pool[i] = Context.CreateDefault();
                }
                
                poolLength = pool.Length;
                @lock = new object();
            }
            
            internal static Context Get() {
                Context ctx;
                
                lock (@lock) {
                    if (poolLength == 0) {
                        ctx = Context.CreateDefault();
                    }
                    else {
                        ctx = pool[--poolLength];
                        pool[poolLength] = null;
                    }
                }
                
                return ctx;
            }
            
            internal static void Return(Context context) {
                lock (@lock) {
                    if (poolLength == pool.Length) {
                        Array.Resize(ref pool, pool.Length << 1);
                    }

                    pool[poolLength++] = context;
                }
            }
        }
    }
}
namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FilterBuilder {
        internal World world;
        internal FilterBuilder parent;
        internal TypeInfo typeInfo;
        internal Filter.Mode mode;
        internal int level;
        internal TypeHash includeHash;
        internal TypeHash excludeHash;
        
        public FilterBuilder With<T>() where T : struct, IComponent {
            var info = ComponentId<T>.info;
            
            var current = this;
            while (current.parent != null) {
                if (current.typeInfo.id == info.id && current.mode == Filter.Mode.Include) {
                    return this;
                }
                current = current.parent;
            }
            
            return new FilterBuilder {
                parent = this,
                world = this.world,
                mode = Filter.Mode.Include,
                typeInfo = info,
                level = this.level + 1,
                includeHash = this.includeHash.Combine(info.hash),
                excludeHash = this.excludeHash,
            };
        }

        public FilterBuilder Without<T>() where T : struct, IComponent {
            var info = ComponentId<T>.info;
            
            var current = this;
            while (current.parent != null) {
                if (current.typeInfo.id == info.id && current.mode == Filter.Mode.Exclude) {
                    return this;
                }
                current = current.parent;
            }
            
            return new FilterBuilder {
                parent = this,
                world = this.world,
                mode = Filter.Mode.Exclude,
                typeInfo = info,
                level = this.level + 1,
                includeHash = this.includeHash,
                excludeHash = this.excludeHash.Combine(info.hash),
            };
        }

        public FilterBuilder Extend<T>() where T : struct, IFilterExtension {
            var newFilter = default(T).Extend(this);
            return newFilter;
        }

        public Filter Build() {
            var lookup = this.world.filtersLookup;

            if (lookup.TryGetValue(this.includeHash.GetValue(), out var excludeMap)) {
                if (excludeMap.TryGetValue(this.excludeHash.GetValue(), out var existFilter)) {
                    return existFilter;
                }
                var filter = this.CompleteBuild();
                excludeMap.Add(this.excludeHash.GetValue(), filter, out _);
                return filter;
            } else {
                var filter = this.CompleteBuild();
                var newMap = new LongHashMap<Filter>();
                newMap.Add(this.excludeHash.GetValue(), filter, out _);
                lookup.Add(this.includeHash.GetValue(), newMap, out _);
                return filter;
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal Filter CompleteBuild() {
            var includedTypeIds = new FastList<int>(this.level);
            var excludedTypeIds = new FastList<int>(this.level);
            
            var current = this;

            while (current.parent != null) {
                if (current.mode == Filter.Mode.Include) {
                    includedTypeIds.Add(current.typeInfo.id);
                } else if (current.mode == Filter.Mode.Exclude) {
                    excludedTypeIds.Add(current.typeInfo.id);
                }
                
                current = current.parent;
            }
            
            var includedTypeIdsSorted = includedTypeIds.ToArray();
            Array.Sort(includedTypeIdsSorted);
            
            var excludedTypeIdsSorted = excludedTypeIds.ToArray();
            Array.Sort(excludedTypeIdsSorted);
            
            return new Filter(this.world, includedTypeIdsSorted, excludedTypeIdsSorted);
        }
    }
}
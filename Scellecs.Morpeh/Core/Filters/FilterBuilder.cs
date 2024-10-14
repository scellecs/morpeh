namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FilterBuilder {
        internal World         world;
        internal FilterBuilder parent;
        internal TypeHash      includeHash;
        internal TypeHash      excludeHash;
        internal Filter.Mode   mode;
        internal int           typeId;
        internal int           includeCount;
        internal int           excludeCount;
        
        
        public FilterBuilder With<T>() where T : struct, IComponent {
            var info = ComponentId<T>.info;
            
            var current = this;
            while (current.parent != null) {
                if (current.typeId == info.id && current.mode == Filter.Mode.Include) {
                    return this;
                }
                current = current.parent;
            }
            
            return new FilterBuilder {
                parent = this,
                world = this.world,
                mode = Filter.Mode.Include,
                typeId = info.id,
                includeCount = this.includeCount + 1,
                excludeCount = this.excludeCount,
                includeHash = this.includeHash.Combine(info.hash),
                excludeHash = this.excludeHash,
            };
        }

        public FilterBuilder Without<T>() where T : struct, IComponent {
            var info = ComponentId<T>.info;
            
            var current = this;
            while (current.parent != null) {
                if (current.typeId == info.id && current.mode == Filter.Mode.Exclude) {
                    return this;
                }
                current = current.parent;
            }
            
            return new FilterBuilder {
                parent = this,
                world = this.world,
                mode = Filter.Mode.Exclude,
                typeId = info.id,
                includeCount = this.includeCount,
                excludeCount = this.excludeCount + 1,
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
            var includedTypeIds = new int[this.includeCount];
            var includedTypeIdsIndex = 0;
            
            var excludedTypeIds = new int[this.excludeCount];
            var excludedTypeIdsIndex = 0;
            
            var current = this;

            while (current.parent != null) {
                if (current.mode == Filter.Mode.Include) {
                    includedTypeIds[includedTypeIdsIndex++] = current.typeId;
                } else if (current.mode == Filter.Mode.Exclude) {
                    excludedTypeIds[excludedTypeIdsIndex++] = current.typeId;
                }
                
                current = current.parent;
            }
            
            Array.Sort(includedTypeIds);
            Array.Sort(excludedTypeIds);
            
            return new Filter(this.world, includedTypeIds, excludedTypeIds);
        }
    }
}
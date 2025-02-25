#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal enum SearchFilterListMode { 
        None = 0,
        EntityIds = 1,
        Archetypes = 2,
    }

    internal sealed class SearchFilterList : IList<Entity> {
        private readonly List<int> archetypesRanges;
        private readonly List<Archetype> archetypes;
        private readonly LongHashMap<int> rangesMap;
        private readonly List<Entity> ids;
        private SearchFilterListMode mode;
        private int count;

        internal SearchFilterList() {
            this.archetypesRanges = new List<int>();
            this.archetypes = new List<Archetype>();
            this.rangesMap = new LongHashMap<int>();
            this.ids = new List<Entity>();
            this.mode = SearchFilterListMode.None;
            this.count = 0;
        }

        public Entity this[int index] {
            get => this.mode == SearchFilterListMode.Archetypes ? Get(index) : this.ids[index];
            set => throw new NotImplementedException();
        }

        internal int this[Entity entity] {
            get => IndexOf(entity);
        }

        public int Count => this.count;
        public bool IsReadOnly => false;

        public void Add(Entity entity) {
            this.ids.Add(entity);
            this.count++;
        }

        internal void Add(Archetype archetype) {
            this.rangesMap.Add(archetype.hash.GetValue(), this.archetypesRanges.Count, out _);
            this.archetypesRanges.Add(this.count);
            this.archetypes.Add(archetype);
            this.count += archetype.length;
        }

        public int IndexOf(Entity entity) {
            if (this.mode == SearchFilterListMode.EntityIds) {
                return this.ids.IndexOf(entity);
            }

            var handle = new EntityHandle(entity);
            if (handle.IsValid) {
                var entityData = handle.EntityData;
                var hash = entityData.currentArchetype.hash.GetValue();
                var indexInArchetype = entityData.indexInCurrentArchetype;
                if (this.rangesMap.TryGetValue(hash, out var rangeIndex)) {
                    var range = this.archetypesRanges[rangeIndex];
                    return range + indexInArchetype;
                }
            }
            return -1;
        }

        public void Clear() {
            this.rangesMap.Clear();
            this.archetypesRanges.Clear();
            this.archetypes.Clear();
            this.ids.Clear();
            this.mode = SearchFilterListMode.None;
            this.count = 0;
        }

        public void CopyTo(Entity[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            if (array.Length - arrayIndex < this.Count) {
                throw new ArgumentException("Destination array is not long enough");
            }

            if (this.mode == SearchFilterListMode.EntityIds) {
                this.ids.CopyTo(array, arrayIndex);
            }
            else {
                var currentIndex = arrayIndex;
                for (var i = 0; i < this.archetypes.Count; i++) {
                    var archetype = this.archetypes[i];
                    var rangeStart = this.archetypesRanges[i];
                    var rangeEnd = (i == this.archetypesRanges.Count - 1) ? this.count : this.archetypesRanges[i + 1];
                    var length = rangeEnd - rangeStart;
                    Array.Copy(archetype.entities.data, 0, array, currentIndex, length);
                    currentIndex += length;
                }
            }
        }

        internal void SetMode(SearchFilterListMode mode) {
            this.mode = mode;
        }

        private Entity Get(int index) {
            if (index >= this.count || index < 0) {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var left = 0;
            var right = this.archetypesRanges.Count - 1;
            var foundRangeIndex = -1;

            while (left <= right) {
                var mid = left + (right - left) / 2;
                var rangeStart = this.archetypesRanges[mid];
                var rangeEnd = (mid == this.archetypesRanges.Count - 1) ? this.count : this.archetypesRanges[mid + 1];
                if (index >= rangeStart && index < rangeEnd) {
                    foundRangeIndex = mid;
                    break;
                }
                if (index < rangeStart) {
                    right = mid - 1;
                }
                else {
                    left = mid + 1;
                }
            }

            var archetype = this.archetypes[foundRangeIndex];
            var indexInArchetype = index - this.archetypesRanges[foundRangeIndex];
            return archetype.entities[indexInArchetype];
        }

        public bool Contains(Entity item) => throw new NotImplementedException();
        public void Insert(int index, Entity item) => throw new NotImplementedException();
        public bool Remove(Entity item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
        public IEnumerator<Entity> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
#endif

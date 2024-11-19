#if UNITY_EDITOR
using Scellecs.Morpeh.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal enum SearchFilterListMode { 
        None = 0,
        EntityIds = 1,
        Archetypes = 2,
    }

    internal sealed class SearchFilterList : IList {
        private readonly List<int> archetypesRanges;
        private readonly List<Archetype> archetypes;
        private readonly LongHashMap<int> rangesMap;
        private readonly List<EntityHandle> ids;

        private SearchFilterListMode mode;
        private int count;

        internal SearchFilterList() {
            this.archetypesRanges = new List<int>();
            this.archetypes = new List<Archetype>();
            this.rangesMap = new LongHashMap<int>();
            this.ids = new List<EntityHandle>();
            this.mode = SearchFilterListMode.None;
            this.count = 0;
        }

        internal int this[EntityHandle handle] {
            get => mode == SearchFilterListMode.Archetypes ? IndexOf(handle) : ids.IndexOf(handle);
        }

        internal EntityHandle this[int index] {
            get => mode == SearchFilterListMode.Archetypes ? Get(index) : ids[index];
        }

        object IList.this[int index] {
            get => mode == SearchFilterListMode.Archetypes ? Get(index) : ids[index];
            set => throw new NotImplementedException();
        }

        public int Count => this.count;

        public void Clear() {
            this.rangesMap.Clear();
            this.archetypesRanges.Clear();
            this.archetypes.Clear();
            this.ids.Clear();
            this.mode = SearchFilterListMode.None;
            this.count = 0;
        }

        internal void SetMode(SearchFilterListMode mode) { 
            this.mode = mode;
        }

        internal void Add(EntityHandle handle) { 
            this.ids.Add(handle);
            this.count++;
        }

        internal void Add(Archetype archetype) {
            this.rangesMap.Add(archetype.hash.GetValue(), this.archetypesRanges.Count, out _);
            this.archetypesRanges.Add(this.count);
            this.archetypes.Add(archetype);
            this.count += archetype.length;
        }

        private EntityHandle Get(int index) {
            if (index >= count || index < 0) {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var left = 0;
            var right = this.archetypesRanges.Count - 1;
            var foundRangeIndex = -1;

            while (left <= right) {
                var mid = left + (right - left) / 2;
                var rangeStart = this.archetypesRanges[mid];
                var rangeEnd = (mid == this.archetypesRanges.Count - 1) ? count : this.archetypesRanges[mid + 1];

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
            var entity = archetype.entities[indexInArchetype];
            return new EntityHandle(entity, archetype.hash.GetValue());
        }

        private int IndexOf(EntityHandle handle) {
            if (handle.IsValid) {
                var entityData = handle.World.entities[handle.entity.Id];
                var hash = entityData.currentArchetype.hash.GetValue();
                var indexInArchetype = entityData.indexInCurrentArchetype;

                if (rangesMap.TryGetValue(hash, out var rangeIndex)) {
                    var range = this.archetypesRanges[rangeIndex];
                    return range + indexInArchetype;
                }
            }

            return -1;
        }

        bool ICollection.IsSynchronized => throw new NotImplementedException();
        bool IList.IsFixedSize => throw new NotImplementedException();
        bool IList.IsReadOnly => throw new NotImplementedException();
        object ICollection.SyncRoot => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();
        int IList.Add(object value) => throw new NotImplementedException();
        bool IList.Contains(object value) => throw new NotImplementedException();
        int IList.IndexOf(object value) => throw new NotImplementedException();
        void IList.Insert(int index, object value) => throw new NotImplementedException();
        void IList.Remove(object value) => throw new NotImplementedException();
        void IList.RemoveAt(int index) => throw new NotImplementedException();
    }
}
#endif

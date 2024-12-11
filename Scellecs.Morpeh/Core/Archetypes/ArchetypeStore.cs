namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class ArchetypeStore {
        internal LongSlotMap map;
        internal Archetype[] archetypes;
        internal int         length;
        
        public ArchetypeStore() {
            this.map = new LongSlotMap(63);
            this.archetypes = new Archetype[this.map.capacity];
            this.length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ArchetypeHash hash, out Archetype archetype) {
            if (this.map.TryGetIndex(hash.GetValue(), out var slotIndex)) {
                archetype = this.archetypes[slotIndex];
                return true;
            }

            archetype = null;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(ArchetypeHash hash) {
            return this.map.Has(hash.GetValue());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Archetype archetype) {
            var slotIndex = this.map.TakeSlot(archetype.hash.GetValue(), out var resized);
            
            if (resized) {
                this.ResizeArchetypes(this.map.capacity);
            }
            
            this.archetypes[slotIndex] = archetype;
            ++this.length;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ResizeArchetypes(int newLength) {
            Array.Resize(ref this.archetypes, newLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(Archetype archetype) {
            if (!this.map.Remove(archetype.hash.GetValue(), out var slotIndex)) {
                return;
            }

            this.archetypes[slotIndex] = null;
            --this.length;
        }
        
        public void Clear() {
            Array.Clear(this.archetypes, 0, this.map.lastIndex);
            this.map.Clear();
            this.length = 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            
            e.enumerator = this.map.GetEnumerator();
            e.archetypes = this.archetypes;
            
            return e;
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            internal LongSlotMap.Enumerator enumerator;
            internal Archetype[] archetypes;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => this.enumerator.MoveNext();

            public Archetype Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.archetypes[this.enumerator.Current];
            }
        }
    }
}
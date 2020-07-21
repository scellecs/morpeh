namespace Morpeh.Globals {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Singleton")]
    public class Singleton : BaseSingleton {
        public int ID => this.Entity.ID;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponent => ref this.Entity.AddComponent<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent => ref this.Entity.AddComponent<T>(out exist);

        public bool AddComponentFast(in int typeId, in int componentId) => this.Entity.AddComponentFast(typeId, componentId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>() where T : struct, IComponent => ref this.Entity.GetComponent<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>(out bool exist) where T : struct, IComponent => ref this.Entity.GetComponent<T>(out exist);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentFast(in int typeId) => this.Entity.GetComponentFast(typeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent => this.Entity.SetComponent(in value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponent => this.Entity.RemoveComponent<T>();

        public bool RemoveComponentFast(int typeId, out int cacheIndex) => this.Entity.RemoveComponentFast(typeId, out cacheIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has<T>() where T : struct, IComponent => this.Entity.Has<T>();
    }
}
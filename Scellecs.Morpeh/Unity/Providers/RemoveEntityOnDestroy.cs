namespace Scellecs.Morpeh.Providers {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(RemoveEntityOnDestroy))]
    public sealed class RemoveEntityOnDestroy : EntityProvider {
        private void OnDestroy() {
            if (this.cachedEntity.IsNullOrDisposed() == false) {
                this.cachedEntity.world.RemoveEntity(this.cachedEntity);
            }
        }
    }
}

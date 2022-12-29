namespace Scellecs.Morpeh.Providers {
    using UnityEngine.Assertions;

    public interface IMonoComponent<T> : IComponent
        where T : UnityEngine.Component {
        T monoComponent { get; set; }
    }

    public abstract class ComponentProvider<T0, T1> : MonoProvider<T1>
        where T0 : UnityEngine.Component
        where T1 : struct, IMonoComponent<T0> {
        protected override void OnValidate() {
            base.OnValidate();
            ref var data = ref this.GetData(out _);
            if (data.monoComponent == null) {
                data.monoComponent = this.gameObject.GetComponent<T0>();
                Assert.IsNotNull(data.monoComponent, $"Missing {typeof(T0)} component.");
            }
        }
    }
}
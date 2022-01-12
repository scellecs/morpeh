namespace Morpeh.Globals {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Events/Event Object")]
    public sealed class GlobalEventObject : BaseGlobalEvent<Object> {
        public override string Serialize(Object data) => data.ToString();

        public override Object Deserialize(string serializedData) => throw new System.NotImplementedException();
    }
}
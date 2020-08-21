namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Events/Event Float")]
    public class GlobalEventFloat : BaseGlobalEvent<float> {
        public override string Serialize(float data) => data.ToString(CultureInfo.InvariantCulture);

        public override float Deserialize(string serializedData) => float.Parse(serializedData, CultureInfo.InvariantCulture);
    }
}
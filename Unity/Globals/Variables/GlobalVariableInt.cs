namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Int")]
    public class GlobalVariableInt : BaseGlobalVariable<int> {
        protected override int Load(string serializedData) => int.Parse(serializedData, CultureInfo.InvariantCulture);

        protected override string Save() => this.value.ToString(CultureInfo.InvariantCulture);
    }
}
namespace Morpeh.Globals {
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable String")]
    public class GlobalVariableString : BaseGlobalVariable<string> {
        protected override string Load(string serializedData) => serializedData;

        protected override string Save() => this.value;
    }
}
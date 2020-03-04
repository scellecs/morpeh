namespace Morpeh.Globals {
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable GameObject")]
    public class GlobalVariableGameObject : BaseGlobalVariable<GameObject> {
        public override bool CanBeSerialized => false;

        protected override GameObject Load(string serializedData) => null;

        protected override string Save() => string.Empty;
    }
}
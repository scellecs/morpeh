namespace Morpeh.Globals {
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Globals/Variable GameObject")]
    public class GlobalVariableGameObject : BaseGlobalVariable<GameObject> {
        public override bool CanBeSerialized => false;

        protected override GameObject Load(string serializedData) => null;

        protected override string Save() => string.Empty;
    }
}
namespace Morpeh.Globals {
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
    [InlineEditor]
#endif
    [CreateAssetMenu(menuName = "ECS/Globals/Variable GameObject")]
    public class GlobalVariableGameObject : BaseGlobalVariable<GameObject> {
        public override bool CanBeSerialized => false;

        protected override GameObject Load(string serializedData) => null;

        protected override string Save() => string.Empty;
    }
}
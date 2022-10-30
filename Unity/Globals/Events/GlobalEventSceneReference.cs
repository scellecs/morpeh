namespace Morpeh.Globals {
    using UnityEngine;
    using Utils;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Events/Event SceneReference")]
    public class GlobalEventSceneReference : BaseGlobalEvent<SceneReference> {
        public override string Serialize(SceneReference data) => data.ToString();

        public override SceneReference Deserialize(string serializedData) => throw new System.NotImplementedException();

        public void Publish(string level) {
            var newScene = new SceneReference {ScenePath = level};
            base.Publish(newScene);
        }
    }
}
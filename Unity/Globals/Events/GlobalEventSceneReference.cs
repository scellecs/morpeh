namespace Morpeh.Globals {
    using UnityEngine;
    using Utils;

    [CreateAssetMenu(menuName = "ECS/Globals/Event SceneReference")]
    public class GlobalEventSceneReference : BaseGlobalEvent<SceneReference> {
        public void Publish(string level) {
            var newScene = new SceneReference {ScenePath = level};
            base.Publish(newScene);
        }
    }
}
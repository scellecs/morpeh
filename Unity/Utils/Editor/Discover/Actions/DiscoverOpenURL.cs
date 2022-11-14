namespace Scellecs.Morpeh.Utils.Editor.Discover.Actions {
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Utils/Discover Actions/" + "Open URL")]
    public sealed class DiscoverOpenURL : DiscoverAction {
        public string URL;
        
        public override string ActionName => "Open URL";

        public override void DoAction() {
            Application.OpenURL(this.URL);
        }
    }

}
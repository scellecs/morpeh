namespace Morpeh.Utils.Editor.Actions {
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Utils/Discover Actions/" + "Execute MenuItem")]
    public sealed class DiscoverExecuteMenuItem : DiscoverAction {
        public string MenuItemPath;
        public string ButtonName = "Execute";
        
        public override string ActionName => this.ButtonName;

        public override void DoAction() {
            EditorApplication.ExecuteMenuItem(this.MenuItemPath);
        }
    }

}
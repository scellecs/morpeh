namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Toggle Group Provider")]
    public class ToggleGroupProvider : BaseComponentProvider<ToggleGroup, ToggleGroupComponent>
    {
    }

    [System.Serializable]
    public struct ToggleGroupComponent : IMonoComponent<ToggleGroup>
    {
        public ToggleGroup ToggleGroup;

        public ToggleGroup monoComponent
        {
            get { return this.ToggleGroup; }
            set { this.ToggleGroup = value; }
        }
    }
}

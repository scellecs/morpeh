namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Toggle Provider")]
    public class ToggleProvider : BaseComponentProvider<Toggle, ToggleComponent>
    {
    }

    [System.Serializable]
    public struct ToggleComponent : IMonoComponent<Toggle>
    {
        public Toggle Toggle;

        public Toggle monoComponent
        {
            get { return this.Toggle; }
            set { this.Toggle = value; }
        }
    }
}

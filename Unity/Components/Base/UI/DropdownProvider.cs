namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Dropdown Provider")]
    public class DropdownProvider : BaseComponentProvider<Dropdown, DropdownComponent>
    {
    }

    [System.Serializable]
    public struct DropdownComponent : IMonoComponent<Dropdown>
    {
        public Dropdown Dropdown;

        public Dropdown monoComponent
        {
            get { return this.Dropdown; }
            set { this.Dropdown = value; }
        }
    }
}

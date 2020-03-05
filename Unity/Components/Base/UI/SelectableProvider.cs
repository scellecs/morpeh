namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Selectable Provider")]
    public class SelectableProvider : BaseComponentProvider<Selectable, SelectableComponent>
    {
    }

    [System.Serializable]
    public struct SelectableComponent : IMonoComponent<Selectable>
    {
        public Selectable Selectable;

        public Selectable monoComponent
        {
            get { return this.Selectable; }
            set { this.Selectable = value; }
        }
    }
}

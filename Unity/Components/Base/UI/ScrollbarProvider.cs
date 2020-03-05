namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Scrollbar Provider")]
    public class ScrollbarProvider : BaseComponentProvider<Scrollbar, ScrollbarComponent>
    {
    }

    [System.Serializable]
    public struct ScrollbarComponent : IMonoComponent<Scrollbar>
    {
        public Scrollbar Scrollbar;

        public Scrollbar monoComponent
        {
            get { return this.Scrollbar; }
            set { this.Scrollbar = value; }
        }
    }
}

namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Scroll Rect Provider")]
    public class ScrollRectProvider : BaseComponentProvider<ScrollRect, ScrollRectComponent>
    {
    }

    [System.Serializable]
    public struct ScrollRectComponent : IMonoComponent<ScrollRect>
    {
        public ScrollRect ScrollRect;

        public ScrollRect monoComponent
        {
            get { return this.ScrollRect; }
            set { this.ScrollRect = value; }
        }
    }
}

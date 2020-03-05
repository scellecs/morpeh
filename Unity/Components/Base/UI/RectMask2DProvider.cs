namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Rect Mask 2D Provider")]
    public class RectMask2DProvider : BaseComponentProvider<RectMask2D, MaskComponent>
    {
    }

    [System.Serializable]
    public struct RectMask2DComponent : IMonoComponent<RectMask2D>
    {
        public RectMask2D RectMask2D;

        public RectMask2D monoComponent
        {
            get { return this.RectMask2D; }
            set { this.RectMask2D = value; }
        }
    }
}

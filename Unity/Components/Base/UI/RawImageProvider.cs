namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Raw Image Provider")]
    public class RawImageProvider : BaseComponentProvider<RawImage, RawImageComponent>
    {
    }

    [System.Serializable]
    public struct RawImageComponent : IMonoComponent<RawImage>
    {
        public RawImage RawImage;

        public RawImage monoComponent
        {
            get { return this.RawImage; }
            set { this.RawImage = value; }
        }
    }
}

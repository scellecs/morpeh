namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Image Provider")]
    public class ImageProvider : BaseComponentProvider<Image, ImageComponent>
    {
    }

    [System.Serializable]
    public struct ImageComponent : IMonoComponent<Image>
    {
        public Image Image;

        public Image monoComponent
        {
            get { return this.Image; }
            set { this.Image = value; }
        }
    }
}

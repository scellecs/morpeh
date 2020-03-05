namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Mask Provider")]
    public class MaskProvider : BaseComponentProvider<Mask, MaskComponent>
    {
    }

    [System.Serializable]
    public struct MaskComponent : IMonoComponent<Mask>
    {
        public Mask Mask;

        public Mask monoComponent
        {
            get { return this.Mask; }
            set { this.Mask = value; }
        }
    }
}

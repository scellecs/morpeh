namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Slider Provider")]
    public class SliderProvider : BaseComponentProvider<Slider, SliderComponent>
    {
    }

    [System.Serializable]
    public struct SliderComponent : IMonoComponent<Slider>
    {
        public Slider Slider;

        public Slider monoComponent
        {
            get { return this.Slider; }
            set { this.Slider = value; }
        }
    }
}

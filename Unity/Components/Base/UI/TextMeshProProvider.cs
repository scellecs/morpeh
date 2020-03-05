namespace Morpeh
{
#if TMPro
    using UnityEngine;
    using TMPro;

    [AddComponentMenu("Morpeh/Base Components/UI/TextMeshPro Provider")]
    public class TextMeshProProvider : BaseComponentProvider<TextMeshProUGUI, TextMeshProComponent>
    {
    }

    [System.Serializable]
    public struct TextMeshProComponent : IMonoComponent<TextMeshProUGUI>
    {
        public TextMeshProUGUI Text;

        public TextMeshProUGUI monoComponent
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
    }
#endif
}

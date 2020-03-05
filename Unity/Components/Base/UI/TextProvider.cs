namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Text Provider")]
    public class TextProvider : BaseComponentProvider<Text, TextComponent>
    {
    }

    [System.Serializable]
    public struct TextComponent : IMonoComponent<Text>
    {
        public Text Text;

        public Text monoComponent
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
    }
}

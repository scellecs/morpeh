namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/Button Provider")]
    public class ButtonProvider : BaseComponentProvider<Button, ButtonComponent>
    {
    }

    [System.Serializable]
    public struct ButtonComponent : IMonoComponent<Button>
    {
        public Button Button;

        public Button monoComponent
        {
            get { return this.Button; }
            set { this.Button = value; }
        }
    }
}

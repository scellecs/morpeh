namespace Morpeh
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Morpeh/Base Components/UI/InputField Provider")]
    public class InputFieldProvider : BaseComponentProvider<InputField, InputFieldComponent>
    {
    }

    [System.Serializable]
    public struct InputFieldComponent : IMonoComponent<InputField>
    {
        public InputField InputField;

        public InputField monoComponent
        {
            get { return this.InputField; }
            set { this.InputField = value; }
        }
    }
}

namespace Test.Namespace;

using Scellecs.Morpeh;

[EcsComponent]
public partial struct PropertyWithBackingFieldComponent {
    public int Value { get; set; }
}
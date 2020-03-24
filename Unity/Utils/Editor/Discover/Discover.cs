using UnityEngine;

[CreateAssetMenu(fileName = "DiscoverElement", menuName = "ScriptableObjects/Discover", order = 1)]
public class Discover : ScriptableObject
{
    public string Name = "Discover";
    public string Category = "Category";
    public bool DefaultSelected = false;

#if UNITY_EDITOR
    public Texture2D image;
#endif

    [Multiline]
    public string Description = "Some Description of the Component\n\nCan be set as multiple lines.";
    public int Priority = 0;
    public DiscoverSection[] Sections = new DiscoverSection[0];

}

[System.Serializable]
public struct DiscoverSection
{
    public string SectionName;
#if UNITY_EDITOR
    public Texture2D image;
#endif
    [Multiline]
    public string SectionContent;
    public SectionAction[] Actions;
}

[System.Serializable]
public struct SectionAction
{
    public string Description;
    public Object Target;
}
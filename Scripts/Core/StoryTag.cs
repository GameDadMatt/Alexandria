using Godot;

[GlobalClass]
public partial class StoryTag : Resource
{
    [ExportGroup("Tag Data")] [Export] public string Name { get; set; } = "New Tag";

    [Export] public Texture2D Icon { get; set; }
    [Export] public Color Color { get; set; } = Colors.White;
}
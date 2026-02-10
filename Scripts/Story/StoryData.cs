using Godot;

namespace Alexandria.Story;

[GlobalClass]
public partial class StoryData : Resource
{
    [ExportGroup("Narrative")] [Export] public string Title { get; set; } = "New Story";

    [Export] public string ShortContent { get; set; } = "Summary of story content to appear on timeline";
    [Export] public string LongContent { get; set; } = "Full story content to appear in library";

    [ExportGroup("Visuals")] [Export] public Texture2D Icon { get; set; } //The icon for the story in the timeline

    [Export] public Texture2D Image { get; set; } //The image for the story in the library

    [ExportGroup("Tags")] [Export] public StoryTag NarrativeTag { get; set; }

    [Export] public StoryTag[] MatchingTags { get; set; }
}
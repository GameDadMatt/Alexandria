using Godot;

namespace Alexandria.Story;

public partial class StoryCard : PanelContainer
{
    [Export] private RichTextLabel _cardDescription;
    [Export] private Label _cardTitle;

    private StoryData _currentStory; //Store a reference to the story event

    public void Initialize(StoryData storyData)
    {
        _currentStory = storyData;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_currentStory == null)
            return;

        _cardTitle.Text = _currentStory.Title;
        _cardDescription.Text = _currentStory.ShortContent;
    }
}
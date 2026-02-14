using Godot;

namespace Alexandria.Story;

public partial class TimelineManager : Control
{
    [Export] public PackedScene CardScene { get; set; }
    [Export] public Node CardContainerNode { get; set; }

    public override void _Ready()
    {
        //Generate two test cards
        SpawnCard(new Vector2(100, 100), "Beginning");
        SpawnCard(new Vector2(400, 100), "Ending");
    }

    private void SpawnCard(Vector2 position, string debugName)
    {
        var newCard = CardScene.Instantiate<StoryCard>();
        CardContainerNode.AddChild(newCard); //Add the card as a child of the card container node
        newCard.GlobalPosition = position;
        newCard.Name = debugName;
    }
}
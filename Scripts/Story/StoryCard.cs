using Godot;

namespace Alexandria.Story;

public partial class StoryCard : Control
{
    [Signal] public delegate void CardMovedEventHandler(StoryCard card);
    //Signals for connections so we can forward the child signals
    [Signal] public delegate void ConnectionStartedEventHandler(ConnectionHandle handle);
    [Signal] public delegate void ConnectionEndedEventHandler(ConnectionHandle handle);
    
    [ExportGroup("Card Display")]
    [Export] private Label _cardTitle;
    [Export] private RichTextLabel _cardDescription;
    
    [ExportGroup ("Connection Handles")]
    [Export] private ConnectionHandle _inputHandle;
    public ConnectionHandle InputHandle => _inputHandle;
    [Export] private ConnectionHandle _outputHandle;
    public ConnectionHandle OutputHandle => _outputHandle;

    private bool _isDragging = false;
    private Vector2 _dragOffset;
    
    [Export] private StoryData _testStory;
    private StoryData _currentStory; //Store a reference to the story event

    public override void _Ready()
    {
        if (_inputHandle == null || _outputHandle == null)
        {
            GD.PrintErr($"CRITICAL: Handles are missing on card {Name}!");
            return;
        }
        
        //Forward the signals from our ConnectionHandles
        _inputHandle.ConnectionStarted += (h) => EmitSignal(SignalName.ConnectionStarted, h);
        _outputHandle.ConnectionStarted += (h) => EmitSignal(SignalName.ConnectionStarted, h);
        _inputHandle.ConnectionEnded += (h) => EmitSignal(SignalName.ConnectionEnded, h);
        _outputHandle.ConnectionEnded += (h) => EmitSignal(SignalName.ConnectionEnded, h);
        
        Initialize(_testStory);
    }

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

    public override void _GuiInput(InputEvent input)
    {
        if (input is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                GD.Print("Card Dragging!");
                _isDragging = true;
                _dragOffset = GetGlobalMousePosition() - GlobalPosition; //Calculate the position of the mouse pointer from the top left
                MoveToFront(); //Ensure the card is on top of other cards
                AcceptEvent(); //Stop the event from being passed to other nodes
            }
            else
            {
                GD.Print("Card Stopped Dragging!");
                _isDragging = false;
            }
        }
        
        //If we are moving, emit a signal saying so
        if (input is InputEventMouseMotion motionEvent && _isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
            EmitSignal(SignalName.CardMoved, this);
        }

        if (input is InputEventMouseMotion mouseMotion && _isDragging)
        {
            //Move the card to the mouse position
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }
    }
}
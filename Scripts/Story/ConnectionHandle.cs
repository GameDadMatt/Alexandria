using Godot;

namespace Alexandria.Story;
public partial class ConnectionHandle : Control
{
	[Signal] public delegate void ConnectionStartedEventHandler(ConnectionHandle handle);
	[Signal] public delegate void ConnectionEndedEventHandler(ConnectionHandle handle);
	
	[Export] public bool IsInputHandle { get; set; } //Input is left, output is right
	
	public override void _GuiInput(InputEvent input)
	{
		if(input is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if(mouseButton.Pressed)
			{
				EmitSignal(SignalName.ConnectionStarted, this);
				AcceptEvent();
			}
			else if (!mouseButton.Pressed)
			{
				//When we release the mouse button, the ConnectionHandle that started it is the one passed, NOT the one the mouse is currently hovering over
				EmitSignal(SignalName.ConnectionEnded, this);
			}
		}
	}

	//Get the position of this connection
	public Vector2 GetConnectionPosition() => GlobalPosition + (Size / 2);
}

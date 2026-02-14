using Godot;
using System.Collections.Generic;

namespace Alexandria.Story;

public struct CardConnection
{
	public ConnectionHandle StartHandle;
	public ConnectionHandle EndHandle;
	public Line2D Line;
}

public partial class ConnectionManager : Control
{
	[ExportGroup("Line Settings")]
	[Export] private float _lineThickness = 4f;
	[Export] private Color _activeLineColor = Colors.DarkRed;
	[Export] private Color _permanentLineColor = Colors.DarkGray;
	
	private Line2D _activeLine;
	private ConnectionHandle _startHandle;
	
	private List<StoryCard> _activeCards = new List<StoryCard>();
	private List<CardConnection> _permanentConnections = new List<CardConnection>();
	
	public override void _Process(double delta)
	{
		if (_activeLine != null && _activeLine.GetPointCount() == 2)
		{
			//Update the end of the line to follow the mouse pointer
			_activeLine.SetPointPosition(1, GetLocalMousePosition());
		}
	}
	
	//Logic for releasing the button while we're not over a valid handle
	public override void _UnhandledInput(InputEvent @event)
	{
		if (_activeLine != null && @event is InputEventMouseButton mouseEvent)
		{
			if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				//Cancel the connection. This will get caught by EndConnection first if there is a valid handle.
				CancelConnection();
			}
		}
	}

	private void OnCardAddedToTree(Node child)
	{
		if (child is StoryCard card)
			RegisterCard(card);
	}

	private void OnCardRemovedFromTree(Node child)
	{
		if (child is StoryCard card)
			UnregisterCard(card);
	}

	//Register cards as they are created
	private void RegisterCard(StoryCard card)
	{
		GD.Print("Registering card!");
		_activeCards.Add(card);
		card.ConnectionStarted += StartConnection;
		card.ConnectionEnded += EndConnection;
		card.CardMoved += OnCardMove;
	}
	
	//Unregister cards as they are removed
	private void UnregisterCard(StoryCard card)
	{
		GD.Print("Unregistering card!");
		_activeCards.Remove(card);
		card.ConnectionStarted -= StartConnection;
		card.ConnectionEnded -= EndConnection;
		card.CardMoved -= OnCardMove;
	}

	private void StartConnection(ConnectionHandle handle)
	{
		GD.Print("Starting line!");
		
		_startHandle = handle;
		_activeLine = new Line2D();
		_activeLine.Width = _lineThickness;
		_activeLine.DefaultColor = _activeLineColor;
		AddChild(_activeLine);
		
		_activeLine.AddPoint(ToLocalInManager(handle.GetConnectionPosition()));
		_activeLine.AddPoint(GetLocalMousePosition());
	}

	private void EndConnection(ConnectionHandle handle)
	{
		GD.Print("Ending line!");
		
		if (_startHandle == null)
			return;
		
		//Get the ending handle
		//The handle we receive will always be the first handle clicked in a drag, NOT the handle we are hovering over at the end
		ConnectionHandle endHandle = GetHandleUnderMouse();
		if (endHandle == null)
		{
			GD.PrintErr("No valid handle under mouse!");
			CancelConnection();
			return;
		}

		var startCard = _startHandle.GetParent<StoryCard>();
		var endCard = endHandle.GetParent<StoryCard>();
		
		//Confirm this is not the same card we started on
		if (startCard != null && endCard != null && startCard == endCard)
		{
			GD.PrintErr("Cannot connect a node to itself!");
			CancelConnection();
			return;
		}
		
		//We cannot connect handles that are both input or output
		GD.Print(_startHandle.IsInputHandle + " compared to " + endHandle.IsInputHandle);
		if (_startHandle.IsInputHandle == endHandle.IsInputHandle)
		{
			GD.PrintErr("Cannot connect two input or two output handles!");
			CancelConnection();
			return;
		}

		CreatePermanentConnection(_startHandle, endHandle);
	}

	private ConnectionHandle GetHandleUnderMouse()
	{
		Vector2 mousePos = GetLocalMousePosition();

		foreach (var card in _activeCards)
		{
			//Check the handles
			if(card.InputHandle.GetGlobalRect().HasPoint(mousePos)) return card.InputHandle;
			if(card.OutputHandle.GetGlobalRect().HasPoint(mousePos)) return card.OutputHandle;
		}

		return null; //The mouse was dropped in empty space
	}
	
	private void CreatePermanentConnection(ConnectionHandle startHandle, ConnectionHandle endHandle)
	{
		//Create a new permanent line
		var permanentLine = new Line2D();
		permanentLine.Width = _lineThickness;
		permanentLine.DefaultColor = _permanentLineColor;
		AddChild(permanentLine);
		
		//Store the new line
		var newLink = new CardConnection
		{
			StartHandle = startHandle,
			EndHandle = endHandle,
			Line = permanentLine
		};
		_permanentConnections.Add(newLink);
		
		//Clear the active line
		CancelConnection();
		
		//Update the line positions
		UpdateLinePosition(newLink);
	}
	
	//Update our lines if we detect a card with connections has moved
	private void OnCardMove(StoryCard card)
	{
		foreach (var link in _permanentConnections)
		{
			if (link.StartHandle.GetParent<StoryCard>() == card || link.EndHandle.GetParent<StoryCard>() == card)
			{
				UpdateLinePosition(link);
			}
		}
	}
	
	//Update line positions for the given link
	private void UpdateLinePosition(CardConnection link)
	{
		var startPos = ToLocalInManager(link.StartHandle.GetConnectionPosition());
		var endPos = ToLocalInManager(link.EndHandle.GetConnectionPosition());

		if (link.Line.GetPointCount() < 2)
		{
			link.Line.AddPoint(startPos);
			link.Line.AddPoint(endPos);
		}
		else
		{
			link.Line.SetPointPosition(0, startPos);
			link.Line.SetPointPosition(1, endPos);
		}
	}

	private Vector2 ToLocalInManager(Vector2 globalPos)
	{
		return GetGlobalTransform().AffineInverse() * globalPos;
	}
	
	//Cancel active conenctions and clear any drawn nodes
	private void CancelConnection()
	{
		_activeLine?.QueueFree(); //Delete the line object
		_activeLine = null;
		_startHandle = null;
	}
}

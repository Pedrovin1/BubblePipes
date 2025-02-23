using Godot;
using System;

public partial class SignalBus : Node
{
    public const string SignalBusPath = "/root/SignalBus";

    public static SignalBus self;

    [Signal]
    public delegate void LevelCompletedEventHandler(int level);

    [Signal]
    public delegate void LevelSelectedEventHandler(int level);

    [Signal]
    public delegate void AddItemToInventoryEventHandler(string itemJsonData);

    [Signal]
    public delegate void ItemSelectedEventHandler(string itemJson, Texture2D texture);


    public override void _Ready()
    {
        SignalBus.self = this;
    }
}

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
    public delegate void LevelLoadedEventHandler();

    [Signal]
    public delegate void AddItemToInventoryEventHandler(string itemJsonData);
    [Signal]
    public delegate void RemoveHeldItemFromInventoryEventHandler();

    [Signal]
    public delegate void ItemSelectedEventHandler(string itemJson, Sprite2D texture);

    [Signal]
    public delegate void ConfigurationsMenuToggledEventHandler(bool toggledOn);

     [Signal]
    public delegate void InventoryMenuToggledEventHandler(bool toggledOn);

    [Signal]
    public delegate void StartBlinkPipeFillingsEventHandler(Timer blinkTimer, int colorToBlinkEnum);

    [Signal]
    public delegate void StopBlinkPipeFillingsEventHandler(Timer blinkTimer);

    // [Signal]
    // public delegate void ObjectiveBubbleAnimationStartedEventHandler(double animationTime);


    public override void _Ready()
    {
        SignalBus.self = this;
    }
}

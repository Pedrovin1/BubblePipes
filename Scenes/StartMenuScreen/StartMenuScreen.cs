using Godot;
using System;
using System.Threading.Tasks;

public partial class StartMenuScreen : Node2D
{
    private Node mainScene;
    public override void _Ready()
    {
        this.mainScene = ResourceLoader.Load<PackedScene>("res://Scenes/MainGameWindow/main_game_window.tscn").Instantiate();
    }
    public void onStartButtonPressed()
    {   
        this.GetNode("/root").AddChild(mainScene);
        this.mainScene = null;
        this.QueueFree();
    }
}

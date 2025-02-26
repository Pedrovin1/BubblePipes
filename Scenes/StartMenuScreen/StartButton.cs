using Godot;
using System;

public partial class StartButton : Button
{
    private Sprite2D sprite;
    public override void _Ready()
    {
        this.sprite = (Sprite2D)GetChild(0);
    }
    public void onMouseEntered()
    {
        this.sprite.Frame = 1;
    }
    public void onMouseExited()
    {
        this.sprite.Frame = 0;
    }
}

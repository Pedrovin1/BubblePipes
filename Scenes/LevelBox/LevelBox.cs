using Godot;
using System;


public partial class LevelBox : Button
{
    public int levelNumber {get; private set;} = -1;

    private Label label;
    private Sprite2D buttonSprite;

    public override void _Ready()
    {
        this.label = (Label) FindChild("Label");
        this.label.Text = "";

        this.buttonSprite = (Sprite2D) FindChild("Sprite2D");
        this.buttonSprite.Frame = 0;
        
        this.Pressed += this.onPressed;
    }

    public void SetLevelNumber(int number)
    {
        this.levelNumber = number;
        this.UpdateSprite();
    }

    public void UpdateSprite()
    {
        this.label.Text = this.levelNumber.ToString();
    }

    public void onPressed()
    {
        //
    }
}

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

    public void SetLevelNumber(int number, bool unlocked = true)
    {
        this.levelNumber = number;
        this.UpdateSprite(unlocked);
    }

    private void UpdateSprite(bool unlocked)
    {
        this.label.Text = this.levelNumber.ToString();
        this.buttonSprite.Frame = 1;
        this.label.Visible = true;

        if(!unlocked)
        {
            this.buttonSprite.Frame = 0;
            this.label.Visible = false;
        }
    }

    public void onPressed()
    {
        //
    }
}

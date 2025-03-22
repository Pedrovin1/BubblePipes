using Godot;
using System;

public partial class LevelBox : Button
{
    public int levelNumber {get; private set;} = -1;

    private Label label;
    private Sprite2D buttonSprite;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        this.label = (Label) FindChild("Label");
        this.label.Text = "";

        this.buttonSprite = (Sprite2D) FindChild("Sprite2D");
        this.buttonSprite.Frame = 0;

        this.animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer");
        
        if(!this.IsConnected(Button.SignalName.Pressed, new Callable(this, MethodName.onPressed)))
        {
            this.Connect(Button.SignalName.Pressed, new Callable(this, MethodName.onPressed));
            this.Connect(Button.SignalName.MouseEntered, new Callable(this, MethodName.onMouseEntered));
            this.Connect(Button.SignalName.MouseExited, new Callable(this, MethodName.onMouseExited));
            GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.LevelCompleted, new Callable(this, MethodName.onLevelCompleted));
        }
    }

    public void onLevelCompleted(int level)
    {
        if(level == this.levelNumber)
        {
            this.FindChild("Checkmark").GetChild<CpuParticles2D>(2).Emitting = true;
        }
    }   

    private void onMouseEntered()
    {
        if(PlayerData.self.lastUnlockedLevel < this.levelNumber){ return; }

        this.buttonSprite.Frame = 2;

        this.label.AddThemeConstantOverride("outline_size", 2);
    }

    private void onMouseExited()
    {
        if(PlayerData.self.lastUnlockedLevel < this.levelNumber){ return; }

        this.label.RemoveThemeConstantOverride("outline_size");

        this.buttonSprite.Frame = 1;
    }

    public void SetLevelNumber(int number, bool unlocked = true)
    {
        this.levelNumber = number;
        this.UpdateSprite(unlocked);
    }

    public void PauseAnimation()
    {
        this.animationPlayer.Stop();
    }

    private void UpdateSprite(bool unlocked)
    {
        this.label.RemoveThemeConstantOverride("outline_size");

        ((Node2D)this.FindChild("Checkmark")).Show();
        this.label.Text = this.levelNumber.ToString();
        this.buttonSprite.Frame = 1;
        this.label.Visible = true;

        Random rng = new(DateTime.Now.Millisecond / (this.levelNumber % 10 + 1));
        if(!this.animationPlayer.IsPlaying())
        {
            this.animationPlayer.Play("Idle", customSpeed: rng.NextSingle() + 0.7f);
        }

        if(this.levelNumber >= PlayerData.self.lastUnlockedLevel)
        {
            ((Node2D)this.FindChild("Checkmark")).Hide();
        }

        if(!unlocked)
        {
            ((Node2D)this.FindChild("Checkmark")).Hide();
            this.buttonSprite.Frame = 0;
            this.label.Visible = false;
            this.animationPlayer.Stop();
        }
        
    }

    public void onPressed()
    {
        if(PlayerData.self.lastUnlockedLevel < this.levelNumber){ return; }
        
        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.LevelSelected, this.levelNumber);
    }
}

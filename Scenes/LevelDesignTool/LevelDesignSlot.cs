using Godot;

public partial class LevelDesignSlot : Button
{
    [Signal]
    public delegate void LevelDesignSlotPressedEventHandler(LevelDesignSlot slot);

    public int contentSampleIndex {get; set;} = 0;
    public int stateNumber {get; set;} = 0;

    public LiquidType color = LiquidType.Vazio;
    public LiquidType color2 = LiquidType.Vazio;


    // public override void _Pressed()
    // {
    //     this.EmitSignal(LevelDesignSlot.SignalName.LevelDesignSlotPressed, this);
    // }

    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseClick && mouseClick.Pressed)
        {
            switch(mouseClick.ButtonIndex)
            {
                case MouseButton.Left: this.EmitSignal(LevelDesignSlot.SignalName.LevelDesignSlotPressed, this); break;
                case MouseButton.Right: 
                    this.stateNumber++; stateNumber %= 4; 
                    this.GetChild<Node2D>(0).Rotate(Mathf.DegToRad(90f)); 
                    this.GetChild<Node2D>(1).Rotate(Mathf.DegToRad(90f)); 
                    break;
            }
        }
    }
    public void SetColor(Color color, int spriteIndex = 0)
    {
        this.GetChild<Node2D>(1).GetChild<Node2D>(spriteIndex).SelfModulate = color;
    }

    public void ResetRotation()
    {
        this.GetChild<Node2D>(0).GlobalRotation = 0;
        this.GetChild<Node2D>(1).GlobalRotation = 0;
    }

    public void ResetColor()
    {
        this.GetChild<Node2D>(0).SelfModulate = Color.Color8(255,255,255);
        this.GetChild<Node2D>(1).SelfModulate = Color.Color8(255,255,255);
    }

    public void ResetExtraSprites()
    {
        foreach(Node node in this.GetChild<Node2D>(1).GetChildren())
        {
            node.QueueFree();
        }
    }

    public void AddDetailSprite(Sprite2D sprite)
    {
        this.GetChild<Node2D>(1).AddChild(sprite);
        sprite.Owner = this;
        sprite.GlobalPosition = this.GetChild<Node2D>(1).GlobalPosition;
        sprite.Centered = true;
    }

    public void SetSprite(Sprite2D sprite)
    {
        this.GetChild<Sprite2D>(0).Show();
        this.GetChild<Sprite2D>(0).Texture = sprite.Texture;
        this.GetChild<Sprite2D>(0).Hframes = sprite.Hframes;
        this.GetChild<Sprite2D>(0).Frame = sprite.Frame;
    }

    public void HideSprite()
    {
        this.GetChild<Sprite2D>(0).Hide();
    }
}

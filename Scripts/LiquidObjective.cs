using Godot;
using System;
using System.Collections.Generic;


public partial class LiquidObjective : Button, ISlotInteractable
{
    private static readonly string ClassName = "LiquidObjective";

    [Signal]
    public delegate void ObjectiveSlotStateChangedEventHandler(LiquidObjective slotNode, bool correctlyFilled);

    [Export]
    public LiquidType requiredLiquid = LiquidType.Azul;
    public bool correctlyFilled {get; private set;} = false;
    public bool bubbleLocked {get; private set;} = false;
    public int[] bubbleLockedTilesIndexes {get; private set;}


    private AnimationPlayer animationNode;
    private Node2D extraDetails;
    private Sprite2D contentSprite;

    public Dictionary<Directions, SlotOutlet> outletStates = new()
    {
        {Directions.Cima,       new SlotOutlet()},
        {Directions.Direita,    new SlotOutlet()},
        {Directions.Baixo,      new SlotOutlet()},
        {Directions.Esquerda,   new SlotOutlet()}
    };

    public override void _Ready()
    {
        if(!this.IsConnected(Button.SignalName.Pressed, new Callable(this, MethodName.onClicked)))
        {
            this.Connect(Button.SignalName.Pressed, new Callable(this, MethodName.onClicked));
            this.Connect(Button.SignalName.MouseEntered, new Callable(this, MethodName.onMouseEntered));
            this.Connect(Button.SignalName.MouseExited, new Callable(this, MethodName.onMouseExited));
        }
        this.animationNode = (AnimationPlayer)FindChild("AnimationPlayer");
        
        this.contentSprite = (Sprite2D)FindChild("ContentFrame");
        this.contentSprite.Hframes = 5;
        this.contentSprite.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/LiquidObjectiveSprites.png");

        this.extraDetails = this.GetNode<Node2D>("./CenterContainer/Panel/ExtraDetails");
        this.ClearDetailSprites();

        this.outletStates[Directions.Cima].Opened = true;
        this.outletStates[Directions.Direita].Opened = true;
        this.outletStates[Directions.Baixo].Opened = true;
        this.outletStates[Directions.Esquerda].Opened = true;

        this.LoadBubbleLocks();
        this.UpdateDrawingState();
    }

    private void LoadBubbleLocks()
    {   
        if(this.bubbleLockedTilesIndexes == null || this.bubbleLockedTilesIndexes.Length == 0){ return; }

        const int frameOffset = -1;
        var bubbleLockScene = ResourceLoader.Load<PackedScene>("res://Scenes/BubbleLock/BubbleLock.tscn");
        
        foreach(int bubbleIndex in this.bubbleLockedTilesIndexes)
        {
            Node2D bubblelockNode = bubbleLockScene.Instantiate<Node2D>();
            bubblelockNode.GetChild<Sprite2D>(1).Frame = (int)this.requiredLiquid + frameOffset;
            bubblelockNode.Position = new Vector2(0f, 0f);
            bubblelockNode.GlobalRotation = 0;
            
            this.extraDetails.AddChild(bubblelockNode);
            bubblelockNode.Owner = this;
            bubblelockNode.Hide();
        }
    }

    private void onMouseEntered()
    {
        Color lineColor = GameUtils.LiquidColorsRGB[this.requiredLiquid];
        lineColor.A8 = 150;

        foreach(Node bubbleLock in this.extraDetails.GetChildren())
        {
            var animationNode = bubbleLock.GetChild<AnimationPlayer>(0);
            animationNode.ClearQueue();
            animationNode.Play("WobblingBubble");
            animationNode.Queue("Idle");
        }

        foreach(int lockedSlotIndex in this.bubbleLockedTilesIndexes)
        {
            var lockedSlotPosition = GetParent<Tabuleiro>().GetChild<Control>(lockedSlotIndex).GlobalPosition; //tightly coupled
            
            var lineNode = new Line2D()
            {
                Points = new Vector2[] {Vector2.Zero,  lockedSlotPosition - this.GlobalPosition},
                Width = 1,
                SelfModulate = lineColor,
                ZIndex = 7
            };
            this.extraDetails.AddChild(lineNode);
            lineNode.Owner = this;
        }


    }

    private void onMouseExited()
    {
        foreach(Node node in this.extraDetails.GetChildren())
        {
            if(node is Line2D){node.QueueFree();}
        }
    }

    public void PlayBubbleSpreadingAnimation(Vector2 boardGlobalPos)
    {
        if(this.bubbleLockedTilesIndexes == null || this.bubbleLockedTilesIndexes.Length == 0){ return; }

        double animationTime = 0.5d / ConfigsMenu.animationSpeedMultiplier;

        const int slotSize = 17;
        const int pivotOffset = 9;

        using Tween movementTween = this.GetTree().CreateTween();
        Random rng = new();

        for(int i = 0; i < this.extraDetails.GetChildCount(); i++)
        {
            var bubble = this.extraDetails.GetChild<Node2D>(i);
            var animationNode = (AnimationPlayer) bubble.FindChild("AnimationPlayer");
            bubble.Position = new Vector2(0f, 0f);

            int destinySlotIndex = this.bubbleLockedTilesIndexes[i];
            Vector2 finalPosition = new Vector2(destinySlotIndex % 5 * slotSize + boardGlobalPos.X + pivotOffset, 
                                                Mathf.Floor(destinySlotIndex / 5f) * slotSize + boardGlobalPos.Y + pivotOffset);
            
            movementTween.TweenCallback(Callable.From(bubble.Show));
            movementTween.TweenProperty(bubble, "global_position", finalPosition, animationTime)
                .SetTrans(Tween.TransitionType.Cubic);
            movementTween.TweenProperty(bubble, "global_rotation", 0, 0);
            movementTween.TweenCallback(Callable.From( () => animationNode.Play("Idle", customSpeed: rng.NextSingle() + 0.5f ) ) );
            movementTween.Play();
        }
    }

    public  void PlayBubbleReleasingAnimation()
    {
        double animationTime = 0.5d / ConfigsMenu.animationSpeedMultiplier;

        for(int i = 0; i < this.extraDetails.GetChildCount(); i++)
        {
            using Tween floatingTween = this.GetTree().CreateTween();

            var bubble = this.extraDetails.GetChild<Node2D>(i);
            ((AnimationPlayer)bubble.FindChild("AnimationPlayer")).Stop();

            floatingTween.TweenProperty(bubble, "global_position:y", -25f, animationTime)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Sine);

            floatingTween.TweenCallback(Callable.From(bubble.Hide));
            floatingTween.TweenProperty(bubble, "position", Vector2.Zero, 0f);
            floatingTween.TweenProperty(bubble, "global_rotation", 0, 0);
    
            floatingTween.Play();
        }
    }

    public void onClicked(){ return; }
    public void LockRotation()
    { 
        this.bubbleLocked = true;
        bool oldState = this.correctlyFilled;
        this.ResetOutletLiquids();
        this.UpdateDrawingState(oldState != this.correctlyFilled);
    }
    public void UnlockRotation()
    {  
        this.bubbleLocked = false; 
    }

    private void ClearDetailSprites()
    {
        var rootLiquidSprites = this.GetNode<Node2D>("./CenterContainer/Panel/RootLiquids");

        this.extraDetails.GlobalRotation = 0;
        
        foreach(Node node in rootLiquidSprites.GetChildren())
        {
            node.Free();
        }
        foreach(Node node in this.extraDetails.GetChildren())
        {
            node.Free();
        }
    }


    public void UpdateDrawingState(bool stateChanged = false)
    {
        this.contentSprite.Frame = (int)requiredLiquid - 1;

        if(this.correctlyFilled && stateChanged)
        {
            animationNode.Play("ContentLoopRotation");
        }
        else if(!this.correctlyFilled)
        {
            animationNode.Stop();  
            this.contentSprite.GlobalRotation = 0;
        }
    }

    public bool IsPlayingAnimation()
    {
        return this.animationNode.IsPlaying();
    }

    public void ResetTweens()
    {
        return;
    }

    public void ResetOutletLiquids(LiquidType defaultLiquid = LiquidType.Vazio)
    {
        foreach(var key in this.outletStates.Keys)
        {
            this.SetLiquid(key,defaultLiquid);
        }
    }

    public bool IsOpened(Directions outletPos)
    {
        return this.outletStates[outletPos].Opened;
    }

    public LiquidType GetLiquid(Directions outletPos)
    {
        return this.outletStates[outletPos].CurrentLiquid;
    }
    public void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        this.outletStates[outletPos].CurrentLiquid = liquid;

        bool oldState = this.correctlyFilled;
        
        this.correctlyFilled = false;

        if(this.bubbleLocked == false)
        {
            foreach(SlotOutlet outlet in this.outletStates.Values)
            {
                if(outlet.CurrentLiquid == requiredLiquid)
                {
                    this.correctlyFilled = true;
                }
            }
        }

        if(this.correctlyFilled != oldState)
        {
            this.EmitSignal(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this, this.correctlyFilled);
            this.UpdateDrawingState(stateChanged:true);
        }
    }

    public Directions[] GetConnections(Directions outletPos)
    {
        return this.outletStates[outletPos].Connections;
    }

     public virtual Godot.Collections.Dictionary<string, Variant> GetExportData()
    {
        return new Godot.Collections.Dictionary<string, Variant>
        {
            {"PipeScriptPath", GameUtils.ScriptPaths[LiquidObjective.ClassName]},
            
            {LiquidObjective.PropertyName.requiredLiquid, (int)this.requiredLiquid },
            {LiquidObjective.PropertyName.bubbleLockedTilesIndexes, this.bubbleLockedTilesIndexes}
        };
    }
    public virtual void ImportData(Godot.Collections.Dictionary<string, Variant> setupData)
    {
        foreach((string propertyName, var data) in setupData)
        {
            this.Set(propertyName, data);
        }
    }
}

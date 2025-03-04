using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;


public partial class LiquidObjective : Button, ISlotInteractable
{
    private static readonly string ClassName = "LiquidObjective";

    [Signal]
    public delegate void ObjectiveSlotStateChangedEventHandler(LiquidObjective slotNode, bool correctlyFilled);

    [Export]
    public LiquidType requiredLiquid = LiquidType.Azul;
    public bool correctlyFilled {get; private set;} = false;
    private bool bubbleLocked = false;
    public int[] bubbleLockedTilesIndexes {get; private set;}


    AnimationPlayer animationNode;
    Node2D extraDetails;

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
        Texture2D texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/bubbleLocks.png");
        
        foreach(int bubbleIndex in this.bubbleLockedTilesIndexes)
        {
            Sprite2D bubbleNode = new Sprite2D()
            {
                Texture = texture,
                Hframes = 5,
                Frame = (int)this.requiredLiquid + frameOffset,
                ZIndex = 5,
                Position = new Vector2(0f, 0f)
            };
            this.extraDetails.AddChild(bubbleNode);
            bubbleNode.Owner = this;
            bubbleNode.Hide();
        }
    }

    public void PlayBubbleSpreadingAnimation(Vector2 boardGlobalPos)
    {
        if(this.bubbleLockedTilesIndexes == null || this.bubbleLockedTilesIndexes.Length == 0){ return; }

        const int slotSize = 17;
        const int pivotOffset = 9;

        using Tween movementTween = this.GetTree().CreateTween();

        for(int i = 0; i < this.extraDetails.GetChildCount(); i++)
        {
            var bubble = this.extraDetails.GetChild<Sprite2D>(i);
            bubble.Position = new Vector2(0f, 0f);

            int destinySlotIndex = this.bubbleLockedTilesIndexes[i];
            Vector2 finalPosition = new Vector2(destinySlotIndex % 5 * slotSize + boardGlobalPos.X + pivotOffset, 
                                                Mathf.Floor(destinySlotIndex / 5f) * slotSize + boardGlobalPos.Y + pivotOffset);
            
            movementTween.TweenCallback(Callable.From(bubble.Show));
            movementTween.TweenProperty(bubble, "global_position", finalPosition, 0.5f)
                .SetTrans(Tween.TransitionType.Cubic);
            movementTween.Play();
        }
    }

    public void PlayBubbleReleasingAnimation()
    {
        using Tween floatingTween = this.GetTree().CreateTween();

        for(int i = 0; i < this.extraDetails.GetChildCount(); i++)
        {
            var bubble = this.extraDetails.GetChild<Sprite2D>(i);
            floatingTween.TweenProperty(bubble, "global_position:y", -25f, 0.5f)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Sine);

            floatingTween.TweenCallback(Callable.From(bubble.Hide));
            floatingTween.TweenProperty(bubble, "position", Vector2.Zero, 0f);
    
            floatingTween.Play();
        }
    }

    public void onClicked(){ return; }
    public void LockRotation()
    { 
        this.bubbleLocked = true; 
        this.ResetOutletLiquids();
        this.UpdateDrawingState();
    }
    public void UnlockRotation()
    {  
        this.bubbleLocked = false; 
    }

    private void ClearDetailSprites()
    {
        var rootLiquidSprites = this.GetNode<Node2D>("./CenterContainer/Panel/RootLiquids");
        
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
        else
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

        foreach(Directions connection in this.outletStates[outletPos].Connections)
        {
            this.outletStates[connection].CurrentLiquid = liquid;
        }

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
            this.EmitSignal(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this, liquid == this.requiredLiquid);
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

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class LiquidObjective : Button, ISlotInteractable
{
    private static readonly string ClassName = "LiquidObjective";

    [Signal]
    public delegate void ObjectiveSlotStateChangedEventHandler(LiquidObjective slotNode, bool correctlyFilled);

    [Export]
    public LiquidType requiredLiquid = LiquidType.Azul;
    private bool correctlyFilled = false;

    private Sprite2D contentSprite;
    private Tween loopRotationTween;

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
        
        this.contentSprite = (Sprite2D)FindChild("ContentFrame");
        this.contentSprite.Hframes = 5;
        this.contentSprite.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/LiquidObjectiveSprites.png");

        this.ClearDetailSprites();

        this.outletStates[Directions.Cima].Opened = true;
        this.outletStates[Directions.Direita].Opened = true;
        this.outletStates[Directions.Baixo].Opened = true;
        this.outletStates[Directions.Esquerda].Opened = true;

        this.UpdateDrawingState();
    }

    public void onClicked()
    {
        return;
    }

    private void ClearDetailSprites()
    {
        var rootLiquidSprites = this.GetNode<Node2D>("./CenterContainer/Panel/RootLiquids");
        var extraDetails = this.GetNode<Node2D>("./CenterContainer/Panel/ExtraDetails");
        
        foreach(Node node in rootLiquidSprites.GetChildren())
        {
            node.Free();
        }
        foreach(Node node in extraDetails.GetChildren())
        {
            node.Free();
        }
    }


    public void UpdateDrawingState(bool stateChanged = false)
    {
        this.contentSprite.Frame = (int)requiredLiquid - 1;

        if(this.loopRotationTween != null)
        {
            this.loopRotationTween.Kill();
        }

        this.loopRotationTween = GetTree().CreateTween().SetLoops();
        if(this.correctlyFilled && stateChanged)
        {
            loopRotationTween.TweenProperty(this.contentSprite, "rotation", Mathf.DegToRad(360), 4.5f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Linear);
            loopRotationTween.TweenProperty(this.contentSprite, "rotation", Mathf.DegToRad(0), 0f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Linear);
            loopRotationTween.Play();
        }
        else
        {
            loopRotationTween.Kill();  
            this.contentSprite.GlobalRotation = 0;
        }
    }

    public bool IsPlayingAnimation()
    {
        return this.loopRotationTween.IsRunning();
    }

    public void ResetTweens()
    {
        if(loopRotationTween != null)
        {
             loopRotationTween.Kill();
        }
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
        foreach(SlotOutlet outlet in this.outletStates.Values)
        {
            if(outlet.CurrentLiquid == requiredLiquid)
            {
                this.correctlyFilled = true;
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

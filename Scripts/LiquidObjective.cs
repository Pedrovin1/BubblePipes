using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class LiquidObjective : Button, ISlotInteractable
{
    [Signal]
    public delegate void ObjectiveSlotStateChangedEventHandler(LiquidObjective slotNode, bool correctlyFilled);

    [Export]
    public LiquidType requiredLiquid = LiquidType.Azul;
    private bool correctlyFilled = false;

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
        this.Pressed += this.onClicked;
        this.contentSprite = (Sprite2D)FindChild("ContentFrame");
        this.contentSprite.Hframes = 5;
        this.contentSprite.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/LiquidObjectiveSprites.png");

        this.outletStates[Directions.Cima].Opened = true; //temporário!

        this.UpdateDrawingState();
    }

    public void onClicked()
    {
        return;
    }


    public void UpdateDrawingState()
    {
        this.contentSprite.GlobalRotation = 0;

        this.contentSprite.Frame = (int)requiredLiquid - 1;
        
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
        this.correctlyFilled = liquid == this.requiredLiquid;

        if(this.correctlyFilled != oldState)
        {
            this.EmitSignal(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this, liquid == this.requiredLiquid);
        }
        
    }

    public Directions[] GetConnections(Directions outletPos)
    {
        return this.outletStates[outletPos].Connections;
    }
}

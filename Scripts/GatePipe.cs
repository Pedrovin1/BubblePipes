using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GatePipe : BasePipe
{
    [Export]
    private LiquidType gateLockColor = LiquidType.Azul;
    private bool gateUnlocked = false;

    protected override void LoadExtraDetails()
    {
        Line2D line = new Line2D
        {
            Points = new Vector2[] { new Vector2(-8f, 0f), new Vector2(8f, 0f)},
            Width = 4,
            SelfModulate = GameUtils.LiquidColorsRGB[gateLockColor]
        };
        this.extraDetails.AddChild(line);
        line.Owner = this;
    }


    public override void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        if(this.outletStates[outletPos].Opened && this.outletStates[outletPos].Connections.Length <= 0) //gate "lever" position (it has edge cases but are highly unlikely to happen)
        {
            this.gateUnlocked = (liquid == this.gateLockColor);
        }

        base.SetLiquid(outletPos, liquid);
    }

    public override void UpdateDrawingState()
    {
        Line2D lineNode = this.extraDetails.GetChild<Line2D>(0);

        lineNode.Points = new Vector2[]{ lineNode.Points[0], new(8f, 0f) }; 
        this.pipeSprite.Frame = 0;
        if(gateUnlocked)
        {
            lineNode.Points = new Vector2[]{ lineNode.Points[0], new(-4f, 0f) }; 
            this.pipeSprite.Frame = 1;
        }

        base.UpdateDrawingState();
    }

    public override bool IsOpened(Directions outletPos)
    {
        this.UpdateGateLockState();
        
        if(this.outletStates[outletPos].Opened && this.outletStates[outletPos].Connections.Length <= 0) //gate "lever" position
        { return true; }

           
        if(!gateUnlocked){ return false; }

        return base.IsOpened(outletPos);
    }

    public override void ResetOutletLiquids(LiquidType defaultLiquid = LiquidType.Vazio)
    {
        this.gateUnlocked = false;

        base.ResetOutletLiquids(defaultLiquid);
    }

    private void UpdateGateLockState()
    {
        Tabuleiro board = (Tabuleiro)GetParent();

        foreach((Directions position, SlotOutlet outlet) in this.outletStates)
        {
            if(outlet.Opened && outlet.Connections.Length <= 0)
            {
                if(board.isMoveInsideBounds(this.GetIndex(), position, out ISlotInteractable neighborNode))
                {
                    this.gateUnlocked = neighborNode.GetLiquid(GameUtils.OppositeSide(position)) == this.gateLockColor;
                }
            }
        }
    }
}
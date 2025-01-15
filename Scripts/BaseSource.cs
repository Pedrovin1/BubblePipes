using Godot;
using System;
using System.Collections.Generic;

public partial class BaseSource : Button, ISlotInteractable
{
    [Export]
    LiquidSourceResource SourceResource;

    [Export]
    bool canRotate = false;

    [Export]
    public Godot.Collections.Dictionary<Directions, bool> outletOpeningStates;

    [Export]
    public Godot.Collections.Dictionary<Directions, LiquidType> outletLiquids;

    private Sprite2D spriteNode;

    public override void _Ready()
    {
        spriteNode = (Sprite2D) this.FindChild("ContentFrame");
        this.UpdateDrawingState();
    }

    public bool IsOpened(Directions outletPos)
    {
        return outletOpeningStates[outletPos];
    }

    public LiquidType GetLiquid(Directions outletPos)
    {
        return outletLiquids[outletPos];
    }
    public void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        return;
    }

    public Directions[] GetConnections(Directions outletPos)
    {
        return Array.Empty<Directions>();
    }

    public void UpdateDrawingState()
    {
        this.spriteNode.Texture = SourceResource.spriteFile;
        //sprite.Hframes = ;
        this.spriteNode.Frame = SourceResource.pipeSpriteFrame;

        this.outletOpeningStates = this.SourceResource.openingStates[0];
        this.outletLiquids = this.SourceResource.outletSourceLiquidTypes[0];
    }
}

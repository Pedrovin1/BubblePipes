using Godot;
using System;
using System.Collections.Generic;

public partial class BaseSource : Button
{
    [Export]
    LiquidSourceResource SourceResource;

    [Export]
    bool canRotate = false;

    [Export]
    public Godot.Collections.Dictionary<Directions, bool> outletOpeningStates;

    [Export]
    public Godot.Collections.Dictionary<Directions, LiquidType> outletLiquids;

    public override void _Ready()
    {
        var sprite = (Sprite2D) this.FindChild("ContentFrame");
        sprite.Texture = SourceResource.spriteFile;
        //sprite.Hframes = ;
        sprite.Frame = SourceResource.pipeSpriteFrame;

        this.outletOpeningStates = this.SourceResource.openingStates[0];
        this.outletLiquids = this.SourceResource.outletSourceLiquidTypes[0];
    }
}

using Godot;
using Godot.Collections;
using System;

public partial class LiquidSourceResource : Resource
{
    [Export]
    public Texture2D spriteFile;

    [Export]
    public byte pipeSpriteFrame;

    [Export]
    public byte statesAmount = 0;

    [Export]
    public Dictionary<byte, Dictionary<Directions, bool>> openingStates;

    [Export]
    public Dictionary<byte, Dictionary<Directions, LiquidType>> outletSourceLiquidTypes;
}
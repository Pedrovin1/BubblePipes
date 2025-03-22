using Godot;
using Godot.Collections;

public partial class PipeResource : Resource
{
    [Export]
    public byte binaryOpeningStates = 0b_0000_0000;

    [Export]
    public Dictionary<Directions, Array<Directions>> baseOutletConnections;

    [Export]
    public byte statesAmount = 4;


    [Export]
    public Texture2D liquidSegmentsSpriteFile;
    [Export]
    public int LiquidSegmentsHframes;
    [Export]
    public Array<Vector2I> baseLiquidSegmentLayout; //(numero do frame, outletPosition que ele pertence)

    
    [Export]
    public Texture2D pipeSpriteFile;

    [Export]
    public int pipeSpriteHframes;

    [Export]
    public byte pipeSpriteFrame;

    
}


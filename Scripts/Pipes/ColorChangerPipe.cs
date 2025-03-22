using Godot;

public partial class ColorChangerPipe : BasePipe
{
    private static readonly string ClassName = "ColorChangerPipe";

    private LiquidType requiredColor = LiquidType.Roxo;
    private Directions positionRequiredColor = Directions.Cima;

    private LiquidType transformedColor = LiquidType.Azul;
    private Directions positionTransformedColor = Directions.Baixo;

    [Export]
    private bool Bidirectional = true;

    public override Godot.Collections.Dictionary<string, Variant> GetExportData()
    {
        Godot.Collections.Dictionary<string, Variant> dataDict = new Godot.Collections.Dictionary<string, Variant>
        {
            {"PipeScriptPath", GameUtils.ScriptPaths[ColorChangerPipe.ClassName]},

            {"requiredColor",            (int)this.requiredColor},
            {"positionRequiredColor",    (int)this.positionRequiredColor},
            {"transformedColor",         (int)this.transformedColor},
            {"positionTransformedColor", (int)this.positionTransformedColor},
            {"Bidirectional",                 this.Bidirectional}
        };

        dataDict.Merge(base.GetExportData());

        return dataDict;
    }

    protected override void LoadExtraDetails()
    {
        Sprite2D detailSprite = new();
        detailSprite.Texture = this.pipeResource.pipeSpriteFile;
        detailSprite.Hframes = this.pipeResource.pipeSpriteHframes;
        detailSprite.Frame = 1;
        detailSprite.SelfModulate = GameUtils.LiquidColorsRGB[requiredColor];
        this.extraDetails.AddChild(detailSprite);
        detailSprite.Owner = this;

        detailSprite = (Sprite2D)detailSprite.Duplicate();
        detailSprite.Frame = 2;
        detailSprite.SelfModulate = GameUtils.LiquidColorsRGB[transformedColor];
        this.extraDetails.AddChild(detailSprite);
    }

    public override void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        this.outletStates[outletPos].CurrentLiquid = liquid;

        Directions requiredOutletPos = (Directions)(((int)positionRequiredColor + this.stateNumber) % 4);
        if(this.Bidirectional)
        {
            if(outletPos == requiredOutletPos && liquid == requiredColor)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = transformedColor;
                return;
            }
            else{
                requiredOutletPos = (Directions)(((int)positionTransformedColor + this.stateNumber) % 4);
                if(outletPos == requiredOutletPos && liquid == transformedColor)
                {
                    this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = requiredColor;
                    return;
                }
                else
                {
                    this.outletStates[outletPos].CurrentLiquid = LiquidType.Vazio;
                    this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = LiquidType.Vazio;
                }
            }
            
        }
        else
        {
            if(outletPos == requiredOutletPos && liquid == requiredColor)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = transformedColor;
            }
            else
            {
                    this.outletStates[outletPos].CurrentLiquid = LiquidType.Vazio;
                    this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = LiquidType.Vazio;
            }
        }
    }
}

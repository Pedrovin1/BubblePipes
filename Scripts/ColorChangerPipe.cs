using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ColorChangerPipe : BasePipe
{
    private (Directions, LiquidType) requiredColor = new (Directions.Cima, LiquidType.Roxo);
    private (Directions, LiquidType) transformedColor = new (Directions.Baixo, LiquidType.Azul);

    [Export]
    private bool Bidirectional = true;

    protected override void LoadExtraDetails()
    {
        Sprite2D detailSprite = new();
        detailSprite.Texture = this.pipeResource.pipeSpriteFile;
        detailSprite.Hframes = this.pipeResource.pipeSpriteHframes;
        detailSprite.Frame = 1;
        detailSprite.SelfModulate = GameUtils.LiquidColorsRGB[requiredColor.Item2];
        this.extraDetails.AddChild(detailSprite);
        detailSprite.Owner = this;

        detailSprite = (Sprite2D)detailSprite.Duplicate();
        detailSprite.Frame = 2;
        detailSprite.SelfModulate = GameUtils.LiquidColorsRGB[transformedColor.Item2];
        this.extraDetails.AddChild(detailSprite);
    }

    public override void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        this.outletStates[outletPos].CurrentLiquid = liquid;

        Directions requiredOutletPos = (Directions)(((int)requiredColor.Item1 + this.stateNumber) % 4);
        if(this.Bidirectional)
        {
            if(outletPos == requiredOutletPos && liquid == requiredColor.Item2)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = transformedColor.Item2;
                return;
            }
            else{
                requiredOutletPos = (Directions)(((int)transformedColor.Item1 + this.stateNumber) % 4);
                if(outletPos == requiredOutletPos && liquid == transformedColor.Item2)
                {
                    this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = requiredColor.Item2;
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
            if(outletPos == requiredOutletPos && liquid == requiredColor.Item2)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = transformedColor.Item2;
            }
            else
            {
                    this.outletStates[outletPos].CurrentLiquid = LiquidType.Vazio;
                    this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = LiquidType.Vazio;
            }
        }
    }
}

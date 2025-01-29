using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ColorChangerPipe : BasePipe
{
    private (Directions, LiquidType) requiredColor = new (Directions.Cima, LiquidType.Azul);
    private (Directions, LiquidType) transformedColor = new (Directions.Baixo, LiquidType.Rosa);

    [Export]
    private bool Bidirectional = true; 

    public override void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        this.outletStates[outletPos].CurrentLiquid = liquid;

        Directions requiredOutletPos = (Directions)(((int)requiredColor.Item1 + this.stateNumber) % 4);
        if(this.Bidirectional)
        {
            if(outletPos == requiredOutletPos && liquid == requiredColor.Item2)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = transformedColor.Item2;
            }

            requiredOutletPos = (Directions)(((int)transformedColor.Item1 + this.stateNumber) % 4);
            if(outletPos == requiredOutletPos && liquid == transformedColor.Item2)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = requiredColor.Item2;
            }
        }
        else
        {
            if(outletPos == requiredOutletPos && liquid == requiredColor.Item2)
            {
                this.outletStates[GameUtils.OppositeSide(outletPos)].CurrentLiquid = transformedColor.Item2;
            } 
        }
    }
}

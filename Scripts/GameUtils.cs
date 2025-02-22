using System;
using System.Collections.Generic;
using Godot;

public static class GameUtils
{
    public static readonly string PipeSlotScenePath = "res://Scenes/PipeSlot/PipeSlot.tscn";
    public readonly static Dictionary<LiquidType, Color> LiquidColorsRGB = new Dictionary<LiquidType, Color>
    {
        {LiquidType.Vazio, Color.Color8(11, 11, 11)     },
        {LiquidType.Azul, Color.Color8(121, 224, 207)   },
        {LiquidType.Roxo, Color.Color8(113, 108, 176)   },
        {LiquidType.Rosa, Color.Color8(224, 122, 179)   },
        {LiquidType.Branco, Color.Color8(255, 255, 255) },
        {LiquidType.Amarelo, Color.Color8(255, 208, 144)}
    };

    public readonly static Dictionary<string, string> ScriptPaths = new Dictionary<string, string>
    {
        {"BaseSource", "res://Scripts/Pipes/BaseSource.cs"},
        {"LiquidObjective","res://Scripts/LiquidObjective.cs"},
        {"BasePipe", "res://Scripts/Pipes/BasePipe.cs"},
        {"ColorChangerPipe", "res://Scripts/Pipes/ColorChangerPipe.cs"},
        {"GatePipe", "res://Scripts/Pipes/GatePipe.cs"},
        {"ChangeablePipe","res://Scripts/Pipes/ChangeablePipe.cs"}
    };

    public static Directions OppositeSide(Directions direction)
    {
        const int outletPositions_Quantity = 4;
        const int oppositeSideOffset = outletPositions_Quantity / 2;

        return (Directions) ( ((int)direction + oppositeSideOffset) % outletPositions_Quantity ); 
    }

    public static bool GetBitFromByte(byte Byte, int bitPosition, bool startFromLeft = false)
    {
        int offset = startFromLeft ? 8 - bitPosition : bitPosition - 1;
        bool bit = (Byte & (0b_1 << (offset))) != 0;

        return bit;
    } 
}
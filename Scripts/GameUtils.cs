using System;
using Godot;

public static class GameUtils
{

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
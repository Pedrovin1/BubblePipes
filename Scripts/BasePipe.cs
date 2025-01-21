using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public class SlotOutlet
{

    public bool Opened {get; set;} = false;
    public LiquidType CurrentLiquid {get; set;} = LiquidType.Vazio;
    public Directions[] Connections {get; set;} = Array.Empty<Directions>();

    public SlotOutlet(){}
    public SlotOutlet(bool opened, LiquidType liquid, Directions[] connections)
    {
        this.Opened = opened;
        this.CurrentLiquid = liquid;
        this.Connections = connections;
    }
}

public partial class BasePipe : Button, ISlotInteractable
{

    [Export]
    private PipeResource pipeResource;

    [Export]
    private byte stateNumber = 0;

    public bool canRotate = true;

    private Sprite2D pipeSprite;

    public Dictionary<Directions, SlotOutlet> outletStates = new()
    {
        {Directions.Cima,       new SlotOutlet()},
        {Directions.Direita,    new SlotOutlet()},
        {Directions.Baixo,      new SlotOutlet()},
        {Directions.Esquerda,   new SlotOutlet()}
    };

    public override void _Ready()
    {
        this.Pressed += this.onClicked;

        // -- Desenha o Pipe -- //
        this.pipeSprite = (Sprite2D)FindChild("ContentFrame"); 
        this.pipeSprite.Texture = pipeResource.spriteFile;
        this.pipeSprite.Hframes = pipeResource.Hframes;
        this.pipeSprite.Frame = pipeResource.pipeSpriteFrame;

        // -- Carrega os detalhes do Pipe -- //
        this.UpdateOutletOpeningStates();
        this.UpdateOutletConnections();
        this.UpdateDrawingState();
    }

    public void onClicked()
    {
        if(!canRotate){ return; }

        this.stateNumber = (byte) ((stateNumber + 1) % pipeResource.statesAmount);
        this.UpdateOutletOpeningStates();
        this.UpdateOutletConnections();
        this.ResetOutletLiquids();

        this.UpdateDrawingState();

        GD.Print(this.outletStates[Directions.Cima].Opened);
    }


    public void ChangePipeContent(PipeResource newPipe, byte state = 0, bool canRotate = true)
    {
        this.pipeResource = newPipe;
        this.stateNumber = (byte) (state % newPipe.statesAmount);
        this.canRotate = canRotate;

        this.pipeSprite.Texture = newPipe.spriteFile;
        this.pipeSprite.Hframes = pipeResource.Hframes;
        this.pipeSprite.Frame = newPipe.pipeSpriteFrame;

        this.UpdateOutletOpeningStates();
        this.UpdateOutletConnections();
        this.UpdateDrawingState();
    }

    public void UpdateDrawingState()
    {
        const int Directions_Quantity = 4;
        this.pipeSprite.GlobalRotation = 0;

        float radiansRotation = (float) (this.stateNumber % Directions_Quantity / 2d * Math.PI);
        this.pipeSprite.Rotate(radiansRotation);

        //pintar com cor do liquid TEMPORARIO
        foreach(var value in this.outletStates.Values)
        {
            if(value.Opened)
            {
                switch(value.CurrentLiquid)
                {
                    case LiquidType.Azul: this.pipeSprite.SelfModulate = Color.Color8(0,255,255); return;
                    case LiquidType.Amarelo: this.pipeSprite.SelfModulate = Color.Color8(255,255,0); return;
                    case LiquidType.Vazio: this.pipeSprite.SelfModulate = Color.Color8(255,255,255); break;
                }
            }
        }
    }

    private void UpdateOutletOpeningStates()
    {
        const int Directions_Quantity = 4;
        const int infoBitStartPos = 5;

        byte baseBinaryOpeningState = this.pipeResource.binaryOpeningStates;
        byte currentBinaryOpeningState = (byte)(baseBinaryOpeningState >> this.stateNumber);

        for(int i = 0; i < Directions_Quantity; i++)
        {
            Directions outletPos = (Directions)i;
            this.outletStates[outletPos].Opened = GetBitFromByte(currentBinaryOpeningState, infoBitStartPos + i, true);
        }
    }


    //Utils Function
    public bool GetBitFromByte(byte Byte, int bitPosition, bool startFromLeft = false)
    {
        int offset = startFromLeft ? 8 - bitPosition : bitPosition - 1;
        bool bit = (Byte & (0b_1 << (offset))) != 0;

        return bit;
    } 


    private void UpdateOutletConnections()
    {
        const int Directions_Quantity = 4;
        List<Directions> updatedConnections = new();

        for(int i = 0; i < Directions_Quantity; i++)
        {
            updatedConnections = new();

            Directions currentOutletPos = (Directions)i;
            Directions newOutletPosition = (Directions)((i + this.stateNumber) % Directions_Quantity);

            foreach(var connection in this.pipeResource.baseOutletConnections[currentOutletPos])
            {
                 updatedConnections.Add((Directions) (((int)connection + this.stateNumber) % Directions_Quantity) );
            }

            this.outletStates[newOutletPosition].Connections = updatedConnections.ToArray();
        }
    }

    public void ResetOutletLiquids(LiquidType defaultLiquid = LiquidType.Vazio)
    {
        foreach(var key in this.outletStates.Keys)
        {
            this.outletStates[key].CurrentLiquid = defaultLiquid;
        }
    }

    public bool IsOpened(Directions outletPos)
    {
        return this.outletStates[outletPos].Opened;
    }

    public LiquidType GetLiquid(Directions outletPos)
    {
        return this.outletStates[outletPos].CurrentLiquid;
    }
    public void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        this.outletStates[outletPos].CurrentLiquid = liquid;

        foreach(Directions connection in this.outletStates[outletPos].Connections)
        {
            this.outletStates[connection].CurrentLiquid = liquid;
        }
    }

    public Directions[] GetConnections(Directions outletPos)
    {
        return this.outletStates[outletPos].Connections;
    }
}

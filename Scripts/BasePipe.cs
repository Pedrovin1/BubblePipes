using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public class SlotOutlet
{
    public bool Opened {get; set;} = false;
    public LiquidType CurrentLiquid {get; set;} = LiquidType.Vazio;
    public Directions[] Connections {get; set;} = null;

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
        //this.pipeSprite.Hframes = pipeResource.spriteFileHframes
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
    }


    public void ChangePipeContent(PipeResource newPipe, byte state = 0, bool canRotate = true)
    {
        this.pipeResource = newPipe;
        this.stateNumber = (byte) (state % newPipe.statesAmount);
        this.canRotate = canRotate;

        this.pipeSprite.Texture = newPipe.spriteFile;
        //this.pipeSprite.Hframes = pipeResource.spriteFileHframes
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

        for(int i = 0; i < Directions_Quantity; i++)
        {
            Directions outletPosition = (Directions)i;
            this.outletStates[outletPosition].Opened = pipeResource.openingStates[this.stateNumber][outletPosition];
        }
    }
    private void UpdateOutletConnections()
    {
        const int Directions_Quantity = 4;

        for(int i = 0; i < Directions_Quantity; i++)
        {
            Directions outletPosition = (Directions)i;
            this.outletStates[outletPosition].Connections = pipeResource.outletConnections[this.stateNumber][outletPosition].ToArray();
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

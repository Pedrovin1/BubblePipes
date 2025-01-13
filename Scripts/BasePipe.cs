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

public partial class BasePipe : Button
{

    [Export]
    PipeResource pipeResource;

    [Export]
    byte stateNumber = 0;

    public bool canRotate = true;

    Sprite2D pipeSprite;

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

    private void UpdateDrawingState()
    {
        const int Directions_Quantity = 4;
        this.pipeSprite.GlobalRotation = 0;

        float radiansRotation = (float) (this.stateNumber % Directions_Quantity / 2d * Math.PI);
        this.pipeSprite.Rotate(radiansRotation);

        //pintar com cor do liquid
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

    private void ResetOutletLiquids(LiquidType defaultLiquid = LiquidType.Vazio)
    {
        foreach(var key in this.outletStates.Keys)
        {
            this.outletStates[key].CurrentLiquid = defaultLiquid;
        }
    }
}

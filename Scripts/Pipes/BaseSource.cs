using Godot;
using System;
using System.Collections.Generic;

public partial class BaseSource : BasePipe
{
    private static readonly string ClassName = "BaseSource";

    [Export]
    LiquidType sourceLiquid = LiquidType.Azul;

    public override void _Ready()
    {
        this.Pressed += this.onClicked;
        this.canRotate = false;

        // -- Desenha o Pipe -- //
        this.pipeSprite = (Sprite2D)FindChild("ContentFrame"); 
        this.pipeSprite.Texture = pipeResource.pipeSpriteFile;
        this.pipeSprite.Hframes = pipeResource.pipeSpriteHframes;

        const int enumOffset = 1;
        this.pipeSprite.Frame = ((int)sourceLiquid) - enumOffset;

        foreach(SlotOutlet slotOutlet in this.outletStates.Values)
        {
            slotOutlet.CurrentLiquid = sourceLiquid;
        }

        this.rootLiquidSprites = this.GetNode<Node2D>("./CenterContainer/Panel/RootLiquids");
        this.extraDetails = this.GetNode<Node2D>("./CenterContainer/Panel/ExtraDetails");
        this.ClearDetailSprites();

        // -- Carrega os detalhes do Pipe -- //
        this.UpdateOutletOpeningStates();
        this.UpdateOutletConnections();
        this.UpdateDrawingState();
    }

    public override Godot.Collections.Dictionary<string, Variant> GetExportData()
    {
        Godot.Collections.Dictionary<string, Variant> dataDict = new Godot.Collections.Dictionary<string, Variant>
        {
            {"PipeScriptPath", GameUtils.ScriptPaths[BaseSource.ClassName]},
            
            {"sourceLiquid", (int)this.sourceLiquid}
        };

        dataDict.Merge(base.GetExportData());

        return dataDict;
    }

    public override void SetLiquid(Directions outletPos, LiquidType liquid)
    {
        return;
    }

    public override void ResetOutletLiquids(LiquidType defaultLiquid = LiquidType.Vazio)
    {
        return;
    }
}

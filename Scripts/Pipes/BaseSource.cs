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
        if(!this.IsConnected(Button.SignalName.Pressed, new Callable(this, MethodName.onClicked)))
        {
            this.Connect(Button.SignalName.Pressed, new Callable(this, MethodName.onClicked));
        }
        
        this.canRotate = false;

        // -- Desenha o Pipe -- //
        this.pipeSprite = (AnimatedSprite2D)FindChild("AnimatedSprite2D");
        ((Node2D)FindChild("ContentFrame")).Hide();

        AnimatedSprite2D sprite = (AnimatedSprite2D)this.pipeSprite;
        sprite.SpriteFrames = ResourceLoader.Load<SpriteFrames>("res://Assets/Sprites/LiquidAnimations.tres");
        sprite.Animation = sourceLiquid.ToString();
        sprite.Offset = new Vector2(-8,-7);
        sprite.Position = new Vector2(9, 9);
        sprite.Centered = false;
        sprite.ZIndex = 2;

        //const int enumOffset = 1;
        //this.pipeSprite.Frame = ((int)sourceLiquid) - enumOffset;

        foreach(SlotOutlet slotOutlet in this.outletStates.Values)
        {
            slotOutlet.CurrentLiquid = sourceLiquid;
        }

        this.rootLiquidSprites = this.GetNode<Node2D>("./CenterContainer/Panel/RootLiquids");
        this.extraDetails = this.GetNode<Node2D>("./CenterContainer/Panel/ExtraDetails");
        this.ClearDetailSprites();

        sprite.Play(); //depois de resetar os detail sprites
        sprite.Show();

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

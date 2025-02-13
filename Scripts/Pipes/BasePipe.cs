using Godot;
using System;
using System.Collections.Generic;

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
    private static readonly string ClassName = "BasePipe";

    [Export]
    protected PipeResource pipeResource = null;

    [Export]
    protected byte stateNumber = 0;

    public bool canRotate = true;

    protected Node2D pipeSprite;
    protected Node2D rootLiquidSprites;
    protected Node2D extraDetails;
    protected bool isPlayingAnimation = false;

    public Dictionary<Directions, SlotOutlet> outletStates = new()
    {
        {Directions.Cima,       new SlotOutlet()},
        {Directions.Direita,    new SlotOutlet()},
        {Directions.Baixo,      new SlotOutlet()},
        {Directions.Esquerda,   new SlotOutlet()}
    };

    public override void _Ready()
    {
        if(!this.IsConnected(Button.SignalName.Pressed, new Callable(this, MethodName.onClicked)))
        {
            this.Connect(Button.SignalName.Pressed, new Callable(this, MethodName.onClicked));
        }

        if(this.pipeResource == null){ this.pipeResource = ResourceLoader.Load<PipeResource>("res://Assets/Resources/Pipe0_empty.tres"); }

        this.pipeSprite = (Sprite2D)FindChild("ContentFrame");
        Sprite2D sprite = (Sprite2D)this.pipeSprite;
        sprite.Texture = pipeResource.pipeSpriteFile;
        sprite.Hframes = pipeResource.pipeSpriteHframes;
        sprite.Frame = pipeResource.pipeSpriteFrame;
        sprite.Show();

        this.rootLiquidSprites = this.GetNode<Node2D>("./CenterContainer/Panel/RootLiquids");
        this.extraDetails = this.GetNode<Node2D>("./CenterContainer/Panel/ExtraDetails");
        this.ClearDetailSprites();


        foreach(Vector2I liquidSpriteInfo in this.pipeResource.baseLiquidSegmentLayout)
        {
            Sprite2D liquidNode = new Sprite2D
            {
                Texture = this.pipeResource.liquidSegmentsSpriteFile,
                Hframes = this.pipeResource.LiquidSegmentsHframes,
                Frame = liquidSpriteInfo[0],
                //SelfModulate = GameUtils.LiquidColorsRGB[LiquidType.Vazio]
            };

            rootLiquidSprites.AddChild(liquidNode);
            liquidNode.Owner = this;
        }

        this.LoadExtraDetails();

        // -- Carrega os detalhes do Pipe -- //
        this.UpdateOutletOpeningStates();
        this.UpdateOutletConnections();
        this.ResetOutletLiquids();

        this.UpdateDrawingState();
    }

    protected virtual void LoadExtraDetails()
    {
        return;
    }

    protected virtual void ClearDetailSprites()
    {
        foreach(Node node in this.rootLiquidSprites.GetChildren())
        {
            node.Free();
        }
        foreach(Node node in this.extraDetails.GetChildren())
        {
            node.Free();
        }

        var animSprite = (AnimatedSprite2D)FindChild("AnimatedSprite2D");
        animSprite.Stop();
        animSprite.Hide();
    }

    public void onClicked()
    {
        if(!canRotate || this.IsPlayingAnimation()){ return; }

        this.stateNumber = (byte) ((stateNumber + 1) % pipeResource.statesAmount);
        this.UpdateOutletOpeningStates();
        this.UpdateOutletConnections();
        this.ResetOutletLiquids();

        this.UpdateDrawingState(animate:true);
    }


    public void ChangePipeContent(PipeResource newPipe, byte state = 0, bool canRotate = true)
    {
        // this.pipeResource = newPipe;
        // this.stateNumber = (byte) (state % newPipe.statesAmount);
        // this.canRotate = canRotate;

        // this.pipeSprite.Texture = newPipe.pipeSpriteFile;
        // this.pipeSprite.Hframes = pipeResource.pipeSpriteHframes;
        // this.pipeSprite.Frame = newPipe.pipeSpriteFrame;

        // //TODO: atualizar os pipe filling sprites2D

        // this.UpdateOutletOpeningStates();
        // this.UpdateOutletConnections();
        // this.UpdateDrawingState();
    }
    public bool IsPlayingAnimation()
    {
        return this.isPlayingAnimation;
    }

    public void ResetTweens()
    {
        return;
    }
    public virtual void UpdateDrawingState(bool animate = false)
    {
        const int Directions_Quantity = 4;
        float radiansRotation;

        if(animate)
        {   
            this.isPlayingAnimation = true;

            radiansRotation = Godot.Mathf.DegToRad(this.stateNumber * 90);
            if(this.stateNumber == 0)
            { 
                radiansRotation = Godot.Mathf.DegToRad(360);
            }

            using var rotationTween = this.GetTree().CreateTween().SetParallel(true); //memory leak here somehow, ~50kb each time
            rotationTween.TweenProperty(this.pipeSprite, "rotation", radiansRotation, 1.0f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
            rotationTween.TweenProperty(this.rootLiquidSprites, "rotation", radiansRotation, 1.0f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
            rotationTween.TweenProperty(this.extraDetails, "rotation", radiansRotation, 1.0f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
            rotationTween.Chain().TweenCallback(Callable.From(this.onAnimationFinished));
            
            rotationTween.Play();
        }
        else
        {
            radiansRotation = Godot.Mathf.DegToRad(this.stateNumber * 90);

            this.pipeSprite.GlobalRotation = 0;
            this.pipeSprite.Rotate(radiansRotation);
            this.rootLiquidSprites.Rotation = 0;
            this.extraDetails.Rotation = 0;
            this.rootLiquidSprites.Rotate(radiansRotation);
            this.extraDetails.Rotate(radiansRotation); 
        }
        
        foreach((var position, var outlet) in this.outletStates)
        {
            if(outlet.Opened)
            {
                Color liquidColor = GameUtils.LiquidColorsRGB[outlet.CurrentLiquid];
                for(int i = 0; i < pipeResource.baseLiquidSegmentLayout.Count; i++)
                {
                    if((pipeResource.baseLiquidSegmentLayout[i].Y + this.stateNumber) % Directions_Quantity == (int)position)
                    {
                        rootLiquidSprites.GetChild<Sprite2D>(i).SelfModulate = liquidColor;
                        break;
                    }
                }
            }
        }
    }

    protected void UpdateOutletOpeningStates()
    {
        const int Directions_Quantity = 4;
        const int infoBitStartPos = 5;

        byte baseBinaryOpeningState = this.pipeResource.binaryOpeningStates;
        byte currentBinaryOpeningState = (byte)(baseBinaryOpeningState >> this.stateNumber);

        for(int i = 0; i < Directions_Quantity; i++)
        {
            Directions outletPos = (Directions)i;
            this.outletStates[outletPos].Opened = GameUtils.GetBitFromByte(currentBinaryOpeningState, infoBitStartPos + i, true);
        }
    }

    protected void UpdateOutletConnections()
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

    public virtual void ResetOutletLiquids(LiquidType defaultLiquid = LiquidType.Vazio)
    {
        foreach(var key in this.outletStates.Keys)
        {
            this.outletStates[key].CurrentLiquid = defaultLiquid;
        }
    }

    public virtual bool IsOpened(Directions outletPos)
    {
        return this.outletStates[outletPos].Opened;
    }

    public LiquidType GetLiquid(Directions outletPos)
    {
        return this.outletStates[outletPos].CurrentLiquid;
    }
    public virtual void SetLiquid(Directions outletPos, LiquidType liquid)
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


    public virtual Godot.Collections.Dictionary<string, Variant> GetExportData()
    {
        return new Godot.Collections.Dictionary<string, Variant>
        {
            {"PipeScriptPath", GameUtils.ScriptPaths[BasePipe.ClassName]},

            { "pipeResourcePath",   this.pipeResource.ResourcePath },
            { "stateNumber",        this.stateNumber},
            { "canRotate",          this.canRotate }
        };
    }
    public virtual void ImportData(Godot.Collections.Dictionary<string, Variant> setupData)
    {
        this.pipeResource = ResourceLoader.Load<PipeResource>((string)setupData["pipeResourcePath"]);
        setupData.Remove("pipeResourcePath");

        foreach((string propertyName, var data) in setupData)
        {
            this.Set(propertyName, data);
        }
    }

    protected void onAnimationFinished()
    {
        this.isPlayingAnimation = false;
        this.UpdateDrawingState(animate:false);
    }

}

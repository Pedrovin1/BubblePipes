using Godot;
using System;
using System.Collections.Generic;

//TO FIX:
//inventory sync issues

public partial class ChangeablePipe : BasePipe
{
    private static readonly string ClassName = "ChangeablePipe";

    
    [Export]
    public string jsonPlacedPipeData;

    [Export]
    public int inventoryPipeAmount = 0;

    private BasePipe currentPipe;

    protected override void LoadExtraDetails()
    {
        this.ClearDetailSprites();

        ((Sprite2D)FindChild("BorderFrame")).Frame = 1;
        ((Sprite2D)FindChild("ContentFrame")).SelfModulate = Color.Color8(0,0,0, 0);

        jsonPlacedPipeData = jsonPlacedPipeData ?? Json.Stringify(base.GetExportData());

        var scene = ResourceLoader.Load<PackedScene>(GameUtils.PipeSlotScenePath);
        Node instance = scene.Instantiate();
        this.extraDetails.AddChild(instance);
        instance.Owner = this;

        Json json = new();
        json.Parse(jsonPlacedPipeData);

        var dataDict = new Godot.Collections.Dictionary<string, Variant>
        (
            (Godot.Collections.Dictionary)json.Data
        );

        ulong nodeID = instance.GetInstanceId();
        instance.SetScript(ResourceLoader.Load((string)dataDict["PipeScriptPath"]));
        instance = (Node)InstanceFromId(nodeID);
        instance.Owner = this;

        dataDict.Remove("PipeScriptPath");
        ((ISavable)instance).ImportData(dataDict);

        currentPipe = (BasePipe)instance; 
        currentPipe.GlobalPosition = this.GlobalPosition;
        currentPipe.MouseFilter = MouseFilterEnum.Ignore;
        currentPipe.canRotate = this.canRotate;
        currentPipe.stateNumber = 0;

        instance._Ready();

        ((Sprite2D)currentPipe.FindChild("BorderFrame")).SelfModulate = Color.Color8(0,0,0, 0);
    }

    public override Godot.Collections.Dictionary<string, Variant> GetExportData()
    {
        Godot.Collections.Dictionary<string, Variant> dataDict = new Godot.Collections.Dictionary<string, Variant>
        {
            {"PipeScriptPath", GameUtils.ScriptPaths[ChangeablePipe.ClassName]},
            
            {"jsonPlacedPipeData", this.jsonPlacedPipeData ?? Json.Stringify(base.GetExportData())},
            {"inventoryPipeAmount", this.inventoryPipeAmount}
        };

        dataDict.Merge(base.GetExportData());

        return dataDict;
    }

    public override void onClicked()
    {
        if(Inventory.indexSelectedItem < 0)
        {
            this.currentPipe.onClicked();
            return;
        }
        

        if(this.currentPipe.pipeResource == BasePipe.defaultEmptyPipeResource) //might cause problems
        {
            this.jsonPlacedPipeData = Inventory.jsonDataSelectedItem;
            GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.RemoveHeldItemFromInventory);
        }
        else
        {
            GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.AddItemToInventory, this.jsonPlacedPipeData);
            this.jsonPlacedPipeData = Inventory.jsonDataSelectedItem;
            GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.RemoveHeldItemFromInventory);
        }

        this._Ready();
    }

    public override bool IsOpened(Directions outletPos) => this.currentPipe.IsOpened(outletPos);
    public override LiquidType GetLiquid(Directions outletPos) => this.currentPipe.GetLiquid(outletPos);
    public override void SetLiquid(Directions outletPos, LiquidType liquid) => this.currentPipe.SetLiquid(outletPos, liquid);
    public override void LockRotation() {this.canRotate = false; this.currentPipe.LockRotation(); }
    public override void UnlockRotation() {this.canRotate = true; this.currentPipe.UnlockRotation();}
    public override Directions[] GetConnections(Directions outletPos) => this.currentPipe.GetConnections(outletPos);
    public override bool IsPlayingAnimation() => this.currentPipe.IsPlayingAnimation();
    public override void ResetTweens() => this.currentPipe.ResetTweens();
    public override void UpdateDrawingState(bool animate = false) => this.currentPipe.UpdateDrawingState(animate);
    public override void ResetOutletLiquids(LiquidType defaultLiquid) => this.currentPipe.ResetOutletLiquids();
}


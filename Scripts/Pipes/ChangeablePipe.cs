using Godot;
using System;
using System.Collections.Generic;

public partial class ChangeablePipe : BasePipe
{
    [Signal]
    public delegate void AddItemToInventoryEventHandler();
    [Signal]
    public delegate void HeldPipePlacedEventHandler();
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
        currentPipe.ZIndex = 10; //TESTING

        instance._Ready();
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
        //if not holding pipe
        this.currentPipe.onClicked();
        //return;

        if(this.currentPipe.pipeResource == BasePipe.defaultEmptyPipeResource)
        {
            //get info of held pipe, 
            // update placed pipe data
            //remove from inventory
        }
        else
        {
            //get info held pipe
            //sent current pipe to inventory
            //update placed pipe data
        }

    }
}


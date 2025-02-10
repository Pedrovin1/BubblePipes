using System;
using System.Collections.Generic;
using Godot;
using Godot.NativeInterop;
using static System.Environment;

public partial class PlayerData : Node, ISavable
{
    [Signal]
    public delegate void PlayerDataChangedEventHandler();
    public static PlayerData self;

    private const string saveFolderName = "BubblePipesData";
    private const string saveFileName = "bubblepipesdata.json";
    private string saveFileDirectory;
    

    public int selectedLevel {get; private set;} = 1;
    public int lastUnlockedLevel {get; private set;} = 1;


    public override void _Ready()
    {
        PlayerData.self = this;

        SignalBus.self.Connect(SignalBus.SignalName.LevelCompleted, new Callable(PlayerData.self, MethodName.onLevelCompleted));

        this.saveFileDirectory = System.Environment.GetFolderPath(SpecialFolder.ApplicationData);
        this.saveFileDirectory += $"\\{PlayerData.saveFolderName}";

        if(!Godot.DirAccess.DirExistsAbsolute(this.saveFileDirectory))
        {
            DirAccess.MakeDirAbsolute(this.saveFileDirectory);
        }

        if(!Godot.FileAccess.FileExists(this.saveFileDirectory+$"\\{PlayerData.saveFileName}"))
        {
            Godot.FileAccess.Open(this.saveFileDirectory+$"\\{PlayerData.saveFileName}", Godot.FileAccess.ModeFlags.Write).Close();
            this.ExportData();
            return;
        }

        using var saveFile = Godot.FileAccess.Open(this.saveFileDirectory+$"\\{PlayerData.saveFileName}", Godot.FileAccess.ModeFlags.Read);


        string jsonString = saveFile.GetAsText(skipCr:true).Trim().Split('\n').Join("");
        Json json = new();
        if(json.Parse(jsonString) != Error.Ok){ throw new Exception(json.GetErrorMessage()); }

        var dataDictionary = new Godot.Collections.Dictionary<string, Variant>
        (
            (Godot.Collections.Dictionary)json.Data
        );

        this.ImportData(dataDictionary);

        saveFile.Close();
    }

    private void onLevelCompleted(int levelCompleted)
    {
        if(levelCompleted == this.lastUnlockedLevel)
        {
            this.lastUnlockedLevel++;
            this.EmitSignal(SignalName.PlayerDataChanged);
        }

        this.ExportData();
    }

    private void ExportData()
    {
        //if(this.lastUnlockedLevel <= 1){ return; }

        using var saveFile = Godot.FileAccess.Open( this.saveFileDirectory + $"\\{PlayerData.saveFileName}", 
                                                    FileAccess.ModeFlags.Write);
        saveFile.StoreString(Json.Stringify(this.GetExportData()));
        saveFile.Close(); 
    }

    public Godot.Collections.Dictionary<string, Variant> GetExportData()
    {
        return new Godot.Collections.Dictionary<string, Variant>()
        {
            {PlayerData.PropertyName.selectedLevel,     this.selectedLevel},
            {PlayerData.PropertyName.lastUnlockedLevel, this.lastUnlockedLevel},
        };
    }

    public void ImportData(Godot.Collections.Dictionary<string, Variant> data)
    {
        foreach((string propertyName, var value) in data)
        {
            this.Set(propertyName, value);
        }

        this.EmitSignal(SignalName.PlayerDataChanged);
    }
}
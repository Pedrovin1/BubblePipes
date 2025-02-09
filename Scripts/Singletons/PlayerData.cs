using System;
using System.Collections.Generic;
using Godot;
using static System.Environment;

public partial class PlayerData : Node, ISavable
{
    private const string saveFolderName = "BubblePipesData";
    private const string saveFileName = "bubblepipesdata.json";
    private string saveFileDirectory;
    
    private int selectedLevel = 1;
    private int lastUnlockedLevel = 1;

    bool safeToExport = false;

    public override void _Ready()
    {
        this.saveFileDirectory = System.Environment.GetFolderPath(SpecialFolder.ApplicationData);
        this.saveFileDirectory += $"\\{PlayerData.saveFolderName}";

        if(!Godot.DirAccess.DirExistsAbsolute(this.saveFileDirectory))
        {
            DirAccess.MakeDirAbsolute(this.saveFileDirectory);
        }

        if(!Godot.FileAccess.FileExists(this.saveFileDirectory+$"\\{PlayerData.saveFileName}"))
        {
            Godot.FileAccess.Open(this.saveFileDirectory+$"\\{PlayerData.saveFileName}", Godot.FileAccess.ModeFlags.Write).Close();
            return;
        }

        using var saveFile = Godot.FileAccess.Open(this.saveFileDirectory+$"\\{PlayerData.saveFileName}", Godot.FileAccess.ModeFlags.Read);

        string jsonString = saveFile.GetAsText(true).Trim().Split('\n').Join("");
        Json json = new();
        if(json.Parse(jsonString) != Error.Ok){ throw new Exception(json.GetErrorMessage()); }

        var dataDictionary = new Godot.Collections.Dictionary<string, Variant>
        (
            (Godot.Collections.Dictionary)json.Data
        );

        this.ImportData(dataDictionary);

        saveFile.Close();
    }

    private void ExportData()
    {

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
    }
}
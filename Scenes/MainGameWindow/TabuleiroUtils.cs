using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public partial class Tabuleiro : GridContainer
{
    public const string LevelExportPath = "res://Assets/Levels";
    private const string defaultMapFileName = "Level_0.json";

    
    public bool isMoveInsideBounds(int currentIndex, Directions movement, out ISlotInteractable neighborNode)
    {
        int movementIndexOffset = 0;
        neighborNode = null;
        bool result = false;

        switch(movement)
        {
            case Directions.Cima: movementIndexOffset = -this.Columns;
                result = currentIndex + movementIndexOffset >= 0;
                break;
            
            case Directions.Direita: movementIndexOffset = 1;
                result = (currentIndex + movementIndexOffset) % this.Columns != 0 &&
                        currentIndex + movementIndexOffset < this.GetChildCount(); //unecessary but safer
                break;
            
            case Directions.Baixo: movementIndexOffset = this.Columns;
                result = currentIndex + movementIndexOffset < this.GetChildCount(); 
                break;
        
            case Directions.Esquerda: movementIndexOffset = -1;
                result = currentIndex % this.Columns != 0;
                break;

        }

        if(result)
        { 
            neighborNode = this.GetChild<ISlotInteractable>(currentIndex + movementIndexOffset); 
        }
        return result;
    }

   public void ExportLevel(string savePath = Tabuleiro.LevelExportPath)
   {
        if(DirAccess.Open(savePath) is null){ throw new DirectoryNotFoundException($"Directory {savePath} Not Found"); }

        using var file = Godot.FileAccess.Open(savePath + "/" + defaultMapFileName, Godot.FileAccess.ModeFlags.Write);

        foreach(ISavable node in this.GetChildren())
        {
            file.StoreLine(Json.Stringify(node.GetExportData()));
        }

        file.Close();
   }

   public void LoadLevel(int levelNumber = 0)
   {
        string filePath = LevelExportPath + $"/Level_{levelNumber}.json";

        if(!Godot.FileAccess.FileExists(filePath)){ throw new FileNotFoundException($"File {filePath} not found"); }

        using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);

        //int nodesAmount = file.GetAsText().Count("\n");
        // this.BalanceChildrenAmount(nodesAmount)
        this.ResetBoard();

        int nodeIndex = -1;
        while(file.GetPosition() < file.GetLength())
        {
            nodeIndex++;
            string jsonString = file.GetLine();
            Godot.Json jsonData = new();

            if(jsonData.Parse(jsonString ) != Error.Ok)
            { 
                throw new FormatException($"Json string {jsonString} \n bad format");
            }

            var dataDictionary = new Godot.Collections.Dictionary<string, Variant>
            (
                (Godot.Collections.Dictionary)jsonData.Data
            );

            Node childNode = this.GetChild(nodeIndex);

            ulong nodeID = childNode.GetInstanceId();
            childNode.SetScript(ResourceLoader.Load((string)dataDictionary["PipeScriptPath"])); //C# disposes current instance for some obscure reason
            childNode = (Node)InstanceFromId(nodeID); //to find the new instance generated after the attachScript 

            dataDictionary.Remove("PipeScriptPath");

            ((ISavable)childNode).ImportData(dataDictionary);
            childNode._Ready();
        }

        file.Close();
   }

    private void ResetBoard(int columns = 5) //has edge cases but not a problem for the current game scope
    {
        int requiredNodes = (columns * 10) - this.GetChildCount();
        var scene = ResourceLoader.Load<PackedScene>(GameUtils.PipeSlotScenePath);

        for(int i = 0; i < requiredNodes; i++)
        {
            var instance = scene.Instantiate();
            this.AddChild(instance);
            instance.Owner = this;
        }

        for(int i = 0; i < GetChildCount(); i++)
        {
            Node instance = this.GetChild(i);

            ulong nodeID = instance.GetInstanceId();
            instance.SetScript(ResourceLoader.Load("res://Scripts/Pipes/BasePipe.cs"));
            instance = (Node)InstanceFromId(nodeID);

            instance.Owner = this;
            instance._Ready();
        }
    }

   private void BalanceChildrenAmount(int targetChildrenAmount)
   {
        int childCount = this.GetChildCount();

        if(childCount > targetChildrenAmount)
        {   
            while(childCount > targetChildrenAmount)
            {
                int index = 0;
                while(this.GetChild(index).IsQueuedForDeletion()){ index++; }
                
                this.GetChild(index).QueueFree();
                childCount--;
            }
            return;
        }

        if(childCount < targetChildrenAmount)
        {
            var scene = ResourceLoader.Load<PackedScene>(GameUtils.PipeSlotScenePath);
            while(childCount < targetChildrenAmount)
            {
                var instance = scene.Instantiate();
                this.AddChild(instance);
                instance.Owner = this;

                childCount++;
            }
            return;
        }
   }
}

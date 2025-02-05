using System.IO;
using Godot;

public interface ISavable
{
    public Godot.Collections.Dictionary<string, Variant> ExportData();
    public void ImportData(Godot.Collections.Dictionary<string, Variant> setupData);
}
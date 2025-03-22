using Godot;

public interface ISavable
{
    public Godot.Collections.Dictionary<string, Variant> GetExportData();
    public void ImportData(Godot.Collections.Dictionary<string, Variant> setupData);
}
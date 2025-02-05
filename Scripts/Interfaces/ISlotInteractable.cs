public interface ISlotInteractable : ISavable
{
    public bool IsOpened(Directions outletPos);

    public LiquidType GetLiquid(Directions outletPos);
    public void SetLiquid(Directions outletPos, LiquidType liquid);

    public Directions[] GetConnections(Directions outletPos);

    public void UpdateDrawingState();
    
    public void ResetOutletLiquids(LiquidType defaultLiquid);
}
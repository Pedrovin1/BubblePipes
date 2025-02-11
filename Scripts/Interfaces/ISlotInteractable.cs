using Godot;

public interface ISlotInteractable : ISavable
{
    public bool IsOpened(Directions outletPos);

    public LiquidType GetLiquid(Directions outletPos);
    public void SetLiquid(Directions outletPos, LiquidType liquid);

    public Directions[] GetConnections(Directions outletPos);

    public bool IsPlayingAnimation();
    public void ResetTweens();
    public void UpdateDrawingState(bool animate = false);
    
    public void ResetOutletLiquids(LiquidType defaultLiquid);
}
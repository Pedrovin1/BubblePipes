using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class ContentSampleSlot : Button
{
    [Signal]
    public delegate void SampleSlotPressedEventHandler(int contentSlotIndex, Sprite2D contentTexture, string pipeName);

    [Export]
    public string DefaultPipeJson {get; private set;}

    [Export]
    public string pipeName {get; private set;} = "unnamed";

    public override void _Pressed()
    {
        this.EmitSignal(ContentSampleSlot.SignalName.SampleSlotPressed, 
                            this.GetIndex(), 
                            this.GetChild<Sprite2D>(0),
                            this.pipeName);
    }
}

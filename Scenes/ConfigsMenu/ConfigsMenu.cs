using Godot;
using System;
using System.Globalization;

public partial class ConfigsMenu : Control
{
    public static double animationSpeedMultiplier = 1;
    public static bool ColorblindMode = false;

    HSlider animationSpeedNode;
    Label animationSpeedNumber;
    Button colorblindButton;
    

    public override void _Ready()
    {
        GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.InventoryMenuToggled, new Callable(this, ConfigsMenu.MethodName.onInventoryMenuToggled));

        this.GetChild<Node2D>(1).Visible = false;
        this.animationSpeedNode = (HSlider)FindChild("AnimationSpeedHSlider");
        this.animationSpeedNumber = GetNode<Label>("./Node2D/Labels/AnimationSpeedNumber");
        this.colorblindButton = (Button)FindChild("ColorblindButton");
    }

    public void onAnimationSpeedSliderValueChanged(float value)
    {
        foreach(Node panel in  FindChild("SpeedAnimationPanels").GetChildren())
        {
            panel.GetChild<Sprite2D>(0).Frame = 0;
        }

        var selectedDotPanel = FindChild("SpeedAnimationPanels").GetChild((int)(Mathf.Round(value * 10) / 5) - 1);
        selectedDotPanel.GetChild<Sprite2D>(0).Frame = 1;
        this.animationSpeedNumber.Text = $"{((decimal)value).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}x";
    }

    public void onAnimationSpeedSliderDragEnded(bool _)
    {
        ConfigsMenu.animationSpeedMultiplier = this.animationSpeedNode.Value;
    }

    public void onColorblindButtonToggled(bool toggledOn)
    {
        ConfigsMenu.ColorblindMode = toggledOn;

        if(toggledOn){ this.colorblindButton.GetChild<Sprite2D>(0).Frame = 1;}
        else{          this.colorblindButton.GetChild<Sprite2D>(0).Frame = 0;}
    }

    public void onQuitGameButtonPressed()
    {
        GetTree().Quit();
    }

    public void onConfigurationsButtonToggled(bool toggledOn)
    {
        this.FindChild("ConfigurationsButton").GetChild<Sprite2D>(0).Frame = toggledOn ? 1 : 0;
        this.GetChild<Node2D>(1).Visible = toggledOn;
        this.GetChild<Node2D>(1).ZIndex = toggledOn ? 8 : 0;

        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.ConfigurationsMenuToggled, toggledOn);
    }

    public void onInventoryMenuToggled(bool toggledOn)
    {

        this.GetChild<CanvasItem>(0).Visible = !toggledOn;
        this.ZIndex = toggledOn ? 8 : 0;
    }

    public void onColorblindButtonMouseEntered()
    {
        ((Node2D)this.FindChild("ColorblindText")).Show();
    }

    public void onColorblindButtonMouseExited()
    {
        ((Node2D)this.FindChild("ColorblindText")).Hide();
    }

}

using Godot;
using System;

public partial class ConfigsMenu : Control
{
    public static double animationSpeedMultiplier = 2;

    HSlider animationSpeedNode;

    public override void _Ready()
    {
        this.animationSpeedNode = (HSlider)FindChild("AnimationSpeedHSlider");
    }

    public void onAnimationSpeedSliderValueChanged(float value)
    {
        foreach(Node panel in  FindChild("SpeedAnimationPanels").GetChildren())
        {
            panel.GetChild<Sprite2D>(0).Frame = 0;
        }

        var selectedDotPanel = FindChild("SpeedAnimationPanels").GetChild((int)(Mathf.Round(value * 10) / 5) - 1);
        selectedDotPanel.GetChild<Sprite2D>(0).Frame = 1;
    }

    public void onAnimationSpeedSliderDragEnded(bool _)
    {
        ConfigsMenu.animationSpeedMultiplier = this.animationSpeedNode.Value;
    }



}

using Godot;
using System;
using System.Text.RegularExpressions;

public partial class LevelDesignTool : Control
{
    Node samplesRoot;
    Node designSlotsRoot;
    Button ColorButton;

    (int index, Sprite2D sprite, string name) selectedSample;

    public override void _Ready()
    {
        this.samplesRoot = FindChild("SamplesContainer");
        this.designSlotsRoot = FindChild("DesignSlotsContainer");
        this.ColorButton = (Button)FindChild("ColorButton");


        ((Button)this.FindChild("ExportLevelButton")).Pressed += this.onExportButtonPressed;
        foreach(ContentSampleSlot sample in this.samplesRoot.GetChildren()){ sample.SampleSlotPressed += this.onSampleSelected; }
        foreach(LevelDesignSlot slot in this.designSlotsRoot.GetChildren()){ slot.LevelDesignSlotPressed += this.onDesignSlotPressed; }
    }
    
    private void onSampleSelected(int sampleIndex, Sprite2D sprite, string name)
    {
        this.selectedSample = (sampleIndex, sprite, name);
    }

    private void onDesignSlotPressed(LevelDesignSlot slot)
    {
        if(ColorButton.ButtonPressed)
        {
            switch(this.samplesRoot.GetChild<ContentSampleSlot>(slot.contentSampleIndex).pipeName)
            {
                case "gate": 
                    slot.color = (LiquidType)(((int)slot.color + 1) % 5); //requires offset when exporting
                    slot.SetColor(GameUtils.LiquidColorsRGB[slot.color]);
                    return;

                case "objective": 
                    slot.color = (LiquidType)(((int)slot.color + 1) % 5); //requires offset when exporting
                    slot.GetChild<Sprite2D>(0).Frame = (int)slot.color; 
                    return;

                case "source": 
                    slot.color = (LiquidType)(((int)slot.color + 1) % 5); //requires offset when exporting
                    slot.GetChild<Sprite2D>(0).Frame = (int)slot.color; 
                    return;

                case "color_changer":
                    if(Input.IsKeyPressed(Key.Shift))
                    {
                        slot.color2 = (LiquidType)(((int)slot.color2 + 1) % 5);
                        slot.SetColor(GameUtils.LiquidColorsRGB[slot.color2], 1);
                    }
                    else
                    {
                        slot.color = (LiquidType)(((int)slot.color + 1) % 5);
                        slot.SetColor(GameUtils.LiquidColorsRGB[slot.color], 0);
                    }
                    return;
            }

            return;
        }


        slot.contentSampleIndex = selectedSample.index;

        slot.stateNumber = 0;
        slot.color = LiquidType.Vazio;
        slot.color2 = LiquidType.Vazio;
        slot.ResetRotation();
        slot.ResetColor();
        slot.ResetExtraSprites();

        slot.SetSprite(selectedSample.sprite);
        switch(selectedSample.name)
        {
            case "gate": 
                slot.AddDetailSprite
                (
                    new Sprite2D()
                    {
                        Texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/line.png"),
                        Hframes = 1,
                        Frame = 0
                    }
                );
            break;
            case "color_changer": 
                slot.AddDetailSprite
                (
                    new Sprite2D()
                    {
                        Texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/ColorChangerPipe.png"),
                        Hframes = 3,
                        Frame = 1
                    }
                );
                slot.AddDetailSprite
                (
                    new Sprite2D()
                    {
                        Texture = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/ColorChangerPipe.png"),
                        Hframes = 3,
                        Frame = 2
                    }
                );
            break;
        }
    }




    private void onExportButtonPressed()
    {
        using var file = Godot.FileAccess.Open("res://Assets/Levels/Level_DRAFTNUMBERHERE.json", FileAccess.ModeFlags.Write);

        foreach(LevelDesignSlot slot in this.designSlotsRoot.GetChildren())
        {
            file.StoreLine(this.samplesRoot.GetChild<ContentSampleSlot>(slot.contentSampleIndex).DefaultPipeJson);
        }

        GD.Print(file.GetLength().ToString());

        file.Close();
    }
}

using Godot;
using System.Collections.Generic;

public partial class LevelDesignTool : Control
{
    Node samplesRoot;
    Node designSlotsRoot;
    Button ColorButton;
    Button BubbleButton;
    List<int> indexSlotsWithBubble = new();

    (int index, Sprite2D sprite, string name) selectedSample;

    public override void _Ready()
    {
        this.samplesRoot = FindChild("SamplesContainer");
        this.designSlotsRoot = FindChild("DesignSlotsContainer");
        this.ColorButton = (Button)FindChild("ColorButton");
        this.BubbleButton = (Button)FindChild("BubbleButton");


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
                    slot.color = (LiquidType)(((int)slot.color + 1) % 5 + 1); //requires offset when exporting
                    slot.SetColor(GameUtils.LiquidColorsRGB[slot.color]);
                    return;

                case "objective": 
                    slot.color = (LiquidType)(((int)slot.color + 1) % 5 + 1); //requires offset when exporting
                    slot.GetChild<Sprite2D>(0).Frame = (int)slot.color; 
                    return;

                case "source": 
                    slot.color = (LiquidType)(((int)slot.color + 1) % 5 + 1); //requires offset when exporting
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

        if(this.BubbleButton.ButtonPressed)
        {
            if(!this.indexSlotsWithBubble.Contains(slot.GetIndex()))
            {
                this.indexSlotsWithBubble.Add(slot.GetIndex());

                slot.FindChild("Bubble").AddChild
                (
                    new Sprite2D()
                    {
                        Texture = this.BubbleButton.GetChild<Sprite2D>(0).Texture,
                        Hframes = 5,
                        Frame = 0,
                        Centered = false,
                        GlobalPosition = slot.GlobalPosition,
                        Offset = new Vector2(1, 1)
                    }
                );
                return;
            }

            if(Input.IsKeyPressed(Key.Shift))
            {
                slot.FindChild("Bubble").GetChild<Sprite2D>(0).QueueFree();
                this.indexSlotsWithBubble.Remove(slot.GetIndex());
                return;
            }

            slot.FindChild("Bubble").GetChild<Sprite2D>(0).Frame++;
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
            case "empty": slot.HideSprite(); break;
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
        int levelsAmount = Godot.DirAccess.GetFilesAt("res://Assets/Levels/").Length;
        using var file = Godot.FileAccess.Open($"res://Assets/Levels/Level_{levelsAmount + 1}.json", FileAccess.ModeFlags.Write);

        foreach(LevelDesignSlot slot in this.designSlotsRoot.GetChildren())
        {
            var sample = this.samplesRoot.GetChild<ContentSampleSlot>(slot.contentSampleIndex);
            if( sample.pipeName == "source")
            {
                string temp = sample.DefaultPipeJson.ReplaceN("\"sourceLiquid\":1", $"\"sourceLiquid\":{(int)slot.color+1}");
                file.StoreLine(temp);
                continue;
            }
            if(sample.pipeName == "objective")
            {
                string temp = sample.DefaultPipeJson.ReplaceN("\"requiredLiquid\":1", $"\"requiredLiquid\":{(int)slot.color+1}");
                file.StoreLine(temp);
                continue;
            }

            file.StoreLine(this.samplesRoot.GetChild<ContentSampleSlot>(slot.contentSampleIndex).DefaultPipeJson);
        }

        GD.Print(file.GetLength().ToString());

        file.Close();
    }
}

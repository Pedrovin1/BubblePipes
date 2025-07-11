using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Control
{

    private struct Buttons
    {
        public Buttons(Node _Left, Node _Right){Left = (Button)_Left; Right = (Button)_Right; }
        public Button Left;
        public Button Right;
    }

    bool opened = false;
    int pagesAmount = 0;
    int currentPage = 0;


    public static int indexSelectedItem = -1;
    public static string jsonDataSelectedItem = "";
    List<string> pipesJsonData = new();

    Buttons buttons;
    Button inventoryButton;
    Sprite2D inventoryBagSprite;
    AnimationPlayer animationNode;
    Node inventorySlotsRoot;

    private bool visibilyHiddenLocked = true;



    public override void _Ready()
    {
        this.animationNode = (AnimationPlayer)FindChild("AnimationPlayer");

        this.buttons = new(FindChild("ArrowLeft"), FindChild("ArrowRight"));
        this.buttons.Left.Connect(Button.SignalName.Pressed, Callable.From(this.onLeftButtonClicked));
        this.buttons.Right.Connect(Button.SignalName.Pressed, Callable.From(this.onRightButtonClicked));

        this.inventoryBagSprite = (Sprite2D)FindChild("BagSprite");
        this.inventoryBagSprite.Frame = 0;

        this.inventoryButton = (Button)FindChild("InventoryButton");
        this.inventoryButton.Connect(Button.SignalName.Pressed, Callable.From(this.onInventoryButtonPressed));

        this.inventorySlotsRoot = FindChild("ItemSlotsContainer");
        foreach(InventorySlot slot in this.inventorySlotsRoot.GetChildren())
        {
            slot.Connect(InventorySlot.SignalName.ItemSlotClicked, new Callable(this, Inventory.MethodName.onInventorySlotClicked));
        }

        GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.RemoveHeldItemFromInventory, Callable.From(this.onRemoveHeldItemFromInventory));
        GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.AddItemToInventory, new Callable(this, Inventory.MethodName.onAddItemToInventory));
        GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.ConfigurationsMenuToggled, new Callable(this, Inventory.MethodName.onConfigurationsMenuToggled));
        GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.LevelLoaded, new Callable(this, Inventory.MethodName.onLevelLoaded));
        this.updateSlotSprites();
    }

    public void onLevelLoaded()
    {
        if(this.pipesJsonData is null || this.pipesJsonData.Count <= 0)
        {
            this.visibilyHiddenLocked = true;
            this.Visible = false;
            this.inventoryBagSprite.Frame = 0;
            this.opened = false;
            this.animationNode.PlayBackwards("OpenInventory");
            GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.InventoryMenuToggled, this.opened);
        }
        else
        {
            this.visibilyHiddenLocked = false;
            this.Visible = true;
        }

    }

    public void onConfigurationsMenuToggled(bool toggledOn)
    {
        if(this.visibilyHiddenLocked){ return; }

        this.Visible = !toggledOn;
    }

    private void onInventoryButtonPressed()
    {
        if(this.pipesJsonData.Count <= 0 && !this.opened){ return; } //maybe add an alert saying that there are no pipes in inventory or somehitng

        this.opened = !this.opened;
        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.InventoryMenuToggled, this.opened);
        
        this.inventoryBagSprite.Frame = this.opened ? 1 : 0;

        if(this.opened){ this.animationNode.Play("OpenInventory"); }
        else{ this.animationNode.PlayBackwards("OpenInventory"); }
    }

    private void onInventorySlotClicked(int slotIndex, Sprite2D itemSpriteNode)
    {
        foreach(InventorySlot slot in this.inventorySlotsRoot.GetChildren()){slot.HideSelectionMarker();}

        int index = slotIndex + this.currentPage * 3;
        if(index > this.pipesJsonData.Count - 1){ Inventory.indexSelectedItem = -1; return; }

        string jsonItem = this.pipesJsonData[index];

        Inventory.indexSelectedItem = index;
        Inventory.jsonDataSelectedItem = jsonItem;

        this.inventorySlotsRoot.GetChild<InventorySlot>(slotIndex).ShowSelectedMarker();

        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.ItemSelected, jsonItem, itemSpriteNode);
    }

    private void onAddItemToInventory(string itemJsonData)
    {
        this.pipesJsonData.Add(itemJsonData);
        this.pagesAmount = (int)Mathf.Ceil(this.pipesJsonData.Count / 3f); 
        this.updateSlotSprites();
    }

    private void onRemoveHeldItemFromInventory()
    {
        foreach(InventorySlot slot in this.inventorySlotsRoot.GetChildren()){slot.HideSelectionMarker();}
        this.pipesJsonData.RemoveAt(Inventory.indexSelectedItem);
        this.pagesAmount = (int)Mathf.Ceil(this.pipesJsonData.Count / 3f); 
        Inventory.indexSelectedItem = -1;
        this.updateSlotSprites();
    }

    public void WipeInventoryItems()
    {
        this.pipesJsonData = new();
        Inventory.indexSelectedItem = -1;
        this.updateSlotSprites();
    }

    private void onLeftButtonClicked()
    {

        this.currentPage = Math.Max(this.currentPage - 1, 0);
        this.updateSlotSprites();
    }

    private void onRightButtonClicked()
    {
        this.currentPage = Math.Min(this.currentPage + 1, Math.Max(this.pagesAmount - 1, 0));
        this.updateSlotSprites();
    }

    private void updateSlotSprites()
    {
        foreach(InventorySlot slot in this.inventorySlotsRoot.GetChildren()){slot.HideSelectionMarker();}

        for(int i = 0; i < 3; i++)
        {
            int index = i + this.currentPage * 3;

            if(index > this.pipesJsonData.Count - 1)
            {
                inventorySlotsRoot.GetChild<InventorySlot>(i).SetSprite(null);
                continue;
            }

            if(index == Inventory.indexSelectedItem){ this.inventorySlotsRoot.GetChild<InventorySlot>(i).ShowSelectedMarker();}

            string pipeData = pipesJsonData[index];

            Json json = new();
            json.Parse(pipeData);
            var dataDict = new Godot.Collections.Dictionary<string, Variant>
            ((Godot.Collections.Dictionary)json.Data);
            PipeResource pipeResource = ResourceLoader.Load<PipeResource>((string)dataDict["pipeResourcePath"]);

            var mirrorSpriteNode = new Sprite2D
            {
                Texture = pipeResource.pipeSpriteFile,
                Hframes = pipeResource.pipeSpriteHframes,
                Frame = pipeResource.pipeSpriteFrame
            };

            //List<Sprite2D> detailSprites = new();
            //

            inventorySlotsRoot.GetChild<InventorySlot>(i).SetSprite(mirrorSpriteNode);
        }
    }
}

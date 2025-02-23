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

    List<string> pipesJsonData;


    Buttons buttons;
    Button inventoryButton;
    Sprite2D inventoryBagSprite;
    AnimationPlayer animationNode;
    Node inventorySlotsRoot;



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
    }

    private void onInventoryButtonPressed()
    {
        this.opened = !this.opened;
        this.inventoryBagSprite.Frame = this.opened ? 1 : 0;

        if(this.opened){ this.animationNode.Play("OpenInventory"); }
        else{ this.animationNode.PlayBackwards("OpenInventory"); }
    }

    private void onInventorySlotClicked(int slotIndex, Texture2D itemTexture)
    {
        string jsonItem = ""; //TO IMPLEMENT list index conversion
        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.ItemSelected, jsonItem, itemTexture);
    }

    private void onAddItemToInventory(string itemJsonData)
    {
        this.pipesJsonData.Add(itemJsonData);
        this.pagesAmount = (int)Mathf.Ceil(this.pipesJsonData.Count / 3f); 
        this.updateSlotSprites();
    }

    private void onRemoveItemToInventory()
    {
        //to implement
        this.pagesAmount = (int)Mathf.Ceil(this.pipesJsonData.Count / 3f); 
        this.updateSlotSprites();
    }

    private void onLeftButtonClicked()
    {
        this.currentPage = Math.Max(this.currentPage - 1, 0);
        this.updateSlotSprites();
    }

    private void onRightButtonClicked()
    {
        this.currentPage = Math.Min(this.currentPage + 1, this.pagesAmount);
        this.updateSlotSprites();
    }

    private void updateSlotSprites()
    {

    }



}

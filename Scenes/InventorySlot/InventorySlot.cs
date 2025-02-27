using Godot;
using System;

public partial class InventorySlot : CenterContainer
{
    [Signal]
    public delegate void ItemSlotClickedEventHandler(int slotPosition, Node2D itemSpriteNode);

    private Sprite2D spriteNode;
    private Sprite2D selectedMarker;
    private AnimationPlayer animationPlayer;
    private Node detailSpritesRoot;

    public override void _Ready()
    {
        this.detailSpritesRoot = FindChild("DetailSprites");
        this.selectedMarker = (Sprite2D)FindChild("SelectedMarker");
        this.animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer");
        this.spriteNode = (Sprite2D)FindChild("ItemSprite");

        Button button = (Button)FindChild("ButtonSlot");
        button.Connect(Button.SignalName.Pressed, Callable.From(this.onClicked));
        button.Connect(Button.SignalName.MouseEntered, Callable.From(this.onMouseEntered));
    }

    public void onClicked()
    {
        this.selectedMarker.Hide();
        this.EmitSignal(InventorySlot.SignalName.ItemSlotClicked, this.GetIndex(), spriteNode);
    }

    public void HideSelectionMarker() => this.selectedMarker.Hide();
    public void ShowSelectedMarker() => this.selectedMarker.Show();


    public void onMouseEntered()
    {
        this.animationPlayer.Play("SmoothMovement");
    }

    public void SetSprite(Sprite2D spriteMirror, Sprite2D[] detailSprites = null)
    {
        if(spriteMirror == null)
        {
            this.spriteNode.Hide();
            this.selectedMarker.Hide();
            return;
        }

        spriteNode.Show();
        this.spriteNode.Texture = spriteMirror.Texture;
        this.spriteNode.Hframes = spriteMirror.Hframes;
        this.spriteNode.Frame = spriteMirror.Frame;

        foreach(var node in this.detailSpritesRoot.GetChildren())
        {
            if(!node.IsQueuedForDeletion())
            {
               node.QueueFree(); 
            }
        }
        
        if(detailSprites != null)
        {
            foreach(Sprite2D detail in detailSprites)
            {
                this.detailSpritesRoot.AddChild(detail);
                detail.Owner = this;
            }
        }
    }   
}

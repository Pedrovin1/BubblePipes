using Godot;
using System;

public partial class InventorySlot : CenterContainer
{
    [Signal]
    public delegate void ItemSlotClickedEventHandler(int slotPosition, Node2D itemSpriteNode);

    private Sprite2D spriteNode;

    public override void _Ready()
    {
        this.spriteNode = (Sprite2D)FindChild("ItemSprite");
        ((Button)FindChild("ButtonSlot")).Connect(Button.SignalName.Pressed, Callable.From(this.onClicked));
    }

    public void onClicked()
    {
        this.EmitSignal(InventorySlot.SignalName.ItemSlotClicked, this.GetIndex(), spriteNode);
    }

    public void SetSprite(Sprite2D spriteMirror)
    {
        if(spriteMirror == null)
        {
            spriteNode.Hide();
            return;
        }

        spriteNode.Show();
        this.spriteNode.Texture = spriteMirror.Texture;
        this.spriteNode.Hframes = spriteMirror.Hframes;
        this.spriteNode.Frame = spriteMirror.Frame;
    }   
}

using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;

namespace NeatMediumcore.UI;

internal class UIHoverImageButton(Asset<Texture2D> texture, string hoverText, string nPath) : UIImageButton(texture)
{
    private string HoverText = hoverText;
    private bool mouseover = false;
    private bool mousedown = false;
    private bool canPickUpAnotherPlayersItems = false;

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var texture = ModContent.Request<Texture2D>(nPath + "Single");
        var textureMO = ModContent.Request<Texture2D>(nPath + "SingleMouseOver");
        var textureA = ModContent.Request<Texture2D>(nPath + "All");
        var textureMOA = ModContent.Request<Texture2D>(nPath + "AllMouseOver");

        base.DrawSelf(spriteBatch);

        SetVisibility(1, 1);

        canPickUpAnotherPlayersItems = Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems;
            
        HoverText = canPickUpAnotherPlayersItems ? "Can Pick Up Other Players Items" : "Can't Pick Up Other Players Items";


        if (mouseover)
        {
            Main.hoverItemName = HoverText;
            Main.LocalPlayer.mouseInterface = true;

            SetImage(canPickUpAnotherPlayersItems ? textureMOA : textureMO);
        }
        else
        {
            SetImage(canPickUpAnotherPlayersItems ? textureA : texture);
        }
    }
    public override void MouseOver(UIMouseEvent evt) => mouseover = true;

    public override void MouseOut(UIMouseEvent evt) => mouseover = false;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        mousedown = true;
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.MiddleMouseUp(evt);
        mousedown = false;
    }
}
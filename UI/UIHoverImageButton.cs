using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using System.IO;

namespace NeatMediumcore.UI
{
    internal class UIHoverImageButton : UIImageButton
    {
        internal string HoverText;
        internal bool mouseover = false;
        internal bool mousedown = false;
        internal string path;
        bool canPickUpAnotherPlayersItems = false;

        public UIHoverImageButton(Asset<Texture2D> texture, string hoverText, string nPath) : base(texture)
        {
            HoverText = hoverText;
            path = nPath;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var texture = ModContent.Request<Texture2D>(path + "Single");
            var textureMO = ModContent.Request<Texture2D>(path + "SingleMouseOver");
            var textureA = ModContent.Request<Texture2D>(path + "All");
            var textureMOA = ModContent.Request<Texture2D>(path + "AllMouseOver");

            base.DrawSelf(spriteBatch);

            SetVisibility(1, 1);

            canPickUpAnotherPlayersItems = Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems;
            
            if (canPickUpAnotherPlayersItems)
            {
                HoverText = "Can Pick Up Other Players Items";
            }
            else
            {
                HoverText = "Can't Pick Up Other Players Items";
            }


            if (mouseover == true)
            {
                Main.hoverItemName = HoverText;
                Main.LocalPlayer.mouseInterface = true;

                if (canPickUpAnotherPlayersItems)
                {
                    SetImage(textureMOA);
                }
                else
                {
                    SetImage(textureMO);
                }
            }
            else
            {
                if (canPickUpAnotherPlayersItems)
                {
                    SetImage(textureA);
                }
                else
                {
                    SetImage(texture);
                }
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
}
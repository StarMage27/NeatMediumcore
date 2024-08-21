using Microsoft.Xna.Framework.Graphics;
using NeatMediumcore.Config;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NeatMediumcore.UI
{
    class NMUI : UIState
    {
        const string texturePath = "NeatMediumcore/UI/";

        public override void OnInitialize()
        {
            string buttonNMName = "Pick Up Other Players Items";
            string pathNM = texturePath + buttonNMName.Replace(" ", "") + "Button";
            var textureNM = ModContent.Request<Texture2D>(pathNM);
            UIHoverImageButton buttonNM = new(textureNM, buttonNMName, pathNM);
            buttonNM.Width.Set(36, 0);
            buttonNM.Height.Set(36, 0);
            buttonNM.Top.Set(267 + 42, 0);
            buttonNM.Left.Set(452, 0);
            buttonNM.OnLeftClick += OnButtonNMClick;
            Append(buttonNM);
        }

        private void OnButtonNMClick(UIMouseEvent evt, UIElement listeningElement)
        {
            Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems = !Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems;
        }

        public override void OnDeactivate() => base.OnDeactivate();

        internal void Unload() { }
    }
}
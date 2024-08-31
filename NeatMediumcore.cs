using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.DataStructures;
using System.IO;
using Terraria.ModLoader.IO;
using ReLogic.Utilities;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace NeatMediumcore
{
	public class NeatMediumcore : Mod
	{
        public static ModKeybind NMBind { get; set; }
        public override void Load()
        {
            NMBind = KeybindLoader.RegisterKeybind(this, "Pick up other players items toggle", "O");
        }

        public override void Unload()
        {
            NMBind = null;
        }

		public enum InventoryType
        {
            Inventory,
            Armor,
            MiscEquips,
            Dye,
            MiscDyes,
            None,
            WingSlot, ShoeSlot, MoreAccessories, PotionSlots
            // inventory, armor, miscEquips, dye, miscDyes, trashItem, default
        }

		public static int CountDeaths(Player player)
		{
			return player.numberOfDeathsPVE + player.numberOfDeathsPVP;
		}


		// Override this method to handle network packets sent for this mod.
		//TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte playerNumber = reader.ReadByte();
			NMPlayer nMPlayer = Main.player[playerNumber].GetModPlayer<NMPlayer>();
			nMPlayer.ReceivePlayerSync(reader);

			if (Main.netMode == NetmodeID.Server) {
				// Forward the changes to the other clients
				nMPlayer.SyncPlayer(-1, whoAmI, false);
			}
		}
	}
}

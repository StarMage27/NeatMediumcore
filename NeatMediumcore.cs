using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.IO;

namespace NeatMediumcore;

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


	public static uint CountDeaths(Player player)
	{
		return (uint)player.numberOfDeathsPVE + (uint)player.numberOfDeathsPVP;
	}


	// Override this method to handle network packets sent for this mod.
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
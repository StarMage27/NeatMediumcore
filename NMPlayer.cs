using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Audio;
using static NeatMediumcore.NeatMediumcore;
using System.IO;

namespace NeatMediumcore
{
    public class NMPlayer : ModPlayer
    {
        public bool canPickUpAnotherPlayersItems = false;
        public int playerID = -1;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (NMBind.JustPressed && Main.LocalPlayer.difficulty.Equals(PlayerDifficultyID.MediumCore))
            {
                Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems = !Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems;
            }
        }

        #region Logic

        public override bool OnPickup(Item item)
        {
            if (!Player.difficulty.Equals(PlayerDifficultyID.MediumCore))
            {
                return base.OnPickup(item);
            }

            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();

            if(nMItem.nMOwnerID != playerID || nMItem.nMLatestDeathCount == -1 || nMItem.nMSlotID == -1 || nMItem.nMInventoryType == InventoryType.None)
            {
                nMItem.nMOwnerID = playerID;
                return base.OnPickup(item);
            }

            item.newAndShiny = false;

            nMItem.nMOwnerID = playerID;
            InventoryType inventoryType = nMItem.nMInventoryType;
            item.favorited = nMItem.nMFavourited;

            switch(inventoryType)
            {
                case InventoryType.Armor:
                {
                    if (nMItem.nMLoadoutID == Player.CurrentLoadoutIndex)
                    {
                        return nMOnPickup(ref Player.armor, ref item);
                    }
                    else
                    {
                        return nMOnPickup(ref Player.Loadouts[nMItem.nMLoadoutID].Armor, ref item);
                    }
                }
                case InventoryType.Dye:
                {
                    if (nMItem.nMLoadoutID == Player.CurrentLoadoutIndex)
                    {
                        return nMOnPickup(ref Player.dye, ref item);
                    }
                    else
                    {
                        return nMOnPickup(ref Player.Loadouts[nMItem.nMLoadoutID].Dye, ref item);
                    }
                }
                case InventoryType.MiscEquips:
                {
                    return nMOnPickup(ref Player.miscEquips, ref item);
                }
                case InventoryType.MiscDyes:
                {
                    return nMOnPickup(ref Player.miscDyes, ref item);
                }
                case InventoryType.Inventory:
                {
                    return nMOnPickup(ref Player.inventory, ref item);
                }
                default:
                {
                    return base.OnPickup(item);
                }
            }
        }

        private bool nMOnPickup(ref Item[] inventory, ref Item item)
        {
            if (!Main.LocalPlayer.difficulty.Equals(PlayerDifficultyID.MediumCore))
            {
                return base.OnPickup(item);
            }

            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            int slotID = nMItem.nMSlotID;
            Item inventoryItem = inventory[slotID];

            if(nMItem.nMOwnerID != playerID)
            {
                return base.OnPickup(item);
            }

            if(inventoryItem == null || inventoryItem.IsAir)
            {
                inventory[slotID] = item;
                if (item.IsACoin)
                {
                    SoundEngine.PlaySound(SoundID.CoinPickup, Player.position);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Grab, Player.position);
                }
                PopupText.NewText(PopupTextContext.RegularItemPickup, item, item.stack);
                return false;
            }
            
            NMGlobalItem inventoryNMItem = inventoryItem.GetGlobalItem<NMGlobalItem>();

            if
            (
                item.favorited
                || (nMItem.nMLatestDeathCount < inventoryNMItem.nMLatestDeathCount && !inventoryItem.favorited)
                || (inventoryNMItem.nMLatestDeathCount == -1 && !inventoryItem.favorited)
            )
            {
                inventory[slotID].favorited = false;

                inventoryNMItem.nMOwnerID = -1;
                inventoryNMItem.nMLatestDeathCount = -1;
                inventoryNMItem.nMSlotID = 0;
                inventoryNMItem.nMInventoryType = InventoryType.None;
                inventoryNMItem.nMFavourited = false;

                Player.QuickSpawnItem(Player.GetSource_FromThis(), inventoryItem, inventoryItem.stack);
                inventory[slotID].TurnToAir();

                item.favorited = nMItem.nMFavourited;
                inventory[slotID] = item;
                
                if (item.IsACoin)
                {
                    SoundEngine.PlaySound(SoundID.CoinPickup, Player.position);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Grab, Player.position);
                }
                PopupText.NewText(PopupTextContext.RegularItemPickup, item, item.stack);
                return false;
            }
            else
            {
                return base.OnPickup(item);
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (!Main.LocalPlayer.difficulty.Equals(PlayerDifficultyID.MediumCore))
            {
                return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
            }

            UpdateSelectInventory(ref Player.armor, InventoryType.Armor);
            UpdateSelectInventory(ref Player.dye, InventoryType.Dye);

            for (int i = 0; i < Player.Loadouts.Length; i++)
            {
                if (i == Player.CurrentLoadoutIndex)
                {
                    UpdateSelectInventory(ref Player.armor, InventoryType.Armor, i);
                    UpdateSelectInventory(ref Player.dye, InventoryType.Dye, i);
                }
                else
                {
                    UpdateSelectInventory(ref Player.Loadouts[i].Armor, InventoryType.Armor, i);
                    UpdateSelectInventory(ref Player.Loadouts[i].Dye, InventoryType.Dye, i);
                }
            }

            UpdateSelectInventory(ref Player.inventory, InventoryType.Inventory);
            UpdateSelectInventory(ref Player.miscEquips, InventoryType.MiscEquips);
            UpdateSelectInventory(ref Player.miscDyes, InventoryType.MiscDyes);
            
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        }

        private void UpdateSelectInventory(ref Item[] inventory, InventoryType inventoryType, int loadoutID = -1)
        {
            for (int slotID = 0; slotID < inventory.Length; slotID++)
            {
                Item item = inventory[slotID];
                
                if (item != null && !item.IsAir)
                {
                    NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();

                    if (nMItem.nMLatestDeathCount == -1 || (nMItem.nMInventoryType != inventoryType))
                    {
                        nMItem.nMLatestDeathCount = CountDeaths(Player);
                    }

                    if (inventoryType == InventoryType.Inventory && item == Main.LocalPlayer.inventory[58]) // for the item in the mouse slot
                    {
                        nMItem.nMSlotID = -1;
                        nMItem.nMFavourited = false;
                    }
                    else
                    {
                        nMItem.nMSlotID = slotID;
                        nMItem.nMFavourited = item.favorited;
                    }

                    nMItem.nMInventoryType = inventoryType;
                    nMItem.nMOwnerID = playerID;
                    nMItem.nMLoadoutID = loadoutID;
                }
            }
        }

        #endregion

        public override void OnEnterWorld()
        {
            if (playerID == -1)
            {
                playerID = Player.GetHashCode();
            }
            base.OnEnterWorld();
        }

        #region Save Data

        public override void SaveData(TagCompound tag)
        {
            tag.Add("NMCcanPickUpAnotherPlayersItems", canPickUpAnotherPlayersItems);
            tag.Add("NMPlayerID", playerID);
            base.SaveData(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("NMCcanPickUpAnotherPlayersItems"))
            {
                canPickUpAnotherPlayersItems = tag.GetBool("NMCcanPickUpAnotherPlayersItems");
            }
            if (tag.ContainsKey("NMPlayerID"))
            {
                playerID = tag.GetInt("NMPlayerID");
            }
            base.LoadData(tag);
        }

        #endregion

        #region Network Sync

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)(canPickUpAnotherPlayersItems ? 1 : 0));
            packet.Write(playerID);
			packet.Send(toWho, fromWho);
		}

		public void ReceivePlayerSync(BinaryReader reader) {
			canPickUpAnotherPlayersItems = reader.ReadBoolean();
            playerID = reader.ReadInt32();
		}

		public override void CopyClientState(ModPlayer targetCopy) {
			NMPlayer clone = (NMPlayer)targetCopy;
			clone.canPickUpAnotherPlayersItems = canPickUpAnotherPlayersItems;
            clone.playerID = playerID;
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			NMPlayer clone = (NMPlayer)clientPlayer;

			if (canPickUpAnotherPlayersItems != clone.canPickUpAnotherPlayersItems || playerID != clone.playerID)
            {
				SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
            }
		}

        #endregion
    }
}
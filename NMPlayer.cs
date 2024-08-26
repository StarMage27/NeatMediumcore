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
        public bool canPickUpAnotherPlayersItems;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Main.LocalPlayer.difficulty.Equals(PlayerDifficultyID.MediumCore))
            {
                if (NMBind.JustPressed)
                {
                    Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems = !Main.LocalPlayer.GetModPlayer<NMPlayer>().canPickUpAnotherPlayersItems;
                }
            }
        }

        #region Logic

        private void UpdateSelectInventory(ref Item[] inventory, InventoryType inventoryType)
        {
            for (int slotID = 0; slotID < inventory.Length; slotID++)
            {
                Item item = inventory[slotID];
                
                if (item != null && !item.IsAir)
                {
                    NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();

                    if (nMItem.latestDeathCount == -1 || (nMItem.inventoryType != inventoryType))
                    {
                        nMItem.latestDeathCount = CountDeaths(Player);
                    }

                    if (inventoryType == InventoryType.Inventory && item == Main.LocalPlayer.inventory[58]) // for the item in the mouse slot
                    {
                        nMItem.slotID = -1;
                        nMItem.nMFavourited = false;
                    }
                    else
                    {
                        nMItem.slotID = slotID;
                        nMItem.nMFavourited = item.favorited;
                    }

                    nMItem.inventoryType = inventoryType;
                    nMItem.ownerID = Player.whoAmI;
                }
            }
        }

        public override bool OnPickup(Item item)
        {
            if (!Player.difficulty.Equals(PlayerDifficultyID.MediumCore))
            {
                return base.OnPickup(item);
            }

            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            if(nMItem.ownerID != Player.whoAmI || nMItem.latestDeathCount == -1 || nMItem.slotID == -1 || nMItem.inventoryType == InventoryType.None)
            {
                nMItem.ownerID = Player.whoAmI;
                return base.OnPickup(item);
            }

            item.newAndShiny = false;

            nMItem.ownerID = Player.whoAmI;
            InventoryType inventoryType = nMItem.inventoryType;
            item.favorited = nMItem.nMFavourited;

            switch(inventoryType)
            {
                case InventoryType.Inventory:
                {
                    return nMOnPickup(ref Player.inventory, ref item);
                }
                case InventoryType.Armor:
                {
                    return nMOnPickup(ref Player.armor, ref item);
                }
                case InventoryType.MiscEquips:
                {
                    return nMOnPickup(ref Player.miscEquips, ref item);
                }
                case InventoryType.Dye:
                {
                    return nMOnPickup(ref Player.dye, ref item);
                }
                case InventoryType.MiscDyes:
                {
                    return nMOnPickup(ref Player.miscDyes, ref item);
                }
                case InventoryType.Loadout0Armor:
                {
                    return nMOnPickup(ref Player.Loadouts[0].Armor, ref item);
                }
                case InventoryType.Loadout0Dye:
                {
                    return nMOnPickup(ref Player.Loadouts[0].Armor, ref item);
                }
                case InventoryType.Loadout1Armor:
                {
                    return nMOnPickup(ref Player.Loadouts[1].Armor, ref item);
                }
                case InventoryType.Loadout1Dye:
                {
                    return nMOnPickup(ref Player.Loadouts[1].Dye, ref item);
                }
                case InventoryType.Loadout2Armor:
                {
                    return nMOnPickup(ref Player.Loadouts[2].Armor, ref item);
                }
                case InventoryType.Loadout2Dye:
                {
                    return nMOnPickup(ref Player.Loadouts[2].Dye, ref item);
                }
                default:
                {
                    return base.OnPickup(item);
                }
            }
        }

        private bool nMOnPickup(ref Item[] inventory, ref Item item)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            int slotID = nMItem.slotID;
            Item inventoryItem = inventory[slotID];

            if(nMItem.ownerID != Main.LocalPlayer.whoAmI)
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
                || (nMItem.latestDeathCount < inventoryNMItem.latestDeathCount && !inventoryItem.favorited)
                || (inventoryNMItem.latestDeathCount == -1 && !inventoryItem.favorited)
            )
            {
                inventory[slotID].favorited = false;

                inventoryNMItem.ownerID = -1;
                inventoryNMItem.latestDeathCount = -1;
                inventoryNMItem.slotID = 0;
                inventoryNMItem.inventoryType = InventoryType.None;
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

            UpdateSelectInventory(ref Player.inventory, InventoryType.Inventory);
            UpdateSelectInventory(ref Player.armor, InventoryType.Armor);
            UpdateSelectInventory(ref Player.miscEquips, InventoryType.MiscEquips);
            UpdateSelectInventory(ref Player.dye, InventoryType.Dye);
            UpdateSelectInventory(ref Player.miscDyes, InventoryType.MiscDyes);
            UpdateSelectInventory(ref Player.Loadouts[0].Armor, InventoryType.Loadout0Armor);
            UpdateSelectInventory(ref Player.Loadouts[0].Dye, InventoryType.Loadout0Dye);
            UpdateSelectInventory(ref Player.Loadouts[1].Armor, InventoryType.Loadout1Armor);
            UpdateSelectInventory(ref Player.Loadouts[1].Dye, InventoryType.Loadout1Dye);
            UpdateSelectInventory(ref Player.Loadouts[2].Armor, InventoryType.Loadout2Armor);
            UpdateSelectInventory(ref Player.Loadouts[2].Dye, InventoryType.Loadout2Dye);
            
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        }

        #endregion

        #region Save Data

        public override void SaveData(TagCompound tag)
        {
            tag.Add("NMCcanPickUpAnotherPlayersItems", canPickUpAnotherPlayersItems);
            base.SaveData(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            canPickUpAnotherPlayersItems = tag.GetBool("NMCcanPickUpAnotherPlayersItems");
            base.LoadData(tag);
        }

        #endregion

        #region Network Sync

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)(canPickUpAnotherPlayersItems ? 1 : 0));
			packet.Send(toWho, fromWho);
		}

		public void ReceivePlayerSync(BinaryReader reader) {
			canPickUpAnotherPlayersItems = reader.ReadBoolean();
		}

		public override void CopyClientState(ModPlayer targetCopy) {
			NMPlayer clone = (NMPlayer)targetCopy;
			clone.canPickUpAnotherPlayersItems = canPickUpAnotherPlayersItems;
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			NMPlayer clone = (NMPlayer)clientPlayer;

			if (canPickUpAnotherPlayersItems != clone.canPickUpAnotherPlayersItems)
            {
				SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
            }
		}

        #endregion
    }
}
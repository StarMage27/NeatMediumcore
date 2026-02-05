using System;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Audio;
using static NeatMediumcore.NeatMediumcore;
using System.IO;
using System.Security.Cryptography;

namespace NeatMediumcore;

public class NMPlayer : ModPlayer
{
    public bool canPickUpAnotherPlayersItems = false;
    public ushort playerID = ushort.MaxValue;

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        Player player = Main.LocalPlayer;
        if (NMBind.JustPressed && player.isMediumcore() && player.TryGetModPlayer(out NMPlayer nMPlayer))
        {
            nMPlayer.canPickUpAnotherPlayersItems = !nMPlayer.canPickUpAnotherPlayersItems;
        }
    }

    #region Logic

    public override bool OnPickup(Item item)
    {
        if (!Player.isMediumcore() || !item.TryGetGlobalItem(out NMGlobalItem nMItem))
        {
            return base.OnPickup(item);
        }

        ref ItemData itemData = ref nMItem.itemData;

        if(itemData.ownerID != playerID
           || !itemData.deathCountIsValid()
           || itemData.slotID == -1
           || itemData.inventoryType.isNone())
        {
            itemData.ownerID = playerID;
            return base.OnPickup(item);
        }

        item.newAndShiny = false;

        itemData.ownerID = playerID;
        InventoryType inventoryType = itemData.inventoryType;
        item.favorited = itemData.isFavorited();

        int currentLoadoutIndex = Player.CurrentLoadoutIndex;
        switch(inventoryType)
        {
            case InventoryType.Armor:
            {
                if (itemData.loadoutID == currentLoadoutIndex)
                {
                    return nMOnPickup(ref Player.armor, ref item);
                }
                else
                {
                    return nMOnPickup(ref Player.Loadouts[itemData.loadoutID].Armor, ref item);
                }
            }
            case InventoryType.Dye:
            {
                if (itemData.loadoutID == currentLoadoutIndex)
                {
                    return nMOnPickup(ref Player.dye, ref item);
                }
                else
                {
                    return nMOnPickup(ref Player.Loadouts[itemData.loadoutID].Dye, ref item);
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
            case InventoryType.InventoryFavorited:
            {
                return nMOnPickup(ref Player.inventory, ref item);
            }
            case InventoryType.None:
            case InventoryType.WingSlot:
            case InventoryType.ShoeSlot:
            case InventoryType.MoreAccessories:
            case InventoryType.PotionSlots:
            default:
            {
                return base.OnPickup(item);
            }
        }
    }

    private bool nMOnPickup(ref Item[] inventory, ref Item item)
    {
        if (!Main.LocalPlayer.isMediumcore() || !item.TryGetGlobalItem(out NMGlobalItem nMItem))
        {
            return base.OnPickup(item);
        }

        ref ItemData itemData = ref nMItem.itemData;
        
        if(itemData.ownerID != playerID)
        {
            return base.OnPickup(item);
        }
        
        int slotID = itemData.slotID;
        Item inventoryItem = inventory[slotID];

        if(inventoryItem == null || inventoryItem.IsAir)
        {
            // Replace air with an item and return false to not duplicate item
            inventory[slotID] = item;
            SoundEngine.PlaySound(item.IsACoin ? SoundID.CoinPickup : SoundID.Grab, Player.position);
            PopupText.NewText(PopupTextContext.RegularItemPickup, item, item.stack);
            return false;
        }
        
        if (!inventoryItem.TryGetGlobalItem(out NMGlobalItem inventoryNMItem))
        {
            return base.OnPickup(item);
        }
        ref ItemData invItemData = ref inventoryNMItem.itemData;

        if
        (
            item.favorited
            || (itemData.latestDeathCount < invItemData.latestDeathCount && !inventoryItem.favorited)
            || (!invItemData.deathCountIsValid() && !inventoryItem.favorited)
        )
        {
            inventory[slotID].favorited = false;

            invItemData.invalidataOwnerId();
            invItemData.invalidateDeathCount();
            invItemData.slotID = -1;
            invItemData.inventoryType = InventoryType.None;

            int spawnedItemID = Player.QuickSpawnItem(Player.GetSource_FromThis(), inventoryItem, inventoryItem.stack);
            if (Main.item[spawnedItemID].TryGetGlobalItem(out NMGlobalItem nMSpawnedItem))
            {
                nMSpawnedItem.itemData = invItemData;
            }
            inventory[slotID].TurnToAir();

            item.favorited = itemData.isFavorited();
            inventory[slotID] = item;

            SoundEngine.PlaySound(item.IsACoin ? SoundID.CoinPickup : SoundID.Grab, Player.position);

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
        if (!Main.LocalPlayer.isMediumcore())
        {
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        }

        ProcessItemsInInventory(ref Player.armor, InventoryType.Armor);
        ProcessItemsInInventory(ref Player.dye, InventoryType.Dye);

        for (byte i = 0; i < Player.Loadouts.Length; i++)
        {
            if (i == Player.CurrentLoadoutIndex)
            {
                ProcessItemsInInventory(ref Player.armor, InventoryType.Armor, i);
                ProcessItemsInInventory(ref Player.dye, InventoryType.Dye, i);
            }
            else
            {
                ProcessItemsInInventory(ref Player.Loadouts[i].Armor, InventoryType.Armor, i);
                ProcessItemsInInventory(ref Player.Loadouts[i].Dye, InventoryType.Dye, i);
            }
        }
    
        ProcessItemsInInventory(ref Player.inventory, InventoryType.Inventory);
        ProcessItemsInInventory(ref Player.miscEquips, InventoryType.MiscEquips);
        ProcessItemsInInventory(ref Player.miscDyes, InventoryType.MiscDyes);
            
        return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
    }

    private void ProcessItemsInInventory(ref Item[] inventory, InventoryType inventoryType, byte loadoutID = byte.MaxValue)
    {
        for (short slotID = 0; slotID < inventory.Length; slotID++)
        {
            Item item = inventory[slotID];
            if (item == null || item.IsAir || !item.TryGetGlobalItem(out NMGlobalItem nMItem)) continue;
            
            ref ItemData itemData = ref nMItem.itemData;
            
            // if (
            //     !itemData.deathCountIsValid()
            //     || (itemData.inventoryType != inventoryType)
            // )
            // {
            // }
            itemData.latestDeathCount = CountDeaths(Player);

            bool shouldBeFavorited = false;
            if (inventoryType.isInvOrInvFav() && item == Main.LocalPlayer.inventory[58]) // for the item in the mouse slot
            {
                itemData.slotID = -1;
                itemData.inventoryType = InventoryType.Inventory;
            }
            else
            {
                itemData.slotID = slotID;
                shouldBeFavorited = item.favorited && inventoryType.isInvOrInvFav();
            }
            
            itemData.inventoryType = shouldBeFavorited ? InventoryType.InventoryFavorited : inventoryType;
            
            itemData.ownerID = playerID;
            itemData.loadoutID = loadoutID;
        }
    }

    #endregion

    public override void OnEnterWorld()
    {
        if (playerID == ushort.MaxValue)
        {
            var randomGen = RandomNumberGenerator.Create();
            var bytes = new byte[2];
            randomGen.GetBytes(bytes);
            playerID = BitConverter.ToUInt16(bytes, 0);
        }
        base.OnEnterWorld();
    }

    #region Save Data

    public override void SaveData(TagCompound tag)
    {
        tag.Add("NMCo1", canPickUpAnotherPlayersItems);
        tag.Add("NMCPID", (short)playerID);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("NMCo1"))
        {
            canPickUpAnotherPlayersItems = tag.GetBool("NMCo1");
        }
        if (tag.ContainsKey("NMCPID"))
        {
            playerID = (ushort)tag.GetShort("NMCPID");
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
        playerID = reader.ReadUInt16();
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

public static class PlayerExtensions
{
    public static bool isMediumcore(this Player player)
    {
        return player.difficulty.Equals(PlayerDifficultyID.MediumCore);
    }
    
    public static uint countDeaths(this Player player)
    {
        return (uint)player.numberOfDeathsPVE + (uint)player.numberOfDeathsPVP;
    }
}
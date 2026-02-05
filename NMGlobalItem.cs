using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.IO;
using Terraria.ModLoader.IO;
using System;
using static NeatMediumcore.NeatMediumcore;
using NeatMediumcore.Config;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Enums;
using Terraria.ID;

namespace NeatMediumcore;

public class NMGlobalItem : GlobalItem
{
    public ItemData itemData = ItemData.defaultData();
    
    public override bool InstancePerEntity => true;

    public override void UpdateInventory(Item item, Player player)
    {
        base.UpdateInventory(item, player);
        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem)) { return; }
        ref ItemData itemData = ref nMItem.itemData;
        if (!itemData.inventoryType.isInventory())
        {
            itemData = ItemData.defaultData();
        }
    }

    public override bool ItemSpace(Item item, Player player)
    {
        if (
            !item.TryGetGlobalItem(out NMGlobalItem nMItem) ||
            !player.TryGetModPlayer(out NMPlayer nMPlayer)
        )
        {
            return base.ItemSpace(item, player);
        }

        uint playerDeathCount = CountDeaths(player);
        
        ref ItemData itemDataS = ref nMItem.itemData;
        
        if(itemDataS.latestDeathCount < playerDeathCount && itemDataS.ownerID == nMPlayer.playerID && itemDataS.deathCountIsValid())
        {
            return true;
        }
        else
        {
            return base.ItemSpace(item, player);
        }
    }

    public override void PostUpdate(Item item)
    {
        base.PostUpdate(item);
        bool darkSoulsMode = ModContent.GetInstance<NMServerConfig>().DarkSoulsModeToggle;
        bool itemsGlow = ModContent.GetInstance<NMConfig>().ItemsGlowToggle;
        if (!darkSoulsMode && !itemsGlow) { return; }

        bool isServer = Main.netMode == NetmodeID.Server;
        bool isMultiplayerClient = Main.netMode == NetmodeID.MultiplayerClient;
        bool isSinglePlayer = Main.netMode == NetmodeID.SinglePlayer;
        
        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem)) { return; }
        ref ItemData itemData = ref nMItem.itemData;

        Team itemOwnerTeam = Team.None;
        if (isMultiplayerClient)
        {
            var players = Main.ActivePlayers;
            ushort ItemOwnerID = itemData.ownerID;
            foreach (var player in players)
            {
                if (!player.TryGetModPlayer(out NMPlayer nMPlayer)) { continue; }

                if (nMPlayer.playerID != ItemOwnerID) continue;

                itemOwnerTeam = (Team)player.team;
            }
        }
        
        if (!isServer && itemData.ownerIdIsValid() && itemsGlow)
        {
            switch(itemOwnerTeam)
            {
                case Team.Red:
                {
                    Lighting.AddLight(item.Center, 1f, 0f, 0f);
                    break;
                }
                case Team.Green:
                {
                    Lighting.AddLight(item.Center, 0f, 0.5f, 0f);
                    break;
                }
                case Team.Blue:
                {
                    Lighting.AddLight(item.Center, 0f, 0f, 1f);
                    break;
                }
                case Team.Yellow:
                {
                    Lighting.AddLight(item.Center, 0.5f, 0.5f, 0f);
                    break;
                }
                case Team.Pink:
                {
                    Lighting.AddLight(item.Center, 0.5f, 0f, 0.5f);
                    break;
                }
                case Team.None:
                default:
                {
                    Lighting.AddLight(item.Center, 0.33f, 0.33f, 0.33f);
                    break;
                }
            }
        }

        if (!darkSoulsMode) { return; }
        
        bool ownerIsActive = false;
        uint ownerDeathCount = 0;

        if (isServer)
        {
            var players = Main.ActivePlayers;
            foreach (var player in players)
            {
                if (!player.TryGetModPlayer(out NMPlayer nMPlayer)) { continue; }

                if (nMPlayer.playerID != itemData.ownerID) continue;

                ownerIsActive = true;
                ownerDeathCount = player.countDeaths();
            }
        }
        else if (isSinglePlayer)
        {
            Player player = Main.LocalPlayer;
            if (player.TryGetModPlayer(out NMPlayer nMPlayer)) {
                ownerIsActive = itemData.ownerID.Equals(nMPlayer.playerID);
                ownerDeathCount = player.countDeaths();
            }
        }

        if (ownerIsActive && itemData.deathCountIsValid() && itemData.latestDeathCount + 1 < ownerDeathCount)
        {
            item.TurnToAir();
            item = null;
        }
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        base.OnSpawn(item, source);
        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem)) {
            return;
        }

        if (source is not EntitySource_Death { Entity: Player })
        {
            nMItem.itemData = ItemData.defaultData();
        }
    }

    public override bool CanPickup(Item item, Player player)
    {
        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem) || !player.TryGetModPlayer(out NMPlayer nMPlayer))
        {
            return base.CanPickup(item, player);
        }
        
        if (nMItem.itemData.ownerID == nMPlayer.playerID || nMPlayer.canPickUpAnotherPlayersItems || !nMItem.itemData.ownerIdIsValid() || itemData.ownerID == 0)
        {
            return base.CanPickup(item, player);
        }
        else
        {
            return false;
        }
    }

    public override bool CanStackInWorld(Item destination, Item source)
    {
        if (!destination.TryGetGlobalItem(out NMGlobalItem nMDst) || !source.TryGetGlobalItem(out NMGlobalItem nMSrc))
        {
            return base.CanStackInWorld(destination, source);
        }
        
        ref ItemData itemDataDst = ref nMDst.itemData; // Destination data
        ref ItemData itemDataSrc = ref nMSrc.itemData; // Source data

        if (itemDataDst.ownerID == itemDataSrc.ownerID)
        {
            if (itemDataDst.slotID != itemDataSrc.slotID
                || !itemDataSrc.inventoryType.almostEquals(InventoryType.InventoryFavorited)
                || itemDataSrc.loadoutID != itemDataDst.loadoutID)
            {
                return false;
            }
            else
            {
                return base.CanStackInWorld(destination, source);
            }
        }
        else
        {
            return false;
        }
    }

    public override GlobalItem Clone(Item from, Item to)
    {
        if (!from.TryGetGlobalItem(out NMGlobalItem nMFrom) || !to.TryGetGlobalItem(out NMGlobalItem nMTo))
        {
            return base.Clone(from, to);
        }
        
        ref ItemData itemDataTo = ref nMTo.itemData;
        ref ItemData itemDataFrom = ref nMFrom.itemData;

        itemDataTo = itemDataFrom;
            
        return base.Clone(from, to);
    }

    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        if (!destination.TryGetGlobalItem(out NMGlobalItem nMDst) || !source.TryGetGlobalItem(out NMGlobalItem nMSrc))
        {
            base.OnStack(destination, source, numToTransfer);
            return;
        }
        
        ref ItemData itemDataDst = ref nMDst.itemData; // Destination data
        ref ItemData itemDataSrc = ref nMSrc.itemData; // Source data

        uint earliestDeath = Math.Min(itemDataSrc.latestDeathCount, itemDataDst.latestDeathCount);
        
        itemDataDst.latestDeathCount = earliestDeath;
        itemDataSrc = itemDataDst;
        
        base.OnStack(destination, source, numToTransfer);
    }

    public override void SplitStack(Item destination, Item source, int numToTransfer)
    {
        if (!destination.TryGetGlobalItem(out NMGlobalItem nMDst) || !source.TryGetGlobalItem(out NMGlobalItem nMSrc))
        {
            base.SplitStack(destination, source, numToTransfer);
            return;
        }

        nMDst.itemData.latestDeathCount = nMSrc.itemData.latestDeathCount;
        base.SplitStack(destination, source, numToTransfer);
    }

    #region Debug

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if(ModContent.GetInstance<NMConfig>().ShowDebugInfoInventoryToggle && item.TryGetGlobalItem(out NMGlobalItem nMItem))
        {
            ref ItemData itemData = ref nMItem.itemData;
            
            tooltips.Add(new TooltipLine(Mod, "Tooltip100", $"[c/FF8888:Inventory Type:] {itemData.inventoryType}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip101", $"[c/FF8888:Slot ID:] {itemData.slotID}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip102", $"[c/FF8888:Owner ID:] {itemData.ownerID}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip103", $"[c/FF8888:Latest Death Count:] {itemData.latestDeathCount}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip105", $"[c/FF8888:Loadout:] {itemData.loadoutID}"));
        }
        base.ModifyTooltips(item, tooltips);
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if(ModContent.GetInstance<NMConfig>().ShowDebugInfoDroppedToggle && item.TryGetGlobalItem(out NMGlobalItem nMItem))
        {
            var position = item.Center - Main.screenPosition;
            ref ItemData itemData = ref nMItem.itemData;
            
            string text = $"Inventory: {itemData.inventoryType}\n" +
                          $"Slot: {itemData.slotID}\n" +
                          $"Owner: {itemData.ownerID}\n" +
                          $"Latest Death: {itemData.latestDeathCount}\n" +
                          $"Loadout: {itemData.loadoutID}";
            Utils.DrawBorderString(spriteBatch, text, position, Color.White);
        }
        base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    #endregion

    #region Network Sync

    public override void NetSend(Item item, BinaryWriter writer)
    {
        base.NetSend(item, writer);

        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem)) { return; }

        byte[] data = nMItem.itemData.serialize();
        
        writer.Write(data.Length);
        writer.Write(data);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        base.NetReceive(item, reader);
        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem)) { return; }

        int length = reader.ReadInt32();
        nMItem.itemData = reader.ReadBytes(length).deserializeToItemData();
    }

    #endregion

    #region Save Data
    public override void SaveData(Item item, TagCompound tag)
    {
        base.SaveData(item, tag);
        if (!item.TryGetGlobalItem(out NMGlobalItem nMItem)) { return; }

        tag.Add("NMCData", nMItem.itemData.serialize());
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        base.LoadData(item, tag);
        if (!tag.ContainsKey("NMCData") || !item.TryGetGlobalItem(out NMGlobalItem nMItem)) { return; }

        nMItem.itemData = tag.GetByteArray("NMCData").deserializeToItemData();
    }

    #endregion
}
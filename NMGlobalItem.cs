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

// ReSharper disable once ClassNeverInstantiated.Global
public class NMGlobalItem : GlobalItem
{
    // public int nMOwnerID = -1;
    // public int nMLatestDeathCount = -1;
    // public short nMSlotID = -1;
    // public byte nMLoadoutID = 255;
    // public InventoryType nMInventoryType = InventoryType.None;
    // public bool nMFavourited = false;
    public ItemData itemData = new();
        
    public override bool InstancePerEntity => true;

    public override bool ItemSpace(Item item, Player player)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
        NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();
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
        bool darkSoulsMode = ModContent.GetInstance<NMConfig>().DarkSoulsModeToggle;
        bool itemsGlow = ModContent.GetInstance<NMConfig>().ItemsGlowToggle;
        if (!darkSoulsMode && !itemsGlow)
        {
            base.PostUpdate(item);
            return;
        }

        bool isServer = Main.netMode == NetmodeID.Server;
        bool isMultiplayerClient = Main.netMode == NetmodeID.MultiplayerClient;
        bool isSinglePlayer = Main.netMode == NetmodeID.SinglePlayer;
        
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
        ref ItemData itemDataU = ref nMItem.itemData;

        Team itemOwnerTeam = Team.None;
        if (isMultiplayerClient)
        {
            var players = Main.ActivePlayers;
            ushort ItemOwnerID = itemDataU.ownerID;
            foreach (var player in players)
            {
                NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();

                if (nMPlayer.playerID != ItemOwnerID) continue;

                itemOwnerTeam = (Team)player.team;
            }
        }
        
        if (!isServer && itemDataU.ownerIdIsValid() && itemsGlow)
        {
            //Lighting.AddLight(item.Center, 0.33f, 0.33f, 0.33f);
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

        if (!darkSoulsMode)
        {
            base.PostUpdate(item);
            return;
        }
        
        bool ownerIsActive = false;
        uint ownerDeathCount = 0;

        if (isServer)
        {
            var players = Main.ActivePlayers;
            ushort ItemOwnerID = itemDataU.ownerID;
            foreach (var player in players)
            {
                NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();

                if (nMPlayer.playerID != ItemOwnerID) continue;

                ownerIsActive = true;
                ownerDeathCount = player.countDeaths();
            }
        }
        else if (isSinglePlayer)
        {
            Player player = Main.LocalPlayer;
            NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();

            ownerIsActive = itemDataU.ownerID.Equals(nMPlayer.playerID);
            ownerDeathCount = player.countDeaths();
        }

        if (ownerIsActive && itemDataU.latestDeathCount < ownerDeathCount)
        {
            item.TurnToAir();
            item = null;
        }

        base.PostUpdate(item);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
        ref ItemData itemDataO = ref nMItem.itemData;

        if (source is not EntitySource_Death { Entity: Player })
        {
            itemDataO = new ItemData();
        }

        base.OnSpawn(item, source);
    }

    public override bool CanPickup(Item item, Player player)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
        NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();
        ref ItemData itemDataC = ref nMItem.itemData;
        
        if (itemDataC.ownerID == nMPlayer.playerID || nMPlayer.canPickUpAnotherPlayersItems || !itemDataC.ownerIdIsValid())
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
        NMGlobalItem nMDst = destination.GetGlobalItem<NMGlobalItem>(); // Destination
        NMGlobalItem nMSrc = source.GetGlobalItem<NMGlobalItem>(); // Source
        
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
        if (from.IsAir || to.IsAir) return base.Clone(from, to);
            
        NMGlobalItem nMFrom = from.GetGlobalItem<NMGlobalItem>();
        NMGlobalItem nMTo = to.GetGlobalItem<NMGlobalItem>();
        
        ref ItemData itemDataTo = ref nMTo.itemData;
        ref ItemData itemDataFrom = ref nMFrom.itemData;

        itemDataTo = itemDataFrom;
        
        // nMTo.nMLatestDeathCount = nMFrom.nMLatestDeathCount;
        // nMTo.nMSlotID = nMFrom.nMSlotID;
        // nMTo.nMInventoryType = nMFrom.nMInventoryType;
        // nMTo.nMFavourited = nMFrom.nMFavourited;
        // nMTo.nMLoadoutID = nMFrom.nMLoadoutID;
        // nMTo.nMOwnerID = nMFrom.nMOwnerID;
            
        return base.Clone(from, to);
    }

    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        NMGlobalItem nMDst = destination.GetGlobalItem<NMGlobalItem>(); // Destination
        NMGlobalItem nMSrc = source.GetGlobalItem<NMGlobalItem>(); // Source
        
        ref ItemData itemDataDst = ref nMDst.itemData; // Destination data
        ref ItemData itemDataSrc = ref nMSrc.itemData; // Source data

        uint earliestDeath = Math.Min(itemDataSrc.latestDeathCount, itemDataDst.latestDeathCount);
        
        itemDataDst.latestDeathCount = earliestDeath;
        itemDataSrc = itemDataDst;
        
        // int earliestDeath;
        // if (nMSrc.nMLatestDeathCount == -1 || nMDst.nMLatestDeathCount == -1)
        // {
        //     earliestDeath = Math.Max(nMSrc.nMLatestDeathCount, nMDst.nMLatestDeathCount);
        // }
        // else
        // {
        //     earliestDeath = Math.Min(nMSrc.nMLatestDeathCount, nMDst.nMLatestDeathCount);
        // }
        //     
        // nMDst.nMLatestDeathCount = earliestDeath;
        // nMSrc.nMSlotID = nMDst.nMSlotID;
        // nMSrc.nMInventoryType = nMDst.nMInventoryType;
        // nMSrc.nMLoadoutID = nMDst.nMLoadoutID;
        // nMSrc.nMFavourited = nMDst.nMFavourited;

        base.OnStack(destination, source, numToTransfer);
    }

    public override void SplitStack(Item destination, Item source, int numToTransfer)
    {
        destination.GetGlobalItem<NMGlobalItem>().itemData.latestDeathCount = source.GetGlobalItem<NMGlobalItem>().itemData.latestDeathCount;
        base.SplitStack(destination, source, numToTransfer);
    }

    #region Debug

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if(ModContent.GetInstance<NMConfig>().ShowDebugInfoInventoryToggle)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            ref ItemData itemDataM = ref nMItem.itemData;
            
            tooltips.Add(new TooltipLine(Mod, "Tooltip100", $"[c/FF8888:Inventory Type:] {itemDataM.inventoryType}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip101", $"[c/FF8888:Slot ID:] {itemDataM.slotID}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip102", $"[c/FF8888:Owner ID:] {itemDataM.ownerID}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip103", $"[c/FF8888:Latest Death Count:] {itemDataM.latestDeathCount}"));
            //tooltips.Add(new TooltipLine(Mod, "Tooltip104", $"[c/FF8888:Favourited:] {itemDataM.favourited}"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip105", $"[c/FF8888:Loadout:] {itemDataM.loadoutID}"));
        }
        base.ModifyTooltips(item, tooltips);
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if(ModContent.GetInstance<NMConfig>().ShowDebugInfoDroppedToggle)
        {
            var position = item.Center - Main.screenPosition;
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            ref ItemData itemDataP = ref nMItem.itemData;
            
            string text = $"Inventory: {itemDataP.inventoryType}\n" +
                          $"Slot: {itemDataP.slotID}\n" +
                          $"Owner: {itemDataP.ownerID}\n" +
                          $"Latest Death: {itemDataP.latestDeathCount}\n" +
                          //$"Favourited: {itemDataP.favourited}\n" +
                          $"Loadout: {itemDataP.loadoutID}";
            Utils.DrawBorderString(spriteBatch, text, position, Color.White);
        }
        base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    #endregion

    #region Network Sync

    public override void NetSend(Item item, BinaryWriter writer)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
        ItemData itemDataN = nMItem.itemData;

        byte[] data = itemDataN.serialize();
        
        writer.Write(data.Length);
        writer.Write(data);
        
        // writer.Write(nMItem.nMLatestDeathCount);
        // writer.Write(nMItem.nMSlotID);
        // writer.Write(nMItem.nMOwnerID);
        // writer.Write(nMItem.nMLoadoutID);
        // writer.Write((short)nMItem.nMInventoryType);
        // writer.Write(nMItem.nMFavourited);

        base.NetSend(item, writer);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();

        int length = reader.ReadInt32();
        byte[] data = reader.ReadBytes(length);
        ItemData itemDataN = data.deserializeToItemData();
        nMItem.itemData = itemDataN;
        
        // nMItem.nMLatestDeathCount = reader.ReadInt32();
        // nMItem.nMSlotID = reader.ReadInt16();
        // nMItem.nMOwnerID = reader.ReadInt32();
        // nMItem.nMLoadoutID = reader.ReadByte();
        // nMItem.nMInventoryType = (InventoryType)reader.ReadByte();
        // nMItem.nMFavourited = reader.ReadBoolean();

        base.NetReceive(item, reader);
    }

    #endregion

    #region Save Data
    public override void SaveData(Item item, TagCompound tag)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
        ItemData itemDataS = nMItem.itemData;
        byte[] data = itemDataS.serialize();
        tag.Add("NMCData", data);
        
        // tag.Add("NMCLDC", nMItem.nMLatestDeathCount);
        // tag.Add("NMCSID", nMItem.nMSlotID);
        // tag.Add("NMCOID", nMItem.nMOwnerID);
        // tag.Add("NMCLID", nMItem.nMLoadoutID);
        // tag.Add("NMCIT", (byte)nMItem.nMInventoryType);
        // tag.Add("NMCF", nMItem.nMFavourited);
        base.SaveData(item, tag);
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();

        if (tag.ContainsKey("NMCData"))
        {
            byte[] data = tag.GetByteArray("NMCData");

            ItemData itemDataN = data.deserializeToItemData();
            nMItem.itemData = itemDataN;
        }
        
        // if (tag.ContainsKey("NMCLDC"))
        // {
        //     nMItem.nMLatestDeathCount = tag.GetInt("NMCLDC");
        // }
        // if (tag.ContainsKey("NMCSID"))
        // {
        //     nMItem.nMSlotID = tag.GetShort("NMCSID");
        // }
        // if (tag.ContainsKey("NMCOID"))
        // {
        //     nMItem.nMOwnerID = tag.GetInt("NMCOID");
        // }
        // if (tag.ContainsKey("NMCLID"))
        // {
        //     nMItem.nMLoadoutID = tag.GetByte("NMCLID");
        // }
        // if (tag.ContainsKey("NMCIT"))
        // {
        //     nMItem.nMInventoryType = (InventoryType)tag.GetShort("NMCIT");
        // }
        // if (tag.ContainsKey("NMCF"))
        // {
        //     nMItem.nMFavourited = tag.GetBool("NMCF");
        // }
        base.LoadData(item, tag);
    }

    #endregion
}
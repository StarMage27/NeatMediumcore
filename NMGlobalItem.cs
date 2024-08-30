using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.DataStructures;
using System.IO;
using Terraria.ModLoader.IO;
using System;
using ReLogic.Utilities;
using NeatMediumcore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Steamworks;
using Terraria.Audio;
using static NeatMediumcore.NeatMediumcore;
using NeatMediumcore.Config;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Numerics;
using System.Collections.Generic;

namespace NeatMediumcore
{
    public class NMGlobalItem : GlobalItem
    {
        public InventoryType inventoryType = InventoryType.None;
        public int nMSlotID = -1;
        public int nMOwnerID = -1;
        public int nMLatestDeathCount = -1;
        public bool nMFavourited = false;
        public int nMLoadoutID = -1;
        
        public override bool InstancePerEntity => true;

        public override bool ItemSpace(Item item, Player player)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();
            int playerDeathCount = CountDeaths(player);
            if(nMItem.nMLatestDeathCount < playerDeathCount && nMItem.nMOwnerID == nMPlayer.playerID && nMItem.nMLatestDeathCount != -1)
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
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            bool itemsGlow = ModContent.GetInstance<NMConfig>().ItemsGlowToggle;
            if (nMItem.nMOwnerID != -1 && itemsGlow)
            {
                switch(Main.player[nMItem.nMOwnerID].team)
                {
                    case 1: // Red Team
                    {
                        Lighting.AddLight(item.Center, 1f, 0f, 0f);
                        break;
                    }
                    case 2: // Green Team
                    {
                        Lighting.AddLight(item.Center, 0f, 0.5f, 0f);
                        break;
                    }
                    case 3: // Blue Team
                    {
                        Lighting.AddLight(item.Center, 0f, 0f, 1f);
                        break;
                    }
                    case 4: // Yellow Team
                    {
                        Lighting.AddLight(item.Center, 0.5f, 0.5f, 0f);
                        break;
                    }
                    case 5: // Pink Team
                    {
                        Lighting.AddLight(item.Center, 0.5f, 0f, 0.5f);
                        break;
                    }
                    default: 
                    {
                        Lighting.AddLight(item.Center, 0.33f, 0.33f, 0.33f);
                        break;
                    }
                }
            }

            base.PostUpdate(item);
        }

        public override void OnSpawn(Item item, IEntitySource source)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();

            if(source is EntitySource_Death parent && parent.Entity is Player player)
            {                
                base.OnSpawn(item, source);
            }
            else
            {
                nMItem.nMOwnerID = -1;
                nMItem.nMLatestDeathCount = -1;
                nMItem.nMSlotID = -1;
                nMItem.inventoryType = InventoryType.None;
                nMItem.nMFavourited = false;
                base.OnSpawn(item, source);
            }
        }

        public override bool CanPickup(Item item, Player player)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();
            if (nMItem.nMOwnerID == nMPlayer.playerID || nMPlayer.canPickUpAnotherPlayersItems == true || nMItem.nMOwnerID == -1)
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
            NMGlobalItem nMDestination = destination.GetGlobalItem<NMGlobalItem>();
            NMGlobalItem nMSource = source.GetGlobalItem<NMGlobalItem>();

            if (nMDestination.nMOwnerID == nMSource.nMOwnerID)
            {
                if (nMDestination.nMSlotID != nMSource.nMSlotID || nMDestination.inventoryType != nMSource.inventoryType)
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
            if (!from.IsAir && from != null && !to.IsAir && to != null)
            {
                NMGlobalItem nMFrom = from.GetGlobalItem<NMGlobalItem>();
                NMGlobalItem nMTo = to.GetGlobalItem<NMGlobalItem>();

                nMTo.nMLatestDeathCount = nMFrom.nMLatestDeathCount;
                nMTo.nMSlotID = nMFrom.nMSlotID;
                nMTo.inventoryType = nMFrom.inventoryType;
                nMTo.nMFavourited = nMFrom.nMFavourited;
                nMTo.nMOwnerID = nMFrom.nMOwnerID;
            }
            return base.Clone(from, to);
        }

        public override void OnStack(Item destination, Item source, int numToTransfer)
        {
            NMGlobalItem nMDestination = destination.GetGlobalItem<NMGlobalItem>();
            NMGlobalItem nMSource = source.GetGlobalItem<NMGlobalItem>();

            int earliestDeath;
            if (nMSource.nMLatestDeathCount == -1 && nMDestination.nMLatestDeathCount == -1)
            {
                earliestDeath = -1;
            }
            else if (nMSource.nMLatestDeathCount == -1)
            {
                earliestDeath = nMDestination.nMLatestDeathCount;
            }
            else if (nMDestination.nMLatestDeathCount == -1)
            {
                earliestDeath = nMSource.nMLatestDeathCount;
            }
            else
            {
                earliestDeath = Math.Min(nMSource.nMLatestDeathCount, nMDestination.nMLatestDeathCount);
            }
            
            nMDestination.nMLatestDeathCount = earliestDeath;
            nMSource.nMSlotID = nMDestination.nMSlotID;
            nMSource.inventoryType = nMDestination.inventoryType;
            nMSource.nMFavourited = nMDestination.nMFavourited;

            base.OnStack(destination, source, numToTransfer);
        }

        public override void SplitStack(Item destination, Item source, int numToTransfer)
        {
            destination.GetGlobalItem<NMGlobalItem>().nMLatestDeathCount = source.GetGlobalItem<NMGlobalItem>().nMLatestDeathCount;
            base.SplitStack(destination, source, numToTransfer);
        }

        #region Debug

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if(ModContent.GetInstance<NMConfig>().ShowDebugInfoInventoryToggle)
            {
                NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
                tooltips.Add(new TooltipLine(Mod, "Tooltip100", $"[c/FF8888:Inventory Type:] {nMItem.inventoryType}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip101", $"[c/FF8888:Slot ID:] {nMItem.nMSlotID}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip102", $"[c/FF8888:Owner ID:] {nMItem.nMOwnerID}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip103", $"[c/FF8888:Latest Death Count:] {nMItem.nMLatestDeathCount}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip104", $"[c/FF8888:Favourited:] {nMItem.nMFavourited}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip105", $"[c/FF8888:Loadout:] {nMItem.nMLoadoutID}"));
            }
            base.ModifyTooltips(item, tooltips);
        }

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if(ModContent.GetInstance<NMConfig>().ShowDebugInfoDroppedToggle)
            {
                var position = item.Center - Main.screenPosition;
                NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
                string text = $"Inventory: {nMItem.inventoryType}\nSlot: {nMItem.nMSlotID}\nOwner: {nMItem.nMOwnerID}\nLatest Death: {nMItem.nMLatestDeathCount}\nFavourited: {nMItem.nMFavourited}\nLoadout: {nMItem.nMLoadoutID}";
                Utils.DrawBorderString(spriteBatch, text, position, Color.White);
            }
            base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
        }

        #endregion

        #region Network Sync

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(nMLatestDeathCount);
            writer.Write(nMOwnerID);
            writer.Write(nMSlotID);
            writer.Write((int)inventoryType);
            writer.Write(nMFavourited);
            writer.Write(nMLoadoutID);

            base.NetSend(item, writer);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            nMLatestDeathCount = reader.ReadInt32();
            nMOwnerID = reader.ReadInt32();
            nMSlotID = reader.ReadInt32();
            inventoryType = (InventoryType)reader.ReadInt32();
            nMFavourited = reader.ReadBoolean();
            nMLoadoutID = reader.ReadInt32();

            base.NetReceive(item, reader);
        }

        #endregion

        #region Save Data
        public override void SaveData(Item item, TagCompound tag)
        {
            tag.Add("NMInventoryType", (int)inventoryType);
            tag.Add("NMSlotID", nMSlotID);
            tag.Add("NMOwnerID", nMOwnerID);
            tag.Add("NMLatestDeathCount", nMLatestDeathCount);
            tag.Add("NMFavourited", nMFavourited);
            tag.Add("NMLoadoutID", nMLoadoutID);
            base.SaveData(item, tag);
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (tag.ContainsKey("NMInventoryType"))
            {
                inventoryType = (InventoryType)tag.GetInt("NMInventoryType");
            }
            if (tag.ContainsKey("NMSlotID"))
            {
                nMSlotID = tag.GetInt("NMSlotID");
            }
            if (tag.ContainsKey("NMOwnerID"))
            {
                nMOwnerID = tag.GetInt("NMOwnerID");
            }
            if (tag.ContainsKey("NMLatestDeathCount"))
            {
                nMLatestDeathCount = tag.GetInt("NMLatestDeathCount");
            }
            if (tag.ContainsKey("NMFavourited"))
            {
                nMFavourited = tag.GetBool("NMFavourited");
            }
            if (tag.ContainsKey("NMLoadoutID"))
            {
                nMLoadoutID = tag.GetInt("NMLoadoutID");
            }
            base.LoadData(item, tag);
        }

        #endregion
    }
}
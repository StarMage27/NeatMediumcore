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
        public int nMSlotID = -1;
        public int nMOwnerID = -1;
        public int nMLatestDeathCount = -1;
        public int nMLoadoutID = -1;
        public bool nMFavourited = false;
        public InventoryType nMInventoryType = InventoryType.None;
        
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
            //NMPlayer nMPlayer = Main.LocalPlayer.GetModPlayer<NMPlayer>();

            bool itemsGlow = ModContent.GetInstance<NMConfig>().ItemsGlowToggle;
            if (nMItem.nMOwnerID != -1 && itemsGlow)
            {
                Lighting.AddLight(item.Center, 0.33f, 0.33f, 0.33f);
                // switch(Main.player[playerRealID].team)
                // {
                //     case 1: // Red Team
                //     {
                //         Lighting.AddLight(item.Center, 1f, 0f, 0f);
                //         break;
                //     }
                //     case 2: // Green Team
                //     {
                //         Lighting.AddLight(item.Center, 0f, 0.5f, 0f);
                //         break;
                //     }
                //     case 3: // Blue Team
                //     {
                //         Lighting.AddLight(item.Center, 0f, 0f, 1f);
                //         break;
                //     }
                //     case 4: // Yellow Team
                //     {
                //         Lighting.AddLight(item.Center, 0.5f, 0.5f, 0f);
                //         break;
                //     }
                //     case 5: // Pink Team
                //     {
                //         Lighting.AddLight(item.Center, 0.5f, 0f, 0.5f);
                //         break;
                //     }
                //     default: 
                //     {
                //         Lighting.AddLight(item.Center, 0.33f, 0.33f, 0.33f);
                //         break;
                //     }
                // }
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
                nMItem.nMInventoryType = InventoryType.None;
                nMItem.nMLoadoutID = -1;
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
                if (nMDestination.nMSlotID != nMSource.nMSlotID || nMDestination.nMInventoryType != nMSource.nMInventoryType || nMSource.nMLoadoutID != nMDestination.nMLoadoutID)
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
                nMTo.nMInventoryType = nMFrom.nMInventoryType;
                nMTo.nMFavourited = nMFrom.nMFavourited;
                nMTo.nMLoadoutID = nMFrom.nMLoadoutID;
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
            nMSource.nMInventoryType = nMDestination.nMInventoryType;
            nMSource.nMLoadoutID = nMDestination.nMLoadoutID;
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
                tooltips.Add(new TooltipLine(Mod, "Tooltip100", $"[c/FF8888:Inventory Type:] {nMItem.nMInventoryType}"));
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
                string text = $"Inventory: {nMItem.nMInventoryType}\nSlot: {nMItem.nMSlotID}\nOwner: {nMItem.nMOwnerID}\nLatest Death: {nMItem.nMLatestDeathCount}\nFavourited: {nMItem.nMFavourited}\nLoadout: {nMItem.nMLoadoutID}";
                Utils.DrawBorderString(spriteBatch, text, position, Color.White);
            }
            base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
        }

        #endregion

        #region Network Sync

        public override void NetSend(Item item, BinaryWriter writer)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            writer.Write(nMItem.nMLatestDeathCount);
            writer.Write(nMItem.nMSlotID);
            writer.Write(nMItem.nMOwnerID);
            writer.Write(nMItem.nMLoadoutID);
            writer.Write((int)nMItem.nMInventoryType);
            writer.Write(nMItem.nMFavourited);

            base.NetSend(item, writer);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            nMItem.nMLatestDeathCount = reader.ReadInt32();
            nMItem.nMSlotID = reader.ReadInt32();
            nMItem.nMOwnerID = reader.ReadInt32();
            nMItem.nMLoadoutID = reader.ReadInt32();
            nMItem.nMInventoryType = (InventoryType)reader.ReadInt32();
            nMItem.nMFavourited = reader.ReadBoolean();

            base.NetReceive(item, reader);
        }

        #endregion

        #region Save Data
        public override void SaveData(Item item, TagCompound tag)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            tag.Add("NMLatestDeathCount", nMItem.nMLatestDeathCount);
            tag.Add("NMSlotID", nMItem.nMSlotID);
            tag.Add("NMOwnerID", nMItem.nMOwnerID);
            tag.Add("NMLoadoutID", nMItem.nMLoadoutID);
            tag.Add("NMInventoryType", (int)nMItem.nMInventoryType);
            tag.Add("NMFavourited", nMItem.nMFavourited);
            base.SaveData(item, tag);
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            if (tag.ContainsKey("NMLatestDeathCount"))
            {
                nMItem.nMLatestDeathCount = tag.GetInt("NMLatestDeathCount");
            }
            if (tag.ContainsKey("NMSlotID"))
            {
                nMItem.nMSlotID = tag.GetInt("NMSlotID");
            }
            if (tag.ContainsKey("NMOwnerID"))
            {
                nMItem.nMOwnerID = tag.GetInt("NMOwnerID");
            }
            if (tag.ContainsKey("NMLoadoutID"))
            {
                nMItem.nMLoadoutID = tag.GetInt("NMLoadoutID");
            }
            if (tag.ContainsKey("NMInventoryType"))
            {
                nMItem.nMInventoryType = (InventoryType)tag.GetInt("NMInventoryType");
            }
            if (tag.ContainsKey("NMFavourited"))
            {
                nMItem.nMFavourited = tag.GetBool("NMFavourited");
            }
            base.LoadData(item, tag);
        }

        #endregion
    }
}
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
        public InventoryType inventoryType;
        public int slotID;
        public int ownerID = -1;
        public int latestDeathCount = -1;
        public bool nMFavourited;
        
        public override bool InstancePerEntity => true;

        public override bool ItemSpace(Item item, Player player)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            int playerDeathCount = CountDeaths(player);
            if(nMItem.latestDeathCount < playerDeathCount && nMItem.ownerID == player.whoAmI && nMItem.latestDeathCount != -1)
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
            if (nMItem.ownerID != -1 && itemsGlow)
            {
                switch(Main.player[nMItem.ownerID].team)
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
                //nMItem.ownerID = player.whoAmI;
                //nMItem.latestDeathCount = CountDeaths(player);
                
                base.OnSpawn(item, source);
            }
            else
            {
                nMItem.ownerID = -1;
                nMItem.latestDeathCount = -1;
                nMItem.slotID = -1;
                nMItem.inventoryType = InventoryType.None;
                nMItem.nMFavourited = false;
                base.OnSpawn(item, source);
            }
        }

        public override bool CanPickup(Item item, Player player)
        {
            NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
            NMPlayer nMPlayer = player.GetModPlayer<NMPlayer>();
            if (nMItem.ownerID == player.whoAmI || nMPlayer.canPickUpAnotherPlayersItems == true || nMItem.ownerID == -1)
            {
                return base.CanPickup(item, player);
            }
            else
            {
                return false;
            }
        }

        

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if(ModContent.GetInstance<NMConfig>().ShowDebugInfoToggle)
            {
                NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
                tooltips.Add(new TooltipLine(Mod, "Tooltip100", $"[c/FF8888:Inventory Type:] {nMItem.inventoryType}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip101", $"[c/FF8888:Slot ID:] {nMItem.slotID}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip102", $"[c/FF8888:Owner ID:] {nMItem.ownerID}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip103", $"[c/FF8888:Latest Death Count:] {nMItem.latestDeathCount}"));
                tooltips.Add(new TooltipLine(Mod, "Tooltip104", $"[c/FF8888:Favourited:] {nMItem.nMFavourited}"));
            }
            base.ModifyTooltips(item, tooltips);
        }

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if(ModContent.GetInstance<NMConfig>().ShowDebugInfoToggle)
            {
                var position = item.Center - Main.screenPosition;
                NMGlobalItem nMItem = item.GetGlobalItem<NMGlobalItem>();
                string text = $"Inventory: {nMItem.inventoryType}\nSlot: {nMItem.slotID}\nOwner: {nMItem.ownerID}\nLatest Death: {nMItem.latestDeathCount}\nFavourited: {nMItem.nMFavourited}";
                Utils.DrawBorderString(spriteBatch, text, position, Color.White);
            }
            base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
        }

        public override bool CanStackInWorld(Item destination, Item source)
        {
            NMGlobalItem nMDestination = destination.GetGlobalItem<NMGlobalItem>();
            NMGlobalItem nMSource = source.GetGlobalItem<NMGlobalItem>();

            if (nMDestination.ownerID == nMSource.ownerID)
            {
                if (nMDestination.slotID != nMSource.slotID || nMDestination.inventoryType != nMSource.inventoryType)
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

                nMTo.latestDeathCount = nMFrom.latestDeathCount;
                nMTo.slotID = nMFrom.slotID;
                nMTo.inventoryType = nMFrom.inventoryType;
                nMTo.nMFavourited = nMFrom.nMFavourited;
                nMTo.ownerID = nMFrom.ownerID;
            }
            return base.Clone(from, to);
        }

        public override void OnStack(Item destination, Item source, int numToTransfer)
        {
            NMGlobalItem nMDestination = destination.GetGlobalItem<NMGlobalItem>();
            NMGlobalItem nMSource = source.GetGlobalItem<NMGlobalItem>();

            int earliestDeath;
            if (nMSource.latestDeathCount == -1 && nMDestination.latestDeathCount == -1)
            {
                earliestDeath = -1;
            }
            else if (nMSource.latestDeathCount == -1)
            {
                earliestDeath = nMDestination.latestDeathCount;
            }
            else if (nMDestination.latestDeathCount == -1)
            {
                earliestDeath = nMSource.latestDeathCount;
            }
            else
            {
                earliestDeath = Math.Min(nMSource.latestDeathCount, nMDestination.latestDeathCount);
            }
            
            nMDestination.latestDeathCount = earliestDeath;
            nMSource.slotID = nMDestination.slotID;
            nMSource.inventoryType = nMDestination.inventoryType;
            nMSource.nMFavourited = nMDestination.nMFavourited;

            base.OnStack(destination, source, numToTransfer);
        }

        public override void SplitStack(Item destination, Item source, int numToTransfer)
        {
            destination.GetGlobalItem<NMGlobalItem>().latestDeathCount = source.GetGlobalItem<NMGlobalItem>().latestDeathCount;
            base.SplitStack(destination, source, numToTransfer);
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(latestDeathCount);
            writer.Write(ownerID);
            writer.Write(slotID);
            writer.Write((int)inventoryType);
            writer.Write(nMFavourited);

            base.NetSend(item, writer);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            latestDeathCount = reader.ReadInt32();
            ownerID = reader.ReadInt32();
            slotID = reader.ReadInt32();
            inventoryType = (InventoryType)reader.ReadInt32();
            nMFavourited = reader.ReadBoolean();

            base.NetReceive(item, reader);
        }

    }
}
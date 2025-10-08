using System.Collections.Generic;
using System.Numerics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NeatMediumcore;

public class NMSystem : ModSystem
{
    public override void LoadWorldData(TagCompound tag)
    {
        // if (!tag.ContainsKey("NMCItems"))
        // {
        //     base.LoadWorldData(tag);
        //     return;
        // }
        //
        // List<TagCompound> data = tag.GetList<TagCompound>("NMCItems") as List<TagCompound>;
        // if (data == null)
        // {
        //     base.LoadWorldData(tag);
        //     return;
        // }
        //
        // foreach (TagCompound tagCompound in data)
        // {
        //     var deserialize = Item.DESERIALIZER;
        //     Item item = deserialize(tagCompound);
        // }
        
        base.LoadWorldData(tag);
    }

    public override void SaveWorldData(TagCompound tag)
    {
        // var items = Main.ActiveItems;
        // List<TagCompound> itemsData = [];
        // foreach (var item in items)
        // {
        //     var data = item.SerializeData();
        //     itemsData.Add(data);
        // }
        // tag.Add("NMCItems", itemsData);
        
        base.SaveWorldData(tag);
    }
}
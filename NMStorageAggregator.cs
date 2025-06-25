using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NeatMediumcore
{
    [ExtendsFromMod("MagicStorage")]
    public class NMStorageAggregator : MagicStorage.CrossMod.StorageAggregator
    {
        public override bool AppliesToItem(Item item) => true;

        // This makes items stack, but data remains
        public override void SelectGlobalData(GlobalItem item, TagCompound tag)
        {
            tag.Remove("NMLatestDeathCount");
            tag.Remove("NMSlotID");
            tag.Remove("NMOwnerID");
            tag.Remove("NMLoadoutID");
            tag.Remove("NMInventoryType");
            tag.Remove("NMFavourited");
            
            base.SelectGlobalData(item, tag);
        }
    }
}
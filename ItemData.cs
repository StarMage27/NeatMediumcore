using System;
using System.Runtime.InteropServices;

namespace NeatMediumcore;

public struct ItemData(
    uint latestDeathCount = uint.MaxValue,
    ushort ownerId = ushort.MaxValue,
    short slotId = -1,
    byte loadoutId = byte.MaxValue,
    InventoryType inventoryType = InventoryType.None)
{
    public uint latestDeathCount = latestDeathCount;
    public ushort ownerID = ownerId;
    public short slotID = slotId;
    public byte loadoutID = loadoutId;
    public InventoryType inventoryType = inventoryType;
}

public static class ItemDataExtensions
{
    public static bool deathCountIsValid(this ItemData itemData) => itemData.latestDeathCount != uint.MaxValue;
    public static void invalidataDeathCount(this ItemData itemData) => itemData.latestDeathCount = uint.MaxValue;
    public static bool ownerIdIsValid(this ItemData itemData) => itemData.ownerID != ushort.MaxValue;
    public static void invalidataOwnerId(this ItemData itemData) => itemData.ownerID = ushort.MaxValue;
    public static bool isFavorited(this ItemData itemData) => itemData.inventoryType.isInventoryFavorited();
    
    public static byte[] serialize(this ItemData msg)
    {
        int objsize = Marshal.SizeOf(typeof(ItemData));
        byte[] data = new byte[objsize];
        IntPtr buff = Marshal.AllocHGlobal(objsize);
        Marshal.StructureToPtr(msg, buff, true);
        Marshal.Copy(buff, data, 0, objsize);
        Marshal.FreeHGlobal(buff);
        return data;
    }

    public static ItemData deserializeToItemData(this byte[] data)
    {
        int objsize = Marshal.SizeOf(typeof(ItemData));
        IntPtr buff = Marshal.AllocHGlobal(objsize);
        Marshal.Copy(data, 0, buff, objsize);
        ItemData itemData = (ItemData)Marshal.PtrToStructure(buff, typeof(ItemData));
        Marshal.FreeHGlobal(buff);
        return itemData;
    }
}
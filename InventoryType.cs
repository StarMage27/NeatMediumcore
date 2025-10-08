namespace NeatMediumcore;

public enum InventoryType : byte
{
    None = 0,
    Inventory,
    InventoryFavorited,
    Armor,
    MiscEquips,
    Dye,
    MiscDyes,
    WingSlot, ShoeSlot, MoreAccessories, PotionSlots
}

public static class InventoryTypeExtensions
{
    public static bool isInvOrInvFav(this InventoryType type)
    {
        if (type is InventoryType.Inventory or InventoryType.InventoryFavorited)
        {
            return true;
        }
        return false;
    }
    
    public static bool almostEquals(this InventoryType firstType, InventoryType secondType) =>
        firstType == secondType || (firstType.isInvOrInvFav() && secondType.isInvOrInvFav());
    
    public static bool isNone(this InventoryType type) => type == InventoryType.None;
    public static bool isInventory(this InventoryType type) => type == InventoryType.Inventory;
    public static bool isInventoryFavorited(this InventoryType type) => type == InventoryType.InventoryFavorited;
    public static bool isArmor(this InventoryType type) => type is InventoryType.Armor;
    public static bool isMiscEquips(this InventoryType type) => type is InventoryType.MiscEquips;
    public static bool isDye(this InventoryType type) => type is InventoryType.Dye;
    public static bool isMiscDyes(this InventoryType type) => type is InventoryType.MiscDyes;
    
    // These are not yet supported (and likely won't be supported)
    public static bool isWingSlot(this InventoryType type) => type is InventoryType.WingSlot;
    public static bool isShoeSlot(this InventoryType type) => type is InventoryType.ShoeSlot;
    public static bool isMoreAccessories(this InventoryType type) => type is InventoryType.MoreAccessories;
    public static bool isPotionSlots(this InventoryType type) => type is InventoryType.PotionSlots;
}
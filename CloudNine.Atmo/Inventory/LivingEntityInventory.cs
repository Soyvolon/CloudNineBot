using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Economy;
using CloudNine.Atmo.Items;
using CloudNine.Atmo.Items.Modifiers;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Inventory
{
    public class LivingEntityInventory : InventoryBase
    {
        #region Equipment Slots
        [JsonProperty("left_hand")]
        public Weapon? LeftHand { get; private set; }
        [JsonProperty("right_hand")]
        public Weapon? RightHand { get; private set; }


        [JsonProperty("left_ear")]
        public Accessory? LeftEaring { get; private set; }
        [JsonProperty("right_ear")]
        public Accessory? RightEaring { get; private set; }
        [JsonProperty("necklace")]
        public Accessory? Necklace { get; private set; }
        [JsonProperty("left_ring")]
        public Accessory? LeftRing { get; private set; }
        [JsonProperty("right_ring")]
        public Accessory? RightRing { get; private set; }


        [JsonProperty("head")]
        public Armor? HeadArmor { get; private set; }
        [JsonProperty("chest")]
        public Armor? ChestArmor { get; private set; }
        [JsonProperty("gloves")]
        public Armor? GloveArmor { get; private set; }
        [JsonProperty("legs")]
        public Armor? LegArmor { get; private set; }
        [JsonProperty("feet")]
        public Armor? FeetArmor { get; private set; }
        #endregion

        [JsonProperty("bank")]
        public BaseBank Bank { get; internal set; }

        public LivingEntityInventory() : base()
        {
            Bank = new();
        }

        #region Equipment Manegment Methods
        public bool EquipItem(ItemBase item, out ItemBase? replacedItem, [Range(-1,1)] int slot = -1)
        {
            if(Items.Contains(item))
            {
                switch(item)
                {
                    case Weapon w:
                        if (slot == -1)
                        {
                            replacedItem = null;
                            return false;
                        }
                        else
                        {
                            return EquipWeapon(w, slot, out replacedItem);
                        }
                    case Accessory a:
                        return EquipAccessory(a, slot, out replacedItem);
                    case Armor a:
                        return EquipArmor(a, out replacedItem);
                }
            }

            replacedItem = null;
            return false;
        }

        private bool EquipWeapon(Weapon item, [Range(0, 1)] int slot, out ItemBase? replacedItem)
        {
            // TODO: More checks for specific weapon types
            switch (slot)
            {
                case 0: // Primary Weapon Slot
                    if (ReferenceEquals(LeftHand, item))
                    {
                        // Weapons are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(RightHand, item))
                    {
                        // Weapon is in the other slot. Switch slots.
                        RightHand = LeftHand;
                        LeftHand = item;
                        // Nothing was changed in the inventory, so retun null.
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Weapon is new. Repalce old Weapon.
                        replacedItem = LeftHand;
                        LeftHand = item;
                        return true;
                    }
                case 1: // Secondary Weapon Slot
                    if (ReferenceEquals(RightHand, item))
                    {
                        // Weapons are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(LeftHand, item))
                    {
                        // Weapon is in the other slot. Switch slots.
                        LeftHand = RightHand;
                        RightHand = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Weapon is new. Repalce old Weapon.
                        replacedItem = RightHand;
                        RightHand = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        protected bool EquipAccessory(Accessory item, [Range(-1,1)] int slot, out ItemBase? replacedItem)
        {
            switch(item.Type)
            {
                case Accessory.AccessoryType.Earing:
                    if (slot == -1)
                    {
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        return EquipEaring(item, slot, out replacedItem);
                    }
                case Accessory.AccessoryType.Ring:
                    if (slot == -1)
                    {
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        return EquipRing(item, slot, out replacedItem);
                    }
                case Accessory.AccessoryType.Necklace:
                    return EquipNecklace(item, out replacedItem);
            }

            replacedItem = null;
            return false;
        }

        protected bool EquipEaring(Accessory item, [Range(0,1)] int slot, out ItemBase? replacedItem)
        {
            switch (slot)
            {
                case 0: // Ring Slot 1
                    if (ReferenceEquals(LeftEaring, item))
                    {
                        // Rings are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(RightEaring, item))
                    {
                        // Ring is in the other slot. Switch slots.
                        RightEaring = LeftEaring;
                        LeftEaring = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Ring is new. Repalce old ring.
                        replacedItem = LeftEaring;
                        LeftEaring = item;
                        return true;
                    }
                case 1: // Ring Slot 2
                    if (ReferenceEquals(RightEaring, item))
                    {
                        // Rings are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(LeftEaring, item))
                    {
                        // Ring is in the other slot. Switch slots.
                        LeftEaring = RightEaring;
                        RightEaring = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Ring is new. Replace old ring.
                        replacedItem = RightEaring;
                        RightEaring = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        protected bool EquipRing(Accessory item, [Range(0,1)] int slot, out ItemBase? replacedItem)
        {
            switch (slot)
            {
                case 0: // Ring Slot 1
                    if (ReferenceEquals(LeftRing, item))
                    {
                        // Rings are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(RightRing, item))
                    {
                        // Ring is in the other slot. Switch slots.
                        RightRing = LeftRing;
                        LeftRing = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Ring is new. Repalce old ring.
                        replacedItem = LeftRing;
                        LeftRing = item;
                        return true;
                    }
                case 1: // Ring Slot 2
                    if (ReferenceEquals(RightRing, item))
                    {
                        // Rings are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(LeftRing, item))
                    {
                        // Ring is in the other slot. Switch slots.
                        LeftRing = RightRing;
                        RightRing = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Ring is new. Replace old ring.
                        replacedItem = RightRing;
                        RightRing = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        protected bool EquipNecklace(Accessory item, out ItemBase? replacedItem)
        {
            if (ReferenceEquals(Necklace, item))
            {
                // Necklace is the same. Fail.
                replacedItem = null;
                return false;
            }

            // Necklace is new. Replace old necklace
            replacedItem = Necklace;
            Necklace = item;
            return true;
        }

        protected bool EquipArmor(Armor item, out ItemBase? replacedItem)
        {
            switch (item.Type)
            {
                case Armor.ArmorType.Head:
                    if (ReferenceEquals(HeadArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = HeadArmor;
                        HeadArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Glove:
                    if (ReferenceEquals(GloveArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = GloveArmor;
                        GloveArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Chest:
                    if (ReferenceEquals(ChestArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = ChestArmor;
                        ChestArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Leg:
                    if (ReferenceEquals(LegArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = LegArmor;
                        LegArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Feet:
                    if (ReferenceEquals(FeetArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = FeetArmor;
                        FeetArmor = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        #endregion

        #region Statistical Calculation Methods
        internal int GetTotalBaseArmorValue()
        {
            int armor = 0;

            armor += HeadArmor?.BaseArmor ?? 0;
            armor += ChestArmor?.BaseArmor ?? 0;
            armor += GloveArmor?.BaseArmor ?? 0;
            armor += LegArmor?.BaseArmor ?? 0;
            armor += FeetArmor?.BaseArmor ?? 0;

            return armor;
        }

        internal Dictionary<DamageModifiers, int> GetBaseArmorModifierse()
        {
            Dictionary<DamageModifiers, int> mods = HeadArmor?.ArmorModifiers ?? new();

            if (ChestArmor is not null)
                AddToModifiers(ChestArmor, ref mods);
            if (GloveArmor is not null)
                AddToModifiers(GloveArmor, ref mods);
            if (LegArmor is not null)
                AddToModifiers(LegArmor, ref mods);
            if (FeetArmor is not null)
                AddToModifiers(FeetArmor, ref mods);

            return mods;
        }

        private static void AddToModifiers(Armor armor, ref Dictionary<DamageModifiers, int> mods)
        {
            foreach(var mod in armor.ArmorModifiers)
            {
                if (mods.ContainsKey(mod.Key))
                    mods[mod.Key] += mod.Value;
                else mods[mod.Key] = mod.Value;
            }
        }

        internal Dictionary<DamageModifiers, int> GetBaseAttackModifiers()
        {
            Dictionary<DamageModifiers, int> mods = LeftHand?.WeaponDamageModifers ?? new();

            if (RightHand is not null)
            {
                foreach (var mod in RightHand.WeaponDamageModifers)
                {
                    if (mods.ContainsKey(mod.Key))
                        mods[mod.Key] += mod.Value;
                    else mods[mod.Key] = mod.Value;
                }
            }

            return mods;
        }

        internal Dictionary<PlayerModifiers, int> GetBasePlayerModifiers()
        {
            Dictionary<PlayerModifiers, int> mods = LeftEaring?.Modifiers ?? new();

            if (RightEaring is not null)
                AddToModifiers(RightEaring, ref mods);
            if (Necklace is not null)
                AddToModifiers(Necklace, ref mods);
            if (LeftRing is not null)
                AddToModifiers(LeftRing, ref mods);
            if (RightRing is not null)
                AddToModifiers(RightRing, ref mods);

            return mods;
        }

        private static void AddToModifiers(Accessory accessory, ref Dictionary<PlayerModifiers, int> mods)
        {
            foreach (var mod in accessory.Modifiers)
            {
                if (mods.ContainsKey(mod.Key))
                    mods[mod.Key] += mod.Value;
                else mods[mod.Key] = mod.Value;
            }
        }
        #endregion
    }
}

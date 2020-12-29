using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using CloudNine.Atmo.Items.Utility;
using CloudNine.Atmo.Items.Modifiers;

namespace CloudNine.Atmo.Items
{
    public abstract class ItemBase
    {
        /// <summary>
        /// Item id, used to idetify items that already have preset values
        /// </summary>
        public long ItemId { get; private set; }
        /// <summary>
        /// Name of the item, free to be able to change.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The rarity of the item
        /// </summary>
        public Rarity ItemRarity { get; set; }

        /// <summary>
        /// Create a new instance of an Item
        /// </summary>
        /// <param name="id">Item ID of the new item</param>
        public ItemBase(long id)
        {
            ItemId = id;
        }

        public ItemBase(string name)
        {
            Name = name;
        }

        [JsonConstructor]
        public ItemBase(long itemId, string name, Rarity rarity) : this(itemId)
        {
            Name = name;
            ItemRarity = rarity;
        }

        /// <summary>
        /// Set the name of the Item
        /// </summary>
        /// <param name="name">New Name</param>
        public void SetItemName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Converts the Item into a JSON string for in storage
        /// </summary>
        /// <returns>Serialized Object of this object</returns>
        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(ItemBase item)
        {
            Name = item.Name;

            // Always returns true unless someting fails
            return true;
        }

        public void SetItemId(long v)
        {
            ItemId = v;
        }

        /// <summary>
        /// Gets the shorthand modifier string for a Player Modification
        /// </summary>
        /// <param name="mod">Player Mod</param>
        /// <param name="val">Value of the player mod</param>
        /// <returns>Shorthand string</returns>
        public static string GetModifierShorthandString(PlayerModifiers mod, int val)
        {
            switch (mod)
            {
                case PlayerModifiers.Health:
                    return $"HP: {(val >= 0 ? "" : "-")}{val}";
                case PlayerModifiers.Magic:
                    return $"MP: {(val >= 0 ? "" : "-")}{val}";
                case PlayerModifiers.Speed:
                    return $"SP: {(val >= 0 ? "" : "-")}{val}";
            }

            return "";
        }

        /// <summary>
        /// Gets the shorthand modifier string for a Damage Modifier
        /// </summary>
        /// <param name="mod">Damage Mod</param>
        /// <param name="val">Value of the damage mod</param>
        /// <returns>Shorthand string</returns>
        public static string GetModifierShorthandString(DamageModifiers mod, int val)
        {
            switch (mod)
            {
                case DamageModifiers.Pierce:
                    return $"PIR: {(val >= 0 ? "" : "-")}{val}";
                case DamageModifiers.Slash:
                    return $"SLS: {(val >= 0 ? "" : "-")}{val}";
                case DamageModifiers.Blunt:
                    return $"BLT: {(val >= 0 ? "" : "-")}{val}";
                case DamageModifiers.Burn:
                    return $"BRN: {(val >= 0 ? "" : "-")}{val}";
                case DamageModifiers.Freeze:
                    return $"FRZ: {(val >= 0 ? "" : "-")}{val}";
                case DamageModifiers.Magic:
                    return $"DMP: {(val >= 0 ? "" : "-")}{val}";
            }

            return "";
        }

        /// <summary>
        /// Gets the shorthand modifier string for a Consumable Modifier
        /// </summary>
        /// <param name="mod">Consumable mod</param>
        /// <param name="val">Value of the consumable mod</param>
        /// <returns>Shorthand string</returns>
        public static string GetModifierShorthandString(ConsumableModifiers mod, int val)
        {
            switch (mod)
            {
                case ConsumableModifiers.Health:
                    return $"HP: {(val >= 0 ? "" : "-")}{val}";
                case ConsumableModifiers.Speed:
                    return $"SP: {(val >= 0 ? "" : "-")}{val}";
                case ConsumableModifiers.Damage:
                    return $"DMG: {(val >= 0 ? "" : "-")}{val}";
                case ConsumableModifiers.Magic:
                    return $"MP: {(val >= 0 ? "" : "-")}{val}";
                case ConsumableModifiers.Armor:
                    return $"ARM: {(val >= 0 ? "" : "-")}{val}";
            }

            return "";
        }
    }
}

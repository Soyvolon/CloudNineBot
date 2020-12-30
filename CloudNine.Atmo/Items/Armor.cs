using System;
using System.Collections.Generic;
using System.Text;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Items.Utility;
using Newtonsoft.Json;

namespace CloudNine.Atmo.Items
{
    public class Armor : ItemBase
    {
        public enum ArmorType
        {
            Head,
            Chest,
            Glove,
            Leg,
            Feet
        }
        [JsonProperty("armor_type")]
        public ArmorType Type { get; internal set; }
        [JsonProperty("base_armor")]
        public int BaseArmor { get; internal set; }
        [JsonProperty("durability")]
        public int Durability { get; internal set; }
        [JsonProperty("armor_modifiers")]
        public Dictionary<DamageModifiers, int> ArmorModifiers { get; internal set; }
        public Armor(int id) : base(id)
        {
            ArmorModifiers = new Dictionary<DamageModifiers, int>();

        }

        public Armor(string name) : base(name)
        {
            ArmorModifiers = new Dictionary<DamageModifiers, int>();
        }

        public Armor(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        public Armor(long ItemId, string Name, ArmorType Type, int BaseArmor, int Durability, Dictionary<DamageModifiers, int> ArmorModifiers, Rarity rarity) : base(ItemId, Name, rarity)
        {
            this.Type = Type;
            this.BaseArmor = BaseArmor;
            this.Durability = Durability;
            this.ArmorModifiers = ArmorModifiers;
            if (this.ArmorModifiers == null)
            {
                this.ArmorModifiers = new Dictionary<DamageModifiers, int>();
            }
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(Armor item)
        {
            Type = item.Type;
            BaseArmor = item.BaseArmor;
            Durability = item.Durability;
            ArmorModifiers = item.ArmorModifiers;

            return base.AssignDefaultVars(item);
        }
    }
}

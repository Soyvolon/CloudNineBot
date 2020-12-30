using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Items.Utility;

namespace CloudNine.Atmo.Items
{
    public class Weapon : ItemBase
    {
        /// <summary>
        /// How many uses the item has
        /// </summary>
        [JsonProperty("durability")]
        public int Durability { get; set; }
        /// <summary>
        /// Damage that the item does as a minimum
        /// </summary>
        [JsonProperty("base_damage")]
        public int BaseDamage { get; set; }
        /// <summary>
        /// Holds values for any damage modifiers the weapon has.
        /// </summary>
        [JsonProperty("weapon_modifiers")]
        public Dictionary<DamageModifiers, int> WeaponDamageModifers { get; set; }

        public Weapon() : base() { }

        public Weapon(ItemType type, string id, string name = "") : base(type, id, name)
        {
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        public Weapon(ItemType type, string name, string id, Rarity rarity) 
            : base(type, id, name, rarity) 
        {
            WeaponDamageModifers = new();
        }

        
        public Weapon(ItemType type, string itemId, string name, int durability, int baseDamage, Dictionary<DamageModifiers, int> weaponDamageModifers, Rarity rarity) 
            : base(type, itemId, name, rarity)
        {
            Durability = durability;
            BaseDamage = baseDamage;
            WeaponDamageModifers = weaponDamageModifers;
            if(WeaponDamageModifers == null)
            {
                WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
            }
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(Weapon item)
        {
            Durability = item.Durability;
            BaseDamage = item.BaseDamage;
            WeaponDamageModifers = item.WeaponDamageModifers;

            return base.AssignDefaultVars(item);
        }
    }
}

﻿using Newtonsoft.Json;
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
        public int Durability { get; set; }
        /// <summary>
        /// Damage that the item does as a minimum
        /// </summary>
        public int BaseDamage { get; set; }
        /// <summary>
        /// Holds values for any damage modifiers the weapon has.
        /// </summary>
        public Dictionary<DamageModifiers, int> WeaponDamageModifers { get; set; }

        public Weapon(long id) : base(id)
        {
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        public Weapon(string name) : base(name)
        {
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        public Weapon(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Weapon(long itemId, string name, int durability, int baseDamage, Dictionary<DamageModifiers, int> weaponDamageModifers, Rarity rarity) : base(itemId, name, rarity)
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

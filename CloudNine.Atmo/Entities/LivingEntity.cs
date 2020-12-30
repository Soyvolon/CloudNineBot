using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Inventory;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Loaders;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Entities
{
    public class LivingEntity : ILoadable<LivingEntity>
    {
        [JsonProperty("internal_id")]
        internal string InternalId { get; set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("inventory")]
        public LivingEntityInventory Inventory { get; internal set; }

        [JsonProperty("base_hp")]
        public int BaseHealth { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public int CalculatedHealth { get; private set; }
        [JsonProperty("current_hp")]
        public int CurrentHealth { get; private set; }

        [JsonProperty("base_magic")]
        public int BaseMagic { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public int CalculatedMagic { get; private set; }
        [JsonProperty("current_magic")]
        public int CurrentMagic { get; private set; }

        [JsonProperty("base_speed")]
        public int BaseSpeed { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public int CalculatedSpeed { get; private set; }
        [JsonProperty("current_speed")]
        public int CurrentSpeed { get; private set; }

        [JsonIgnore]
        [NotMapped]
        public Dictionary<PlayerModifiers, int> PlayerModifiers { get; private set; }

        [JsonIgnore]
        [NotMapped]
        public int Armor { get; private set; }
        [JsonIgnore]
        [NotMapped]
        public Dictionary<DamageModifiers, int> ArmorModifiers { get; private set; }

        [JsonIgnore]
        [NotMapped]
        public int AttackPower { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public Dictionary<DamageModifiers, int> AttackModifiers { get; private set; }

        [JsonProperty("level")]
        public int Level { get; private set; }
        
        [JsonProperty("is_fainted")]
        public bool Fainted { get; private set; }

        [JsonIgnore]
        [NotMapped]
        protected double noMissModifier = 0.0;

        public delegate void OnDeath(LivingEntity le);
        [JsonIgnore]
        [NotMapped]
        public OnDeath OnFaint { get; set; }

        [JsonProperty("entity_type")]
        public EntityType Type { get; internal set; }

        public LivingEntity() : this("") { }

        public LivingEntity(string name)
        {
            Name = name;
            
            BaseHealth = 100;
            BaseMagic = 100;
            BaseSpeed = 10;

            Inventory = new();

            AttackModifiers = new();
            ArmorModifiers = new();
            PlayerModifiers = new();
        }

        public bool LoadDefaultVars(LivingEntity item)
        {
            InternalId = item.InternalId;
            Name = item.Name;

            BaseHealth = item.BaseHealth;
            BaseMagic = item.BaseMagic;
            BaseSpeed = item.BaseSpeed;

            Level = item.Level;
            Fainted = item.Fainted;
            Type = item.Type;

            RecalculateEntitiyStatistics();
            ResetCurrentEntitiyStatistics();

            return true;
        }

        /// <summary>
        /// Used to recalculate the Armor, Max HP, Max Magic, Max Speed, and Attack Power from equiped items.
        /// </summary>
        internal void RecalculateEntitiyStatistics()
        {
            Armor = Inventory.GetTotalBaseArmorValue();


        }

        /// <summary>
        /// Used to reset the Current HP, Current Magic, Current Speed, and no hit modifiers to their base values.
        /// </summary>
        internal void ResetCurrentEntitiyStatistics()
        {
            noMissModifier = 0.0f;

            CurrentHealth = CalculatedHealth;
            CurrentSpeed = CalculatedSpeed;
            CurrentMagic = CalculatedMagic;
        }
    }
}

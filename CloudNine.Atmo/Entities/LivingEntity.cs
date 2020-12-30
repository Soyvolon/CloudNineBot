using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Inventory;
using CloudNine.Atmo.Items.Modifiers;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Entities
{
    public class LivingEntity
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("inventory")]
        public LivingEntityInventory Inventory { get; internal set; }

        [JsonProperty("base_hp")]
        public int BaseHealth { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public int MaxHealth { get; private set; }
        [JsonProperty("current_hp")]
        public int CurrentHealth { get; private set; }

        [JsonProperty("base_magic")]
        public int BaseMagic { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public int MaxMagic { get; private set; }
        [JsonProperty("current_magic")]
        public int CurrentMagic { get; private set; }

        [JsonProperty("base_speed")]
        public int BaseSpeed { get; internal set; }
        [JsonIgnore]
        [NotMapped]
        public int MaxSpeed { get; private set; }
        [JsonProperty("current_speed")]
        public int CurrentSpeed { get; private set; }

        [JsonIgnore]
        [NotMapped]
        public int Armor { get; private set; }
        [JsonIgnore]
        [NotMapped]
        public Dictionary<DamageModifiers, int> ArmorModifiers { get; init; }

        [JsonIgnore]
        [NotMapped]
        public int AttackPower { get; private set; }
        [JsonIgnore]
        [NotMapped]
        public Dictionary<DamageModifiers, int> AttackModifiers { get; init; }

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
        }
    }
}

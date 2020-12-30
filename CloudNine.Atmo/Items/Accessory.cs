using System;
using System.Collections.Generic;
using System.Text;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Items.Utility;
using Newtonsoft.Json;

namespace CloudNine.Atmo.Items
{
    public class Accessory : ItemBase
    {
        public enum AccessoryType
        {
            Earing,
            Necklace,
            Ring
        }

        [JsonProperty("modifiers")]
        public Dictionary<PlayerModifiers, int> Modifiers { get; internal set; }
        [JsonProperty("accessory_type")]
        public AccessoryType Type { get; internal set; }

        public Accessory(long id) : base(id)
        {
            Modifiers = new();
        }

        public Accessory(string name) : base(name)
        {
            Modifiers = new();
        }

        public Accessory(long id, string name, Rarity raity) : base(id, name, raity)
        {
            Modifiers = new();
        }

        public Accessory(long ItemId, string Name, AccessoryType Type, Dictionary<PlayerModifiers, int> Modifiers, Rarity Rarity)
            : base(ItemId, Name, Rarity)
        {
            this.Modifiers = Modifiers;
            this.Type = Type;
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(Accessory item)
        {
            Modifiers = item.Modifiers;

            return base.AssignDefaultVars(item);
        }
    }
}

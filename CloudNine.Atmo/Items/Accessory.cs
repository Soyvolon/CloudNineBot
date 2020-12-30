using System;
using System.Collections.Generic;
using System.Text;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Items.Utility;
using CloudNine.Atmo.Loaders;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Items
{
    public class Accessory : ItemBase, ILoadable<Accessory>
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

        public Accessory() : base() { }

        public Accessory(ItemType type, string id, string name = "") : base(type, id, name)
        {
            Modifiers = new();
        }

        public Accessory(ItemType type, string id, string name, Rarity raity) : base(type, id, name, raity)
        {
            Modifiers = new();
        }

        public Accessory(ItemType type, string ItemId, string Name, AccessoryType Type, Dictionary<PlayerModifiers, int> Modifiers, Rarity Rarity)
            : base(type, ItemId, Name, Rarity)
        {
            this.Modifiers = Modifiers;
            this.Type = Type;
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        public bool LoadDefaultVars(Accessory item)
        {
            Modifiers = item.Modifiers;
            Type = item.Type;

            return base.LoadDefaultVars(item);
        }
    }
}

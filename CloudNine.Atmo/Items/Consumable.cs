using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Items.Utility;
using CloudNine.Atmo.Loaders;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Items
{
    public class Consumable : ItemBase, ILoadable<Consumable>
    {
        [JsonProperty("consumeable_modifiers")]
        public Dictionary<ConsumableModifiers, int> Modifiers { get; private set; }

        public Consumable() : base() { }
        
        public Consumable(ItemType type, string id, string name = "") : base(type, id, name)
        {
            Modifiers = new Dictionary<ConsumableModifiers, int>();
        }

        public Consumable(ItemType type, string name, string id, Rarity rarity) : base(type, id, name, rarity) 
        {
            Modifiers = new();
        }

        
        public Consumable(ItemType type, string itemId, string name, Dictionary<ConsumableModifiers, int> modifiers, Rarity rarity) 
            : base(type, itemId, name, rarity)
        {
            Modifiers = modifiers;
            if (this.Modifiers == null)
            {
                this.Modifiers = new Dictionary<ConsumableModifiers, int>();
            }
        }

        public string GetInfoString()
        {
            string output = "";
            ConsumableModifiers[] Modkeys = Modifiers.Keys.ToArray();

            for(int i = 0; i < Modkeys.Count(); i++)
            {
                output += GetModifierShorthandString(Modkeys[i], Modifiers[Modkeys[i]]);

                if(i != Modkeys.Count() - 1)
                {
                    output += " | ";
                }
            }

            return output;
        }

        public bool LoadDefaultVars(Consumable item)
        {
            Modifiers = item.Modifiers;

            return base.LoadDefaultVars(item);
        }
    }
}

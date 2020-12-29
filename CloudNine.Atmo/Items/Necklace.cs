using System;
using System.Collections.Generic;
using System.Text;
using CloudNine.Atmo.Items.Modifiers;
using CloudNine.Atmo.Items.Utility;
using Newtonsoft.Json;

namespace CloudNine.Atmo.Items
{
    public class Necklace : ItemBase
    {
        public Dictionary<PlayerModifiers, int> Modifiers { get; set; }
        public Necklace(int id) : base(id)
        {
            Modifiers = new Dictionary<PlayerModifiers, int>();
        }

        public Necklace(string name) : base(name)
        {
            Modifiers = new Dictionary<PlayerModifiers, int>();
        }

        public Necklace(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Necklace(long itemId, string name, Dictionary<PlayerModifiers, int> modifiers, Rarity rarity) : base(itemId, name, rarity)
        {
            Modifiers = modifiers;
            if(Modifiers == null)
            {
                Modifiers = new Dictionary<PlayerModifiers, int>();
            }
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(Necklace item)
        {
            Modifiers = item.Modifiers;

            return base.AssignDefaultVars(item);
        }
    }
}

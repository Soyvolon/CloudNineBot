using CloudNine.Atmo.Items.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudNine.Atmo.Items
{
    public class Resource : ItemBase
    {
        /// <summary>
        /// Types of Resources
        /// </summary>
        public enum Type
        {
            Crafting,
            Misc
        }

        /// <summary>
        /// Value of the resource before supply and demand
        /// </summary>
        [JsonProperty("value")]
        public int Value { get; set; }
        public Resource(long id) : base(id)
        {

        }

        public Resource(string name) : base(name) { }

        public Resource(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        public Resource(long itemId, string name, Rarity rarity) : base(itemId, name, rarity) { }


        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(Resource item)
        {
            Value = item.Value;

            return base.AssignDefaultVars(item);
        }
    }
}

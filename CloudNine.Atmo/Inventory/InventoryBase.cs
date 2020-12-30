using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Items;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Inventory
{
    public abstract class InventoryBase
    {
        [JsonProperty("items")]
        public List<ItemBase> Items { get; init; }

        public InventoryBase()
        {
            Items = new();
        }

        public void AddItem(ItemBase item)
        {
            Items.Add(item);
        }

        public bool RemoveItem(ItemBase item)
        {
            return Items.Remove(item);
        }
    }
}

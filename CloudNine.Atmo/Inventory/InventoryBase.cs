using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Items;
using CloudNine.Atmo.Items;

namespace CloudNine.Atmo.Inventory
{
    public abstract class InventoryBase
    {
        public List<ItemBase> Items { get; init; }

        public InventoryBase()
        {
            Items = new();
        }
    }
}

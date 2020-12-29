using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Inventory;

namespace CloudNine.Atmo.Entities
{
    public class LivingEntity
    {
        public int BaseHealth { get; internal set; }
        public int ActualHealth { get; internal set; }

        public LivingEntityInventory Inventory { get; internal set; }
    }
}

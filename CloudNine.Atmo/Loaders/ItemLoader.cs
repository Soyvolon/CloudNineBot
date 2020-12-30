using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CloudNine.Atmo.Items;
using CloudNine.Atmo.Items.Utility;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudNine.Atmo.Loaders
{
    internal class ItemLoader : BaseLoader
    {
        internal string ItemPath { get; init; }

        internal Dictionary<string, ItemBase> Items { get; init; }
        private ILogger Logger { get; init; }

        public ItemLoader(ILogger<ItemLoader> logger) : base()
        {
            this.Logger = logger;
            ItemPath = Path.Join(BasePath, "Items");

            if (!Directory.Exists(ItemPath))
                Directory.CreateDirectory(ItemPath);

            Items = new();
        }

        public async Task InitalizeAsync()
        {
            var toLoad = Directory.GetFiles(ItemPath, "*.json");
            if(toLoad is null)
            {
                Logger.LogCritical("No items found to load.");
                return;
            }

            foreach(var file in toLoad)
            {
                using FileStream fs = new(file, FileMode.Open);
                using StreamReader sr = new(fs);
                var json = await sr.ReadToEndAsync();

                var jobject = JObject.Parse(json);

                var id = jobject["item_id"]!.ToString();

                if (Items.ContainsKey(id))
                    throw new DuplicateItemIdException($"Item ID {id} has already been loaded.");

                var type = jobject["item_type"]!.ToObject<ItemType>();

                ItemBase? item;
                switch(type)
                {
                    case ItemType.Accessory:
                        item = JsonConvert.DeserializeObject<Accessory>(json);
                        break;
                    case ItemType.Armor:
                        item = JsonConvert.DeserializeObject<Armor>(json);
                        break;
                    case ItemType.Consumable:
                        item = JsonConvert.DeserializeObject<Consumable>(json);
                        break;
                    case ItemType.Resource:
                        item = JsonConvert.DeserializeObject<Resource>(json);
                        break;
                    case ItemType.Weapon:
                        item = JsonConvert.DeserializeObject<Weapon>(json);
                        break;
                    default:
                        item = null;
                        break;
                }

                if(item is not null)
                {
                    Items[item.ItemId] = item;
                }
                else
                {
                    Logger.LogWarning("Item data had an invalid type.");
                }
            }
        }

        internal class DuplicateItemIdException : Exception
        {
            public DuplicateItemIdException() : base() { }
            public DuplicateItemIdException(string? message) : base(message) { }
        }

        /// <summary>
        /// Retrives a copy of a base item.
        /// </summary>
        /// <typeparam name="T">Type of item to retrive.</typeparam>
        /// <param name="itemId">ID of the item to retrive.</param>
        /// <param name="item">The item retrived.</param>
        /// <returns>If the item was retrived.</returns>
        public bool GetItem<T>(string itemId, out T? item) where T : ItemBase, new()
        {
            if(Items.TryGetValue(itemId, out var baseItem))
            {
                T? baseI = baseItem as T;

                if(baseI is not null)
                {
                    item = new();
                    item.AssignDefaultVars(baseI);
                    return true;
                }
            }

            item = default;
            return false;
        }
    }
}

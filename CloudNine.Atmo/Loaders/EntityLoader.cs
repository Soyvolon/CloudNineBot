using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Entities;
using CloudNine.Atmo.Entities.Hostile;
using CloudNine.Atmo.Entities.NPC;
using CloudNine.Atmo.Entities.Passive;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudNine.Atmo.Loaders
{
    internal class EntityLoader : BaseLoader
    {
        internal string EntityPath { get; init; }
        internal Dictionary<string, LivingEntity> Entities { get; init; }
        private ILogger Logger { get; init; }

        public EntityLoader(ILogger<EntityLoader> logger) : base()
        {
            EntityPath = Path.Join(BasePath, "Encounters");

            if (!Directory.Exists(EntityPath))
                Directory.CreateDirectory(EntityPath);

            Logger = logger;
            Entities = new();
        }

        public async Task InitalizeAsync()
        {
            var toLoad = Directory.GetFiles(EntityPath, "*.json");
            if (toLoad is null)
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

                if (Entities.ContainsKey(id))
                    throw new DuplicateItemIdException($"Item ID {id} has already been loaded.");

                var type = jobject["entity_type"]!.ToObject<EntityType>();

                LivingEntity? entity;
                switch(type)
                {
                    case EntityType.Hostile:
                        entity = JsonConvert.DeserializeObject<HostileEntity>(json);
                        break;
                    case EntityType.NPC:
                        entity = JsonConvert.DeserializeObject<NPCEntitiy>(json);
                        break;
                    case EntityType.Player:
                        entity = null; // not a valid type to parse;
                        break;
                    case EntityType.Passive:
                        entity = JsonConvert.DeserializeObject<PassiveEntitiy>(json);
                        break;
                    default:
                        entity = null;
                        break;
                }

                if(entity is not null)
                {
                    Entities[entity.InternalId] = entity;
                }
                else
                {
                    Logger.LogWarning($"Failed to properly parse a JSON file: {file}");
                }
            }
        }

        /// <summary>
        /// Retrives a copy of a base entitiy.
        /// </summary>
        /// <typeparam name="T">Type of entitiy to retrive.</typeparam>
        /// <param name="itemId">ID of the entitiy to retrive.</param>
        /// <param name="entitity">The entitiy retrived.</param>
        /// <returns>If the entitiy was retrived.</returns>
        public bool GetItem<T>(string itemId, out T? entitity) where T : LivingEntity, ILoadable<T>, new()
        {
            if (Entities.TryGetValue(itemId, out var baseItem))
            {
                T? baseI = baseItem as T;

                if (baseI is not null)
                {
                    entitity = new();
                    entitity.LoadDefaultVars(baseI);
                    return true;
                }
            }

            entitity = default;
            return false;
        }

        public bool RegisterOrUpdateEntity(LivingEntity entitiy)
        {
            try
            {
                var json = JsonConvert.SerializeObject(entitiy, Formatting.Indented);

                File.WriteAllText(Path.Join(EntityPath, $"{entitiy.InternalId}.json"), json);

                Entities[entitiy.InternalId] = entitiy;

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save item");
                return false;
            }
        }

        public async Task<bool> RegisterOrUpdateEntitiyAsync(LivingEntity entitiy)
        {
            try
            {
                var json = JsonConvert.SerializeObject(entitiy, Formatting.Indented);

                await File.WriteAllTextAsync(Path.Join(EntityPath, $"{entitiy.InternalId}.json"), json);

                Entities[entitiy.InternalId] = entitiy;

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save item");
                return false;
            }
        }
    }
}

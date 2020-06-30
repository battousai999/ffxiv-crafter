using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ffxiv_crafter.Serialization
{
    public static class SaveDataJsonAdapter
    {
        public class SaveDataResults
        {
            public List<CraftingItem> DefinedCraftingItems { get; set; }
            public List<CraftingMaterial> DefinedCraftingMaterials { get; set; }
            public List<SpecifiedCraftingItem> CraftingItems { get; set; }
        }

        public static string ToJson(List<CraftingItem> definedCraftingItems, List<CraftingMaterial> definedMaterialItems, List<SpecifiedCraftingItem> craftingItems)
        {
            var itemGuids = definedCraftingItems.ToDictionary(x => x.Name, _ => Guid.NewGuid(), StringComparer.OrdinalIgnoreCase);
            var materialGuids = definedMaterialItems.ToDictionary(x => x.Name, _ => Guid.NewGuid(), StringComparer.OrdinalIgnoreCase);

            var saveData = new SaveData();

            Func<ICraftingMaterial, Guid> lookupMaterialId = item =>
            {
                if (item is CraftingMaterial)
                    return materialGuids[item.Name];
                else
                    return itemGuids[item.Name];
            };

            saveData.AllCraftingMaterials = definedMaterialItems
                .Select(item =>
                {
                    return new CraftingMaterialData
                    {
                        Id = materialGuids[item.Name],
                        Name = item.Name,
                        SourceType = item.SourceType,
                        Location = item.Location
                    };
                })
                .ToList();

            saveData.AllCraftingItems = definedCraftingItems
                .Select(item =>
                {
                    return new CraftingItemData
                    {
                        Id = itemGuids[item.Name],
                        Name = item.Name,
                        SourceType = item.SourceType,
                        Materials = item.Materials
                            .Select(x =>
                            {
                                return new CountedCraftingItemData
                                {
                                    ItemId = lookupMaterialId(x.Material),
                                    Count = x.Count,
                                    IsMaterial = x.Material is CraftingMaterial
                                };
                            })
                            .ToList()
                    };
                })
                .ToList();

            saveData.ListedItems = craftingItems
                .Select(x => new CountedCraftingItemData { ItemId = itemGuids[x.Name], Count = x.Count, IsMaterial = false })
                .ToList();

            return JsonConvert.SerializeObject(saveData);
        }

        public static SaveDataResults FromJson(string jsonString)
        {
            var saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);

            var materialsList = saveData.AllCraftingMaterials
                .Select(x =>
                {
                    return new CraftingMaterial
                    {
                        Name = x.Name,
                        SourceType = x.SourceType,
                        Location = x.Location
                    };
                })
                .ToList();

            Func<Guid, CraftingItemData> lookupCraftingItem = id => saveData.AllCraftingItems.FirstOrDefault(x => x.Id == id);
            Func<Guid, CraftingMaterialData> lookupCraftingMaterial = id => saveData.AllCraftingMaterials.FirstOrDefault(x => x.Id == id);

            CraftingItem convertCraftingItem(CraftingItemData item)
            {
                if (item == null)
                    throw new InvalidOperationException("Cannot convert null crafting item data.");

                var craftingItem = new CraftingItem
                {
                    Name = item.Name,
                    SourceType = item.SourceType
                };

                craftingItem.SetMaterials(
                    item.Materials
                        .Select(x =>
                        {
                            ICraftingMaterial craftingMaterial;

                            if (x.IsMaterial)
                            {
                                var material = lookupCraftingMaterial(x.ItemId);

                                if (material == null)
                                    throw new InvalidOperationException($"Cannot find saved material with id '{x.ItemId}'.");

                                craftingMaterial = new CraftingMaterial
                                {
                                    Name = material.Name,
                                    SourceType = material.SourceType,
                                    Location = material.Location
                                };
                            }
                            else
                            {
                                var craftingItem = lookupCraftingItem(x.ItemId);

                                if (craftingItem == null)
                                    throw new InvalidOperationException($"Cannot find saved crafting item with id '{x.ItemId}'.");

                                craftingMaterial = convertCraftingItem(craftingItem);
                            }

                            return new SpecifiedCraftingMaterial
                            {
                                Material = craftingMaterial,
                                Count = x.Count
                            };
                        }));

                return craftingItem;
            };

            var itemsList = saveData.AllCraftingItems
                .Select(item => convertCraftingItem(item))
                .ToList();

            var listedItemsList = saveData.ListedItems
                .Select(item =>
                {
                    var itemData = lookupCraftingItem(item.ItemId);

                    if (itemData == null)
                        throw new InvalidOperationException($"Cannot find saved crafting item with id '{item.ItemId}'.");

                    var craftingItem = itemsList.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, itemData.Name));

                    if (craftingItem == null)
                        throw new InvalidOperationException($"Cannot find rendered crafting item with name '{itemData.Name}'.");

                    return new SpecifiedCraftingItem
                    {
                        Item = craftingItem,
                        Count = item.Count
                    };
                })
                .ToList();

            return new SaveDataResults
            {
                DefinedCraftingMaterials = materialsList,
                DefinedCraftingItems = itemsList,
                CraftingItems = listedItemsList
            };
        }
    }
}

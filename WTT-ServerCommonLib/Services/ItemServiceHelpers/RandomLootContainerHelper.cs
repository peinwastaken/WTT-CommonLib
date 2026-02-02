using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Logger;
using WTTServerCommonLib.Helpers;
using WTTServerCommonLib.Models;

namespace WTTServerCommonLib.Services.ItemServiceHelpers;

[Injectable]
public class RandomLootContainerHelper(DatabaseService databaseService, SptLogger<RandomLootContainerHelper> logger, ConfigServer configServer)
{
    public void ConfigureRandomLootContainer(CustomItemConfig itemConfig, string newItemId)
    {
        var inventoryConfig = configServer.GetConfig<InventoryConfig>();
        var itemDb = databaseService.GetItems();
        var itemInDb = itemDb.GetValueOrDefault(newItemId);
        if (itemInDb == null)
        {
            logger.Error("Item not found in db. Something is seriously wrong.");
            return;
        }
        itemInDb.Name = newItemId;
        inventoryConfig.RandomLootContainers[newItemId] = itemConfig.RandomLootContainerRewards ?? throw new ArgumentNullException(nameof(itemConfig.RandomLootContainerRewards));
    }
}
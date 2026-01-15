using System;
using System.Collections.Generic;
using System.Linq;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using WTTServerCommonLib.Helpers;
using WTTServerCommonLib.Models;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace WTTServerCommonLib.Services.ItemServiceHelpers;

[Injectable]
public class CaliberHelper(ISptLogger<CaliberHelper> logger, DatabaseService databaseService)
{
    private readonly List<(string newItemId, CustomItemConfig config)> _deferredCaliberConfigs = new();

    public void AddNewCaliberToItems(CustomItemConfig itemConfig, string newItemId)
    {
        AddDeferredCaliberConfig(newItemId, itemConfig);
    }

    private void AddDeferredCaliberConfig(string newItemId, CustomItemConfig config)
    {
        if (_deferredCaliberConfigs.Any(d => d.newItemId == newItemId))
        {
            logger.Warning($"Deferred caliber config for {newItemId} already exists, skipping.");
            return;
        }

        _deferredCaliberConfigs.Add((newItemId, config));
    }

    public void ProcessDeferredCalibers()
    {
        if (_deferredCaliberConfigs.Count == 0)
        {
            LogHelper.Debug(logger, "No deferred caliber configs to process");
            return;
        }

        LogHelper.Debug(logger, $"Processing {_deferredCaliberConfigs.Count} deferred caliber configs...");

        var tables = databaseService.GetTables();
        var items = tables.Templates.Items;

        foreach (var (newItemId, config) in _deferredCaliberConfigs)
        {
            try
            {
                foreach (var (itemId, item) in items)
                {
                    UpdateItemFilters(item, config.ItemTplToClone, newItemId, itemId);
                }

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    LogHelper.Debug(logger, $"Processed caliber config for {newItemId}");
                }
            }
            catch (Exception ex)
            {
                logger.Critical($"Failed processing caliber config for {newItemId}", ex);
            }
        }

        _deferredCaliberConfigs.Clear();
        LogHelper.Debug(logger, "Finished processing deferred caliber configs");
    }

    private void UpdateItemFilters(TemplateItem item, string cloneId, string newId, string itemId)
    {
        if (item.Properties?.Cartridges != null)
            foreach (var cartridge in item.Properties.Cartridges)
                if (cartridge.Properties?.Filters != null)
                    UpdateFilters(cartridge.Properties.Filters, cloneId, newId, itemId);

        if (item.Properties?.Slots != null)
            foreach (var slot in item.Properties.Slots)
                if (slot.Properties?.Filters != null)
                    UpdateFilters(slot.Properties.Filters, cloneId, newId, itemId);

        if (item.Properties?.Chambers != null)
            foreach (var chamber in item.Properties.Chambers)
                if (chamber.Properties?.Filters != null)
                    UpdateFilters(chamber.Properties.Filters, cloneId, newId, itemId);
    }

    private void UpdateFilters(IEnumerable<SlotFilter> filters, string cloneId, string newId, string itemId)
    {
        foreach (var filter in filters)
            if (filter.Filter != null && filter.Filter.Contains(cloneId) && filter.Filter.Add(newId))
                LogHelper.Debug(logger, $"Added {newId} to filter in {itemId}");
    }
}

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace WTTServerCommonLib.Helpers;

[Injectable]
public class SlotHelper
{
    public void ReplaceSlotFilters(TemplateItem item, int slotIndex, int filterIndex, HashSet<MongoId> ids)
    {
        var slot = GetSlotAtIndex(item, slotIndex);
        var filter = GetSlotFilterAtIndex(slot, filterIndex);

        filter.Filter = ids;
    }
    public void ModifySlotFilters(TemplateItem item, int slotIndex, int filterIndex, List<MongoId> ids, bool isCartridge = false)
    {
        var slot = GetSlotAtIndex(item, slotIndex, isCartridge);
        var filter = GetSlotFilterAtIndex(slot, filterIndex);

        filter.Filter!.UnionWith(ids);
    }
    private Slot GetSlotAtIndex(TemplateItem item, int index, bool isCartridge = false)
    {
        var slots = isCartridge ? item.Properties?.Cartridges?.ToArray() : item.Properties?.Slots?.ToArray();

        if (index >= 0 && index < slots?.Length)
        {
            return slots[index];
        }

        throw new IndexOutOfRangeException($"Index on item slot property `{item.Name}` is out of range");
    }
    public void AddIdsToNamedSlot(TemplateItem item, string slotName, params string[] ids)
    {
        var slot = item.Properties.Slots.FirstOrDefault(s => s.Name == slotName);
        var slotFilter = slot?.Properties?.Filters?.FirstOrDefault();

        if (slotFilter?.Filter == null)
            return;

        foreach (var id in ids)
        {
            if (!slotFilter.Filter.Contains(id))
            {
                slotFilter.Filter.Add(id);
            }
        }
    }
    private SlotFilter GetSlotFilterAtIndex(Slot slot, int index)
    {
        var slotFilter = slot.Properties?.Filters?.ToArray() ?? [];

        if (index >= 0 && index < slotFilter.Length)
        {
            return slotFilter[index];
        }

        throw new IndexOutOfRangeException($"Index on slot property `{slot.Name}` is out of range");
    }
}
using System;
using ItemSortOrderManager = GClass3381;

namespace WTTClientCommonLib.Helpers
{
    public static class SortHelper
    {
        public static void AddToSortOrder(this Type itemType, Type itemToInsertAfter, int offset = 1)
        {
            InsertAfter(itemType, itemToInsertAfter, offset);
        }
        
        public static void Insert(int sortIndex, Type itemType)
        {
            ItemSortOrderManager.List_0.Insert(sortIndex, itemType);
        }

        public static void InsertAfter(Type itemType, Type itemToInsertAfter, int offset = 1)
        {
            if (!ItemSortOrderManager.List_0.ContainsElement(itemToInsertAfter))
            {
                LogHelper.LogWarn($"failed to add item type {itemType} to sort order because item type {itemToInsertAfter} doesn't exist in sort order list");
                return;
            }
            
            int itemIndex = ItemSortOrderManager.IndexOf(itemToInsertAfter);
            
            ItemSortOrderManager.List_0.Insert(itemIndex + offset, itemType);
        }
    }
}

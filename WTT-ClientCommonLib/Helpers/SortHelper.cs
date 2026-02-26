using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using ItemSortOrderManager = GClass3381;

namespace WTTClientCommonLib.Helpers
{
    public static class SortHelper
    {
        public static void AddToSortOrder(this Type itemType, Type itemTypeToInsertAfter, int offset = 1)
        {
            InsertAfter(itemType, itemTypeToInsertAfter, offset);
        }

        public static int GetSortOrder(this Type itemType)
        {
            return ItemSortOrderManager.List_0.IndexOf(itemType);
        }
        
        public static void Insert(int sortIndex, Type itemType)
        {
            ItemSortOrderManager.List_0.Insert(sortIndex, itemType);
        }

        public static void SetIndex(int newIndex, Type itemType)
        {
            List<Type> list = ItemSortOrderManager.List_0;
            int oldIndex = list.IndexOf(itemType);

            if (oldIndex < 0)
            {
                LogHelper.LogWarn($"failed to set sort index for item {itemType} because it does not exist in sort order list");
                return;
            }
            
            newIndex = Math.Clamp(newIndex, 0, list.Count - 1);
            
            list.RemoveAt(oldIndex);
            
            if (newIndex > oldIndex)
            {
                newIndex--;
            }
            
            list.Insert(newIndex, itemType);
        }

        public static void InsertAfter(Type itemType, Type itemToInsertAfter, int offset = 1)
        {
            int itemIndex = ItemSortOrderManager.IndexOf(itemToInsertAfter);
            
            if (itemIndex < 0)
            {
                LogHelper.LogWarn($"failed to add item type {itemType} to sort order because item type {itemToInsertAfter} doesn't exist in sort order list");
                return;
            }
            
            ItemSortOrderManager.List_0.Insert(itemIndex + offset, itemType);
        }
    }
}

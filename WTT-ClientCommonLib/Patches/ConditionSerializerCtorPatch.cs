using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using WTTClientCommonLib.Components;

namespace WTTClientCommonLib.Patches
{
    internal class ConditionSerializerCtorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Constructor(typeof(GClass1871), Type.EmptyTypes);
        }

        [PatchPostfix]
        public static void PatchPostfix(GClass1871 __instance)
        {
            var listField = AccessTools.Field(typeof(GClass1871), "List_0");
            if (listField == null)
                return;

            var list = listField.GetValue(__instance) as List<Type>;
            if (list == null)
                return;

            var salvageType = typeof(ConditionSalvage);
            if (!list.Contains(salvageType))
                list.Add(salvageType);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Interactive;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Patching;
using WTTClientCommonLib.Components;
using WTTClientCommonLib.Helpers;


internal class Salvage_InvokeConditionsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(QuestControllerAbstractClass<IConditional>),
            "InvokeConditionsConnector");
    }

    [PatchPrefix]
    public static bool Prefix(
        QuestClass conditional,
        EQuestStatus status,
        Condition condition,
        QuestControllerAbstractClass<IConditional> __instance)
    {
        if (condition is not ConditionSalvage salvage)
            return true;
        
        ConditionSalvage conditionSalvage = condition as ConditionSalvage;
        __instance.method_7(conditional, status, conditionSalvage);
        return false;
    }
}



internal static class SalvageZoneTracker
{
    private static readonly Dictionary<Player, SalvageItemTrigger> _active = new();

    public static void Set(Player player, SalvageItemTrigger zone)
    {
        if (player == null) return;
        _active[player] = zone;
    }

    public static void Clear(Player player, SalvageItemTrigger zone)
    {
        if (player == null) return;

        if (_active.TryGetValue(player, out var current) && current == zone)
            _active.Remove(player);
    }

    public static SalvageItemTrigger Get(Player player)
    {
        if (player == null) return null;
        return _active.TryGetValue(player, out var zone) ? zone : null;
    }
}

internal class Salvage_AddTriggerZonePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.AddTriggerZone));
    }

    [PatchPostfix]
    public static void Postfix(Player __instance, TriggerWithId zone)
    {
        if (zone is not SalvageItemTrigger salvage)
            return;

        SalvageZoneTracker.Set(__instance, salvage);

        __instance.SearchForInteractions();
    }
}

internal class Salvage_RemoveTriggerZonePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.RemoveTriggerZone));
    }

    [PatchPostfix]
    public static void Postfix(Player __instance, TriggerWithId zone)
    {
        if (zone is not SalvageItemTrigger salvage)
            return;

        SalvageZoneTracker.Clear(__instance, salvage);

        __instance.SearchForInteractions();
    }
}


internal class Salvage_InteractionsChangedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GamePlayerOwner), nameof(GamePlayerOwner.InteractionsChangedHandler));
    }

    [PatchPrefix]
    private static bool Prefix(GamePlayerOwner __instance)
    {
        var player = __instance.Player;
        if (player == null)
            return true;

        if (player.InteractableObject != null ||
            player.PlaceItemZone != null ||
            player.BtrInteractionSide != null ||
            player.TripwireInteractionTrigger != null ||
            player.EventObjectInteractive != null ||
            player.ExfiltrationPoint != null)
        {
            return true;
        }

        var salvage = SalvageZoneTracker.Get(player);
        if (salvage == null)
            return true;

        GInterface177 interactive = salvage;

        var actions = GetActionsClass.GetAvailableActions(__instance, interactive);
        TransitInteractionControllerAbstractClass transit;
        if (actions == null &&
            TransitControllerAbstractClass.Exist<TransitInteractionControllerAbstractClass>(out transit))
        {
            actions = transit.AvailableInteractionState;
        }

        if (actions != null)
            actions.InitSelected();

        __instance.AvailableInteractionState.Value = actions;
        return false;
    }
}


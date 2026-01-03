using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace WTTServerCommonLib.Helpers;

[Injectable]
public class QuestHelper(ISptLogger<QuestHelper> logger)
{
    public void AddWeaponsToKillCondition(Dictionary<MongoId, Quest> quests, string questId, string[] weaponIds)
    {
        if (!quests.TryGetValue(questId, out var quest))
        {
            logger.Warning($"Quest {questId} not found");
            return;
        }

        if (quest.Conditions.AvailableForFinish == null)
        {
            logger.Warning($"Quest {questId} has no AvailableForFinish conditions");
            return;
        }

        var modified = false;

        foreach (var condition in quest.Conditions.AvailableForFinish)
        {
            logger.Debug($"Checking condition type: {condition.ConditionType}");

            if (condition is { ConditionType: "CounterCreator", Counter.Conditions: not null })
            {
                foreach (var counterCond in condition.Counter.Conditions)
                {
                    logger.Debug($"  Counter condition type: {counterCond.ConditionType}");

                    if (counterCond is { Weapon: not null, ConditionType: "Kills" or "Shots" })
                    {
                        var beforeCount = counterCond.Weapon.Count;

                        foreach (var weaponId in weaponIds)
                        {
                            if (counterCond.Weapon.Add(weaponId))
                            {
                                modified = true;
                                logger.Debug($"    Added weapon {weaponId}");
                            }
                        }

                        logger.Debug($"  Weapon count before: {beforeCount}, after: {counterCond.Weapon.Count}");
                    }
                }
            }
        }

        if (modified)
        {
            logger.Debug($"Successfully modified quest {questId}");
        }
        else
        {
            logger.Warning($"No modifications made to quest {questId} - condition structure might differ");
        }
    }

    public void AddArmorToEquipmentExclusive(Dictionary<MongoId, Quest> quests, string questId, string[] armorIds)
    {
        if (!quests.TryGetValue(questId, out var quest) || quest.Conditions.AvailableForFinish == null)
            return;

        foreach (var condition in quest.Conditions.AvailableForFinish)
        {
            if (condition is { ConditionType: "CounterCreator", Counter.Conditions: not null })
            {
                foreach (var counterCond in condition.Counter.Conditions)
                {
                    if (counterCond is { ConditionType: "Equipment", EquipmentExclusive: not null })
                    {
                        foreach (var armorId in armorIds)
                        {
                            counterCond.EquipmentExclusive.Add([armorId]);
                        }
                    }
                }
            }
        }
    }

    public void AddWeaponsToFindOrHandoverCondition(Dictionary<MongoId, Quest> quests, string questId,
        string[] weaponIds)
    {
        if (!quests.TryGetValue(questId, out var quest) || quest.Conditions.AvailableForFinish == null)
            return;

        foreach (var condition in quest.Conditions.AvailableForFinish)
        {
            if ((condition.ConditionType == "FindItem" || condition.ConditionType == "HandoverItem") &&
                condition.Target != null)
            {
                foreach (var weaponId in weaponIds)
                {
                    if (condition.Target.List != null && !condition.Target.List.Contains(weaponId))
                    {
                        condition.Target.List.Add(weaponId);
                    }
                }
            }
        }
    }
}
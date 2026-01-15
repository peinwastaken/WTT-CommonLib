using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using WTTServerCommonLib.Helpers;
using Path = System.IO.Path;

namespace WTTServerCommonLib.Services;

[Injectable(InjectionType.Singleton)]
public class WTTCustomDialogueService(
    ModHelper modHelper, 
    ISptLogger<WTTCustomDialogueService> logger, 
    ConfigHelper configHelper, 
    DatabaseService databaseService)
{
    /// <summary>
    /// Loads custom NPC Dialogues from JSON/JSONC files and registers them to the game database.
    /// 
    /// Dialogues are loaded from the mod's "config/CustomDialogues" directory (or a custom path if specified).
    /// </summary>
    /// <param name="assembly">The calling assembly, used to determine the mod folder location</param>
    /// <param name="relativePath">(OPTIONAL) Custom path relative to the mod folder</param>
    public async Task CreateCustomDialogues(Assembly assembly, string? relativePath = null)

    {
        
        var assemblyLocation = modHelper.GetAbsolutePathToModFolder(assembly);
        var defaultDir = Path.Combine("db", "CustomDialogues");
        var finalDir = Path.Combine(assemblyLocation, relativePath ?? defaultDir);

        if (!Directory.Exists(finalDir))
        {
            logger.Warning($"Custom dialogues directory not found at {finalDir}");
            return;
        }

        var jsonFiles = Directory.GetFiles(finalDir, "*.json")
            .Concat(Directory.GetFiles(finalDir, "*.jsonc"))
            .ToArray();
            
        if (jsonFiles.Length == 0)
        {
            logger.Warning($"No custom dialogue files found in {finalDir}");
            return;
        }

        var traderDialogElements = databaseService.GetTemplates().Dialogue.Elements;
        foreach (var file in jsonFiles)
        {
            try
            {
                
                var dialogueData = await configHelper.LoadJsonFile<List<TraderDialogElement>>(file);

                if (dialogueData == null)
                {
                    logger.Error($"Failed to load dialogue data from {file}");
                    continue;
                }

                foreach (var element in dialogueData)
                {
                    traderDialogElements.Add(element);
                    LogHelper.Debug(logger, $"Successfully added custom dialogue to the server!");
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading dialogue file {file}: {ex.Message}");
            }
        }
    }
}

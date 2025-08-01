using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;


namespace DomaPuckTrajectoryTrace;

public class Plugin : IPuckMod
{
    public static string MOD_NAME = "DomaPuckTrajectoryTrace";
    public static string MOD_VERSION = "1.0.0";
    public static string MOD_GUID = "dp.rp.DomaPho.DomaPuckTrajectoryTrace";
    static readonly Harmony harmony = new Harmony(MOD_GUID);

    public static ServerManager serverManager;
    public static GameManager gameManager;
    public static PlayerManagerController playerManagerController;
    public static PlayerManager playerManager;
    public static string resultsPath = ".//results";
    public static DirectoryInfo resultsDir = Directory.CreateDirectory(resultsPath);

    // TODO 

    public bool OnEnable()
    {
        Plugin.Log($"Enabling...");
        try
        {
            if (IsDedicatedServer())
            {
                Plugin.Log("Environment: dedicated server.");
                Plugin.Log("Patching methods...");
                //harmony.PatchAll();
                //Plugin.Log($"All patched! Patched methods:");
                Plugin.Log($"This mod is designed to be used only on clients!");
            }
            else
            {
                Plugin.Log("Environment: client.");
                Plugin.Log("Patching methods...");
                Plugin.Log($"This mod is designed to be used only on dedicated servers!");
                harmony.PatchAll();
                Plugin.Log($"All patched! Patched methods:");

            }

            Plugin.Log($"Enabled!");
            return true;
        }
        catch (Exception e)
        {
            Plugin.LogError($"Failed to Enable: {e.Message}!");
            return false;
        }
    }

    public bool OnDisable()
    {
        try
        {
            Plugin.Log($"Disabling...");
            harmony.UnpatchSelf();
            Plugin.Log($"Disabled! Goodbye!");
            return true;
        }
        catch (Exception e)
        {
            Plugin.LogError($"Failed to disable: {e.Message}!");
            return false;
        }
    }

    public static bool IsDedicatedServer()
    {
        return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }

    public static void LogAllPatchedMethods()
    {
        var allPatchedMethods = harmony.GetPatchedMethods();
        var pluginId  = harmony.Id;

        var mine = allPatchedMethods
            .Select(m => new { method = m, info = Harmony.GetPatchInfo(m) })
            .Where(x =>
                // could be prefix, postfix, transpiler or finalizer
                x.info.Prefixes.  Any(p => p.owner == pluginId) ||
                x.info.Postfixes. Any(p => p.owner == pluginId) ||
                x.info.Transpilers.Any(p => p.owner == pluginId) ||
                x.info.Finalizers.Any(p => p.owner == pluginId)
            )
            .Select(x => x.method);

        foreach (var m in mine)
            Plugin.Log($" - {m.DeclaringType.FullName}.{m.Name}");
    }
    
    
    public static void Log(string message)
    {
        Debug.Log($"[{MOD_NAME}] {message}");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"[{MOD_NAME}] {message}");
    }
}
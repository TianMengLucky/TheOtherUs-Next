namespace TheOtherUs.Patches;

/*[HarmonyPatch(typeof(ShipStatus))]
public class ShipStatusPatch
{
    private static int originalNumCommonTasksOption;
    private static int originalNumShortTasksOption;
    private static int originalNumLongTasksOption;
    public static float originalNumCrewVisionOption = 0;
    public static float originalNumImpVisionOption = 0;
    public static float originalNumKillCooldownOption = 0;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static bool Prefix(ref float __result, ShipStatus __instance,
        [HarmonyArgument(0)] NetworkedPlayerInfo player)
    {
        if (!__instance.Systems.ContainsKey(SystemTypes.Electrical) ||
            GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
        var switchSystem = GameOptionsManager.Instance.currentNormalGameOptions.MapId == 5
            ? null
            : __instance.Systems[SystemTypes.Electrical]?.TryCast<SwitchSystem>();
        // If Game Mode is PropHunt:
        if (PropHunt.isPropHuntGM)
        {
            if (!PropHunt.timerRunning)
            {
                var progress = PropHunt.blackOutTimer > 0f && PropHunt.blackOutTimer < 1f
                    ? 1 - PropHunt.blackOutTimer
                    : 0f;
                var minVision = __instance.MaxLightRadius *
                                (PropHunt.propBecomesHunterWhenFound ? 0.25f : PropHunt.propVision);
                __result = Mathf.Lerp(minVision, __instance.MaxLightRadius * PropHunt.propVision,
                    progress); // For future start animation
            }
            else
            {
                __result = __instance.MaxLightRadius * (PlayerControl.LocalPlayer.Data.Role.IsImpostor
                    ? PropHunt.hunterVision
                    : PropHunt.propVision);
            }

            return false;
        }

        if (!HideNSeek.isHideNSeekGM || (HideNSeek.isHideNSeekGM && !Hunter.lightActive.Contains(player.PlayerId)))
            // If player is a role which has Impostor vision
            if (Helpers.hasImpVision(player))
            {
                //__result = __instance.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
                __result = GetNeutralLightRadius(__instance, true);
                return false;
            }
            

        /#1#/ If Game mode is Hide N Seek and hunter with ability active
        else if (HideNSeek.isHideNSeekGM && Hunter.isLightActive(player.PlayerId))
        {
            var unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius,
                GetNeutralLightRadius(__instance, false));
            __result = Mathf.Lerp(__instance.MaxLightRadius * Hunter.lightVision,
                __instance.MaxLightRadius * Hunter.lightVision, unlerped);
            return false;
        }#1#

        // If there is a Trickster with their ability active
        if (Trickster.trickster != null && Trickster.lightsOutTimer > 0f)
        {
            var lerpValue = 1f;
            if (Trickster.lightsOutDuration - Trickster.lightsOutTimer < 0.5f)
                lerpValue = Mathf.Clamp01((Trickster.lightsOutDuration - Trickster.lightsOutTimer) * 2);
            else if (Trickster.lightsOutTimer < 0.5) lerpValue = Mathf.Clamp01(Trickster.lightsOutTimer * 2);

            __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1 - lerpValue) *
                       GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
        }

        // If player is Lawyer, apply Lawyer vision modifier
        else if (Lawyer.lawyer != null && Lawyer.lawyer.PlayerId == player.PlayerId)
        {
            var unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius,
                GetNeutralLightRadius(__instance, false));
            __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius * Lawyer.vision, unlerped);
            return false;
        }

        // Default light radius
        else
        {
            __result = GetNeutralLightRadius(__instance, false);
        }
        /*
        // Additional code
        //var switchSystem = GameOptionsManager.Instance.currentNormalGameOptions.MapId == 5 ? null : __instance.Systems[SystemTypes.Electrical]?.TryCast<SwitchSystem>();
        var t = switchSystem != null ? switchSystem.Value / 255f : 1;
        if (Torch.torch.FindAll(x => x.PlayerId == player.PlayerId).Count > 0) t = 1;
        //__result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, t) *
        //           GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
        #1#

        if (Torch.torch.FindAll(x => x.PlayerId == player.PlayerId).Count > 0) // Torch
            __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1) *
                       GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod * Torch.vision;

        if (Sunglasses.sunglasses.FindAll(x => x.PlayerId == player.PlayerId).Count > 0) // Sunglasses
            __result *= 1f - (Sunglasses.vision * 0.1f);

        return false;
    }

    public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor)
    {
        if (SubmergedCompatibility.IsSubmerged)
            return SubmergedCompatibility.GetSubmergedNeutralLightRadius(isImpostor);

        if (isImpostor)
            return shipStatus.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
        var lerpValue = 1.0f;
        try
        {
            var switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
            lerpValue = switchSystem.Value / 255f;
        }
        catch
        {
        }

        return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) *
               GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    public static void Postfix2(ShipStatus __instance, ref bool __result)
    {
        __result = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    public static bool Prefix(ShipStatus __instance)
    {
        originalNumCommonTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks;
        originalNumShortTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks;
        originalNumLongTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks;

        if (MapOptions.gameMode != CustomGameModes.HideNSeek)
        {
            var commonTaskCount = __instance.CommonTasks.Count;
            var normalTaskCount = __instance.ShortTasks.Count;
            var longTaskCount = __instance.LongTasks.Count;

            if (MapOptions.gameMode == CustomGameModes.PropHunt)
                commonTaskCount = normalTaskCount = longTaskCount = 0;


            if (GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks > commonTaskCount)
                GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = commonTaskCount;
            if (GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks > normalTaskCount)
                GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = normalTaskCount;
            if (GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks > longTaskCount)
                GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = longTaskCount;
        }
        else
        {
            GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks =
                Mathf.RoundToInt(CustomOptionHolder.hideNSeekCommonTasks.getFloat());
            GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks =
                Mathf.RoundToInt(CustomOptionHolder.hideNSeekShortTasks.getFloat());
            GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks =
                Mathf.RoundToInt(CustomOptionHolder.hideNSeekLongTasks.getFloat());
        }

        MapBehaviourPatch.VentNetworks.Clear();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    public static void Postfix3(ShipStatus __instance)
    {
        // Restore original settings after the tasks have been selected
        GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = originalNumCommonTasksOption;
        GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = originalNumShortTasksOption;
        GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = originalNumLongTasksOption;
    }

    public static void resetVanillaSettings()
    {
        GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod = originalNumImpVisionOption;
        GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = originalNumCrewVisionOption;
        GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown = originalNumKillCooldownOption;
    }
}*/
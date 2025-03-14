﻿namespace TheOtherUs.Patches;

/*[HarmonyPatch(typeof(MapBehaviour))]
internal static class MapBehaviourPatch
{
    public static Dictionary<PlayerControl, SpriteRenderer> herePoints = new();

    public static Sprite Vent = UnityHelper.loadSpriteFromResources("TheOtherUs.Resources.Vent.png", 150f);

    public static List<List<Vent>> VentNetworks = [];

    public static Dictionary<string, GameObject> mapIcons = new();

    public static void clearAndReload()
    {
        foreach (var mapIcon in mapIcons.Values) mapIcon.Destroy();
        mapIcons = new Dictionary<string, GameObject>();
        VentNetworks = [];
        herePoints = new Dictionary<PlayerControl, SpriteRenderer>();
    }


    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    private static void Postfix(MapBehaviour __instance)
    {
        __instance.HerePoint.transform.SetLocalZ(-2.1f);
        if (Trapper.trapper != null && LocalPlayer.PlayerId == Trapper.trapper.PlayerId)
        {
            foreach (PlayerControl player in Trapper.playersOnMap)
            {
                if (herePoints.ContainsKey(player)) continue;
                var v = Trap.trapPlayerIdMap[player.PlayerId].trap.transform.position;
                v /= MapUtilities.CachedShipStatus.MapScale;
                v.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
                v.z = -2.1f;
                var herePoint = Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent, true);
                herePoint.transform.localPosition = v;
                herePoint.enabled = true;
                var colorId = player.CurrentOutfit.ColorId;
                if (Trapper.anonymousMap) player.CurrentOutfit.ColorId = 6;
                player.SetPlayerMaterialColors(herePoint);
                player.CurrentOutfit.ColorId = colorId;
                herePoints.Add(player, herePoint);
            }

            foreach (var s in herePoints.Where(x => !Trapper.playersOnMap.Contains(x.Key)).ToList())
            {
                Object.Destroy(s.Value);
                herePoints.Remove(s.Key);
            }
        }

        foreach (var vent in MapUtilities.CachedShipStatus.AllVents)
        {
            if (vent.name.StartsWith("JackInThe") && !(PlayerControl.LocalPlayer == Trickster.trickster ||
                                                       PlayerControl.LocalPlayer.Data.IsDead))
                continue; //for trickster vents

            if (!Main.ShowVentsOnMap.Value)
            {
                if (mapIcons.Count > 0)
                {
                    mapIcons.Values.Do(x => x.Destroy());
                    mapIcons.Clear();
                }

                break;
            }

            var Instance = DestroyableSingleton<MapTaskOverlay>.Instance;
            var task = PlayerControl.LocalPlayer.myTasks.ToArray()
                .FirstOrDefault(x => x.TaskType == TaskTypes.VentCleaning);

            var location = vent.transform.position / MapUtilities.CachedShipStatus.MapScale;
            location.z = -2f; //show above sabotage buttons

            GameObject MapIcon;
            if (!mapIcons.ContainsKey($"vent {vent.Id} icon"))
            {
                MapIcon = Object.Instantiate(__instance.HerePoint.gameObject, __instance.HerePoint.transform.parent);
                mapIcons.Add($"vent {vent.Id} icon", MapIcon);
            }
            else
            {
                MapIcon = mapIcons[$"vent {vent.Id} icon"];
            }

            MapIcon.GetComponent<SpriteRenderer>().sprite = Vent;

            MapIcon.name = $"vent {vent.Id} icon";
            MapIcon.transform.localPosition = location;

            if (task?.IsComplete == false && task.FindConsoles().Get(0).ConsoleId == vent.Id)
                MapIcon.transform.localScale *= 0.6f;
            if (vent.name.StartsWith("JackInThe"))
            {
                MapIcon.GetComponent<SpriteRenderer>().sprite = JackInTheBox.getBoxAnimationSprite(0);
                MapIcon.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                MapIcon.GetComponent<SpriteRenderer>().color =
                    vent.isActiveAndEnabled ? Color.yellow : Color.yellow.SetAlpha(0.5f);
            }

            if (AllVentsRegistered(Instance))
            {
                var array = VentNetworks.ToArray();
                foreach (var connectedgroup in VentNetworks)
                {
                    var index = Array.IndexOf(array, connectedgroup);
                    if (connectedgroup[0].name.StartsWith("JackInThe"))
                        continue;
                    connectedgroup.Do(
                        x => GetIcon(x).GetComponent<SpriteRenderer>().color = Palette.PlayerColors[index]);
                }

                continue;
            }

            HandleMiraOrSub();

            var network = GetNetworkFor(vent);
            if (network == null)
            {
                VentNetworks.Add([..vent.NearbyVents.Where(x => x != null), vent]);
            }
            else
            {
                if (!network.Any(x => x == vent)) network.Add(vent);
            }
        }

        HudManagerUpdate.CloseSettings();
    }

    public static List<Vent> GetNetworkFor(Vent vent)
    {
        return VentNetworks.FirstOrDefault(x =>
            x.Any(y => y == vent || y == vent.Left || y == vent.Center || y == vent.Right));
    }

    public static bool AllVentsRegistered(MapTaskOverlay __instance)
    {
        foreach (var vent in MapUtilities.CachedShipStatus.AllVents)
        {
            if (!vent.isActiveAndEnabled) continue;
            var network = GetNetworkFor(vent);
            if (network == null || !network.Any(x => x == vent)) return false;
            if (!mapIcons.ContainsKey($"vent {vent.Id} icon")) return false;
        }

        return true;
    }

    public static GameObject GetIcon(Vent vent)
    {
        var icon = mapIcons[$"vent {vent.Id} icon"];
        return icon;
    }

    public static void HandleMiraOrSub()
    {
        if (VentNetworks.Count != 0) return;

        if (Helpers.isMira())
        {
            var vents = MapUtilities.CachedShipStatus.AllVents.Where(x => !x.name.Contains("JackInTheBoxVent_"));
            VentNetworks.Add(vents.ToList());
            return;
        }

        if (MapUtilities.CachedShipStatus.Type == SubmergedCompatibility.SUBMERGED_MAP_TYPE)
        {
            var vents = MapUtilities.CachedShipStatus.AllVents.Where(x => x.Id is 12 or 13 or 15 or 16);
            VentNetworks.Add(vents.ToList());
        }
    }
}*/
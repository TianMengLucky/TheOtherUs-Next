using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherUs.Modules.Compatibility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherUs.Objects;

internal class Trap
{
    public static List<Trap> traps = [];
    public static Dictionary<byte, Trap> trapPlayerIdMap = new();

    private static int instanceCounter;

    private static Sprite trapSprite;
    private readonly Arrow arrow = new(Color.blue);
    public readonly int instanceId;
    /*private readonly int neededCount = Trapper.trapCountToReveal;*/
    public bool revealed;
    public GameObject trap;
    public List<PlayerControl> trappedPlayer = [];
    public bool triggerable;
    private int usedCount;

    public Trap(Vector2 p)
    {
        trap = new GameObject("Trap") { layer = 11 };
        trap.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
        var position = new Vector3(p.x, p.y, (p.y / 1000) + 0.001f); // just behind player
        trap.transform.position = position;
        /*neededCount = Trapper.trapCountToReveal;*/

        var trapRenderer = trap.AddComponent<SpriteRenderer>();
        trapRenderer.sprite = getTrapSprite();
        trap.SetActive(false);
        if (LocalPlayer.Is<Trapper>()) trap.SetActive(true);
        instanceId = ++instanceCounter;
        traps.Add(this);
        arrow.Update(position);
        arrow.arrow.SetActive(false);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5, new Action<float>(x =>
        {
            if (x == 1f) triggerable = true;
        })));
    }

    public static Sprite getTrapSprite()
    {
        if (trapSprite) return trapSprite;
        trapSprite = UnityHelper.loadSpriteFromResources("TheOtherUs.Resources.Trapper_Trap_Ingame.png", 300f);
        return trapSprite;
    }

    public static void clearTraps()
    {
        foreach (var t in traps)
        {
            Object.Destroy(t.arrow.arrow);
            Object.Destroy(t.trap);
        }

        traps = [];
        trapPlayerIdMap = new Dictionary<byte, Trap>();
        instanceCounter = 0;
    }

    public static void clearRevealedTraps()
    {
        var trapsToClear = traps.FindAll(x => x.revealed);

        foreach (var t in trapsToClear)
        {
            traps.Remove(t);
            Object.Destroy(t.trap);
        }
    }

    public static void triggerTrap(byte playerId, byte trapId)
    {
        var t = traps.FirstOrDefault(x => x.instanceId == trapId);
        var player = playerId.GetCachePlayer();
        var localIsTrapper = LocalPlayer.Is<Trapper>();;
        trapPlayerIdMap.TryAdd(playerId, t);
        if (t == null) return;
        {
            t.usedCount++;
            t.triggerable = false;
            if (playerId == LocalPlayer.PlayerId || playerId.Is<Trapper>())
            {
                t.trap.SetActive(true);
                SoundEffectsManager.play("trapperTrap");
            }

            player.Control.moveable = false;
            player.NetTransform.Halt();
            /*Trapper.playersOnMap.Add(player);*/
            if (localIsTrapper) t.arrow.arrow.SetActive(true);

            /*FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Trapper.trapDuration,
                new Action<float>(p =>
                {
                    if (p == 1f)
                    {
                        player.Control.moveable = true;
                        /*Trapper.playersOnMap.RemoveAll(x => x == player);#1#
                        if (trapPlayerIdMap.ContainsKey(playerId)) trapPlayerIdMap.Remove(playerId);
                        t.arrow.arrow.SetActive(false);
                    }
                })));*/

            /*if (t.usedCount == t.neededCount) t.revealed = true;*/

            t.trappedPlayer.Add(player);
            t.triggerable = true;
        }
    }

    public static void Update()
    {
        /*if (Trapper.trapper == null) return;*/
        var player = LocalPlayer;
        var vent = MapUtilities.CachedShipStatus.AllVents[0];
        var closestDistance = float.MaxValue;

        if (vent == null || player == null) return;
        var ud = vent.UsableDistance / 2;
        Trap target = null;
        foreach (var trap in traps)
        {
            if (trap.arrow.arrow.active) trap.arrow.Update();
            if (trap.revealed || !trap.triggerable || trap.trappedPlayer.Contains(player.Control)) continue;
            if (player.Control.inVent || !player.Control.CanMove) continue;
            var distance = Vector2.Distance(trap.trap.transform.position, player.Control.GetTruePosition());
            if (distance <= ud && distance < closestDistance)
            {
                closestDistance = distance;
                target = trap;
            }
        }

        /*if (target != null && player.PlayerId != Trapper.trapper.PlayerId && !player.Data.IsDead)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.TriggerTrap, SendOption.Reliable);
            writer.Write(player.PlayerId);
            writer.Write(target.instanceId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.triggerTrap(player.PlayerId, (byte)target.instanceId);
        }*/


        if (!player.IsDead || player.PlayerId.Is<Trapper>()) return;
        foreach (var trap in traps.Where(trap => !trap.trap.active))
            trap.trap.SetActive(true);
    }
}
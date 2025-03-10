namespace TheOtherUs.Patches;

/*[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class PlayerControlFixedUpdatePatch
{
    private static bool mushroomSaboWasActive;
    // Helpers

    private static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false,
        List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        var num = GameOptionsData.KillDistances[
            Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = LocalPlayer.Control;
        if (targetingPlayer.Data.IsDead) return result;

        var truePosition = targetingPlayer.GetTruePosition();
        foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead &&
                (!onlyCrewmates || !playerInfo.Role.IsImpostor))
            {
                var @object = playerInfo.Object;
                if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object))
                    // if that player is not targetable: skip check
                    continue;

                if (@object && (!@object.inVent || targetPlayersInVents))
                {
                    var vector = @object.GetTruePosition() - truePosition;
                    var magnitude = vector.magnitude;
                    if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized,
                            magnitude, Constants.ShipAndObjectsMask))
                    {
                        result = @object;
                        num = magnitude;
                    }
                }
            }

        return result;
    }

    private static void setPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;

        color = color.SetAlpha(Chameleon.visibility(target.PlayerId));

        target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
        target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
    }

    // Update functions

    private static void setBasePlayerOutlines()
    {
        foreach (PlayerControl target in AllPlayers)
        {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

            var isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null &&
                                     Morphling.morphTimer > 0f;
            var hasVisibleShield = false;
            Color color = Medic.shieldedColor;
            if (!Helpers.isCamoComms() && Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() &&
                Medic.shielded != null && ((target == Medic.shielded && !isMorphedMorphling) ||
                                           (isMorphedMorphling && Morphling.morphTarget == Medic.shielded)))
            {
                hasVisibleShield = Medic.showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone or Ghost info
                                                           || (Medic.showShielded == 1 &&
                                                               (LocalPlayer.Control == Medic.shielded ||
                                                                LocalPlayer.Control ==
                                                                Medic.medic)) // Shielded + Medic
                                                           || (Medic.showShielded == 2 &&
                                                               LocalPlayer.Control ==
                                                               Medic.medic); // Medic only
                // Make shield invisible till after the next meeting if the option is set (the medic can already see the shield)
                hasVisibleShield = hasVisibleShield && (Medic.meetingAfterShielding || !Medic.showShieldAfterMeeting ||
                                                        LocalPlayer.Control == Medic.medic ||
                                                        Helpers.shouldShowGhostInfo());
            }

            if (LocalPlayer.Data.IsDead && BodyGuard.guarded != null && target == BodyGuard.guarded)
            {
                hasVisibleShield = true;
                color = BodyGuard.color;
            }

            if (!Helpers.isCamoComms() && Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() &&
                MapOptions.firstKillPlayer != null && MapOptions.shieldFirstKill &&
                ((target == MapOptions.firstKillPlayer && !isMorphedMorphling) ||
                 (isMorphedMorphling && Morphling.morphTarget == MapOptions.firstKillPlayer)))
            {
                hasVisibleShield = true;
                color = Color.blue;
            }

            if (hasVisibleShield)
            {
                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
            }
            else
            {
                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
            }
        }
    }

    private static void setPetVisibility()
    {
        var localalive = !LocalPlayer.Data.IsDead;
        foreach (var player in AllPlayers)
        {
            var playeralive = !player.Data.IsDead;
            player.Control.cosmetics.SetPetVisible((localalive && playeralive) || !localalive);
        }
    }

    public static void bendTimeUpdate()
    {
        if (TimeMaster.isRewinding)
        {
            if (localPlayerPositions.Count > 0)
            {
                // Set position
                var next = localPlayerPositions[0];
                if (next.Item2)
                {
                    // Exit current vent if necessary
                    if (LocalPlayer.Control.inVent)
                        foreach (var vent in MapUtilities.CachedShipStatus.AllVents)
                        {
                            bool canUse;
                            bool couldUse;
                            vent.CanUse(LocalPlayer.Data, out canUse, out couldUse);
                            if (canUse)
                            {
                                LocalPlayer.Physics.RpcExitVent(vent.Id);
                                vent.SetButtons(false);
                            }
                        }

                    // Set position
                    LocalPlayer.transform.position = next.Item1;
                }
                else if (localPlayerPositions.Any(x => x.Item2))
                {
                    LocalPlayer.transform.position = next.Item1;
                }

                if (SubmergedCompatibility.IsSubmerged) SubmergedCompatibility.ChangeFloor(next.Item1.y > -7);

                localPlayerPositions.RemoveAt(0);

                if (localPlayerPositions.Count > 1)
                    localPlayerPositions
                        .RemoveAt(0); // Skip every second position to rewinde twice as fast, but never skip the last position
            }
            else
            {
                TimeMaster.isRewinding = false;
                LocalPlayer.Control.moveable = true;
            }
        }
        else
        {
            while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime))
                localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
            localPlayerPositions.Insert(0,
                new Tuple<Vector3, bool>(LocalPlayer.transform.position,
                    LocalPlayer.Control.CanMove)); // CanMove = CanMove
        }
    }

    private static void medicSetTarget()
    {
        if (Medic.medic == null || Medic.medic != LocalPlayer.Control) return;
        Medic.currentTarget = setTarget();
        if (!Medic.usedShield) setPlayerOutline(Medic.currentTarget, Medic.shieldedColor);
    }

    private static void bomber2SetTarget()
    {
        setBomber2BombTarget();
        if (Bomber2.bomber2 == null || Bomber2.bomber2 != LocalPlayer.Control) return;
        Bomber2.currentTarget = setTarget();
        if (Bomber2.hasBomb == null) setPlayerOutline(Bomber2.currentTarget, Bomber2.color);
    }

    private static void setBomber2BombTarget()
    {
        if (Bomber2.bomber2 == null || Bomber2.hasBomb != LocalPlayer.Control) return;
        Bomber2.currentBombTarget = setTarget();
        //        if (Bomber2.hasBomb != null) setPlayerOutline(Bomber2.currentBombTarget, Bomber2.color);
    }

    private static void bodyGuardSetTarget()
    {
        if (BodyGuard.bodyguard == null || BodyGuard.bodyguard != LocalPlayer.Control) return;
        BodyGuard.currentTarget = setTarget();
        if (!BodyGuard.usedGuard) setPlayerOutline(Medic.currentTarget, Medic.shieldedColor);
    }

    private static void werewolfSetTarget()
    {
        if (Werewolf.werewolf == null || Werewolf.werewolf != LocalPlayer.Control) return;
        Werewolf.currentTarget = setTarget();
    }

    private static void blackMailerSetTarget()
    {
        if (Blackmailer.blackmailer == null || Blackmailer.blackmailer != LocalPlayer.Control) return;
        Blackmailer.currentTarget = setTarget();
        setPlayerOutline(Medic.currentTarget, Blackmailer.blackmailedColor);
    }

    private static void shifterSetTarget()
    {
        if (Shifter.shifter == null || Shifter.shifter != LocalPlayer.Control) return;
        Shifter.currentTarget = setTarget();
        if (Shifter.futureShift == null) setPlayerOutline(Shifter.currentTarget, Color.yellow);
    }


    private static void morphlingSetTarget()
    {
        if (Morphling.morphling == null || Morphling.morphling != LocalPlayer.Control) return;
        Morphling.currentTarget = setTarget();
        setPlayerOutline(Morphling.currentTarget, Morphling.color);
    }

    private static void privateInvestigatorSetTarget()
    {
        if (PrivateInvestigator.privateInvestigator == null ||
            PrivateInvestigator.privateInvestigator != LocalPlayer.Control) return;
        PrivateInvestigator.currentTarget = setTarget();
        setPlayerOutline(PrivateInvestigator.currentTarget, PrivateInvestigator.color);
    }

    private static void sheriffSetTarget()
    {
        if (Sheriff.sheriff == null || Sheriff.sheriff != LocalPlayer.Control) return;
        Sheriff.currentTarget = setTarget();
        setPlayerOutline(Sheriff.currentTarget, Sheriff.color);
    }

    private static void deputySetTarget()
    {
        if (Deputy.deputy == null || Deputy.deputy != LocalPlayer.Control) return;
        Deputy.currentTarget = setTarget();
        setPlayerOutline(Deputy.currentTarget, Deputy.color);
    }

    public static void deputyCheckPromotion(bool isMeeting = false)
    {
        // If LocalPlayer is Deputy, the Sheriff is disconnected and Deputy promotion is enabled, then trigger promotion
        if (Deputy.deputy == null || Deputy.deputy != LocalPlayer.Control) return;
        if (Deputy.promotesToSheriff == 0 || Deputy.deputy.Data.IsDead == true ||
            (Deputy.promotesToSheriff == 2 && !isMeeting)) return;
        if (Sheriff.sheriff == null || Sheriff.sheriff?.Data?.Disconnected == true || Sheriff.sheriff.Data.IsDead)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.DeputyPromotes, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.deputyPromotes();
        }
    }

    private static void trackerSetTarget()
    {
        if (Tracker.tracker == null || Tracker.tracker != LocalPlayer.Control) return;
        Tracker.currentTarget = setTarget();
        if (!Tracker.usedTracker) setPlayerOutline(Tracker.currentTarget, Tracker.color);
    }

    private static void detectiveUpdateFootPrints()
    {
        if (Detective.detective == null || Detective.detective != LocalPlayer.Control) return;

        Detective.timer -= Time.fixedDeltaTime;
        if (Detective.timer <= 0f)
        {
            Detective.timer = Detective.footprintIntervall;
            foreach (PlayerControl player in AllPlayers)
                if (player != null && player != LocalPlayer.Control && !player.Data.IsDead &&
                    !player.inVent)
                    FootprintHolder.Instance.MakeFootprint(player);
        }
    }

    private static void vampireSetTarget()
    {
        if (Vampire.vampire == null || Vampire.vampire != LocalPlayer.Control) return;

        PlayerControl target = null;
        if (Spy.spy != null || Sidekick.wasSpy || Jackal.wasSpy)
        {
            if (Spy.impostorsCanKillAnyone)
                target = setTarget(false, true);
            else
                target = setTarget(true, true,
                [
                    Spy.spy, Sidekick.wasTeamRed ? Sidekick.sidekick : null,
                    Jackal.wasTeamRed ? Jackal.jackal : null
                ]);
        }
        else
        {
            target = setTarget(true, true,
                [Sidekick.wasImpostor ? Sidekick.sidekick : null, Jackal.wasImpostor ? Jackal.jackal : null]);
        }

        var targetNearGarlic = false;
        if (target != null)
            foreach (var garlic in Garlic.garlics)
                if (Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f)
                    targetNearGarlic = true;
        Vampire.targetNearGarlic = targetNearGarlic;
        Vampire.currentTarget = target;
        setPlayerOutline(Vampire.currentTarget, Vampire.color);
    }

    private static void jackalSetTarget()
    {
        if (Jackal.jackal == null || Jackal.jackal != LocalPlayer.Control) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (Jackal.canCreateSidekickFromImpostor)
            // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
            if (Sidekick.sidekick != null)
                untargetablePlayers.Add(Sidekick.sidekick);
        if (Jackal.jackal != null && Jackal.isInvisable) untargetablePlayers.Add(Jackal.jackal);
        if (Mini.mini != null && !Mini.isGrownUp())
            untargetablePlayers.Add(Mini.mini); // Exclude Jackal from targeting the Mini unless it has grown up
        Jackal.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        setPlayerOutline(Jackal.currentTarget, Palette.ImpostorRed);
    }

    private static void sidekickSetTarget()
    {
        if (Sidekick.sidekick == null || Sidekick.sidekick != LocalPlayer.Control) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
        if (Mini.mini != null && !Mini.isGrownUp())
            untargetablePlayers.Add(Mini.mini); // Exclude Sidekick from targeting the Mini unless it has grown up
        Sidekick.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        if (Sidekick.canKill) setPlayerOutline(Sidekick.currentTarget, Palette.ImpostorRed);
    }

    private static void sidekickCheckPromotion()
    {
        // If LocalPlayer is Sidekick, the Jackal is disconnected and Sidekick promotion is enabled, then trigger promotion
        if (Sidekick.sidekick == null || Sidekick.sidekick != LocalPlayer.Control) return;
        if (Sidekick.sidekick.Data.IsDead == true || !Sidekick.promotesToJackal) return;
        if (Jackal.jackal == null || Jackal.jackal?.Data?.Disconnected == true)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.SidekickPromotes, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.sidekickPromotes();
        }
    }

    private static void cultistSetFollower()
    {
        if (Cultist.cultist == null || Cultist.cultist != LocalPlayer.Control) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (Jackal.jackal != null && Jackal.isInvisable) untargetablePlayers.Add(Jackal.jackal);
        if (Mini.mini != null && !Mini.isGrownUp())
            untargetablePlayers.Add(Mini.mini); // Exclude Jackal from targeting the Mini unless it has grown up
        Cultist.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        //        Cultist.currentFollower = setTarget(untargetablePlayers: untargetablePlayers);
        //        setPlayerOutline(Cultist.currentTarget, Palette.ImpostorRed);
    }

    private static void eraserSetTarget()
    {
        if (Eraser.eraser == null || Eraser.eraser != LocalPlayer.Control) return;

        List<PlayerControl> untargetables = [];
        if (Spy.spy != null) untargetables.Add(Spy.spy);
        if (Sidekick.wasTeamRed) untargetables.Add(Sidekick.sidekick);
        if (Jackal.wasTeamRed) untargetables.Add(Jackal.jackal);
        Eraser.currentTarget = setTarget(!Eraser.canEraseAnyone, untargetablePlayers: Eraser.canEraseAnyone
            ?
            [
            ]
            : untargetables);
        setPlayerOutline(Eraser.currentTarget, Eraser.color);
    }

    private static void deputyUpdate()
    {
        if (LocalPlayer.Control == null ||
            !Deputy.handcuffedKnows.ContainsKey(LocalPlayer.PlayerId)) return;

        if (Deputy.handcuffedKnows[LocalPlayer.PlayerId] <= 0)
        {
            Deputy.handcuffedKnows.Remove(LocalPlayer.PlayerId);
            // Resets the buttons
            Deputy.setHandcuffedKnows(false);

            // Ghost info
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable);
            writer.Write(LocalPlayer.PlayerId);
            writer.Write((byte)RPCProcedure.GhostInfoTypes.HandcuffOver);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    private static void engineerUpdate()
    {
        var jackalHighlight = Engineer.highlightForTeamJackal && (LocalPlayer.Control == Jackal.jackal ||
                                                                  LocalPlayer.Control ==
                                                                  Sidekick.sidekick);
        var impostorHighlight = Engineer.highlightForImpostors && LocalPlayer.Data.Role.IsImpostor;
        if ((jackalHighlight || impostorHighlight) && MapUtilities.CachedShipStatus?.AllVents != null)
            foreach (var vent in MapUtilities.CachedShipStatus.AllVents)
                try
                {
                    if (vent?.myRend?.material != null)
                    {
                        if (Engineer.engineer != null && Engineer.engineer.inVent)
                        {
                            vent.myRend.material.SetFloat("_Outline", 1f);
                            vent.myRend.material.SetColor("_OutlineColor", Engineer.color);
                        }
                        else if (vent.myRend.material.GetColor("_AddColor") != Color.red)
                        {
                            vent.myRend.material.SetFloat("_Outline", 0);
                        }
                    }
                }
                catch
                {
                }
    }

    private static void impostorSetTarget()
    {
        if (!LocalPlayer.Data.Role.IsImpostor || !LocalPlayer.Control.CanMove ||
            LocalPlayer.Data.IsDead)
        {
            // !isImpostor || !canMove || isDead
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            return;
        }

        PlayerControl target = null;
        if (Spy.spy != null || Sidekick.wasSpy || Jackal.wasSpy)
        {
            if (Spy.impostorsCanKillAnyone)
                target = setTarget(false, true);
            else
                target = setTarget(true, true,
                [
                    Spy.spy, Sidekick.wasTeamRed ? Sidekick.sidekick : null,
                    Jackal.wasTeamRed ? Jackal.jackal : null
                ]);
        }
        else
        {
            target = setTarget(true, true,
                [Sidekick.wasImpostor ? Sidekick.sidekick : null, Jackal.wasImpostor ? Jackal.jackal : null]);
        }

        FastDestroyableSingleton<HudManager>.Instance.KillButton
            .SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
    }

    private static void warlockSetTarget()
    {
        if (Warlock.warlock == null || Warlock.warlock != LocalPlayer.Control) return;
        if (Warlock.curseVictim != null && (Warlock.curseVictim.Data.Disconnected || Warlock.curseVictim.Data.IsDead))
            // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
            Warlock.resetCurse();
        if (Warlock.curseVictim == null)
        {
            Warlock.currentTarget = setTarget();
            setPlayerOutline(Warlock.currentTarget, Warlock.color);
        }
        else
        {
            Warlock.curseVictimTarget = setTarget(targetingPlayer: Warlock.curseVictim);
            setPlayerOutline(Warlock.curseVictimTarget, Warlock.color);
        }
    }

    private static void swooperUpdate()
    {
        if (Jackal.isInvisable && Jackal.swoopTimer <= 0 && Jackal.jackal == LocalPlayer.Control)
        {
            var invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.SetSwoop, SendOption.Reliable);
            invisibleWriter.Write(Jackal.jackal.PlayerId);
            invisibleWriter.Write(byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
            RPCProcedure.setSwoop(Jackal.jackal.PlayerId, byte.MaxValue);
        } /*
        if (Jackal.jackal != null && Jackal.canSwoop){
            {
            MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.Control.NetId, (byte)CustomRPC.SetSwooper, Hazel.SendOption.Reliable, -1);
            invisibleWriter.Write(Jackal.jackal.PlayerId);
            invisibleWriter.Write(byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
            RPCProcedure.setSwooper(Jackal.jackal.PlayerId);
        }
        }#1#
        /*
        if ((Swooper.swooper = Jackal.jackal) && Jackal.canSwoop2){
            foreach (Control player in CachedPlayer.AllPlayers) {
            Swooper.swooper = Jackal.jackal;
            }
        }
        #1#
    }

    private static void ninjaUpdate()
    {
        if (Ninja.isInvisble && Ninja.invisibleTimer <= 0 && Ninja.ninja == LocalPlayer.Control)
        {
            var invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.SetInvisible, SendOption.Reliable);
            invisibleWriter.Write(Ninja.ninja.PlayerId);
            invisibleWriter.Write(byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
            RPCProcedure.setInvisible(Ninja.ninja.PlayerId, byte.MaxValue);
        }

        if (Ninja.arrow?.arrow != null)
        {
            if (Ninja.ninja == null || Ninja.ninja != LocalPlayer.Control || !Ninja.knowsTargetLocation)
            {
                Ninja.arrow.arrow.SetActive(false);
                return;
            }

            if (Ninja.ninjaMarked != null && !LocalPlayer.Data.IsDead)
            {
                bool trackedOnMap = !Ninja.ninjaMarked.Data.IsDead;
                Vector3 position = Ninja.ninjaMarked.transform.position;
                if (!trackedOnMap)
                {
                    // Check for dead body
                    var body = Object.FindObjectsOfType<DeadBody>()
                        .FirstOrDefault(b => b.ParentId == Ninja.ninjaMarked.PlayerId);
                    if (body != null)
                    {
                        trackedOnMap = true;
                        position = body.transform.position;
                    }
                }

                Ninja.arrow.Update(position);
                Ninja.arrow.arrow.SetActive(trackedOnMap);
            }
            else
            {
                Ninja.arrow.arrow.SetActive(false);
            }
        }
    }

    private static void trackerUpdate()
    {
        // Handle player tracking
        if (Tracker.arrow?.arrow != null)
        {
            if (Tracker.tracker == null || LocalPlayer.Control != Tracker.tracker)
            {
                Tracker.arrow.arrow.SetActive(false);
                return;
            }

            if (Tracker.tracker != null && Tracker.tracked != null &&
                LocalPlayer.Control == Tracker.tracker && !Tracker.tracker.Data.IsDead)
            {
                Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

                if (Tracker.timeUntilUpdate <= 0f)
                {
                    bool trackedOnMap = !Tracker.tracked.Data.IsDead;
                    Vector3 position = Tracker.tracked.transform.position;
                    if (!trackedOnMap)
                    {
                        // Check for dead body
                        var body = Object.FindObjectsOfType<DeadBody>()
                            .FirstOrDefault(b => b.ParentId == Tracker.tracked.PlayerId);
                        if (body != null)
                        {
                            trackedOnMap = true;
                            position = body.transform.position;
                        }
                    }

                    Tracker.arrow.Update(position);
                    Tracker.arrow.arrow.SetActive(trackedOnMap);
                    Tracker.timeUntilUpdate = Tracker.updateIntervall;
                }
                else
                {
                    Tracker.arrow.Update();
                }
            }
        }

        // Handle corpses tracking
        if (Tracker.tracker != null && Tracker.tracker == LocalPlayer.Control &&
            Tracker.corpsesTrackingTimer >= 0f && !Tracker.tracker.Data.IsDead)
        {
            bool arrowsCountChanged = Tracker.localArrows.Count != Tracker.deadBodyPositions.Count();
            var index = 0;

            if (arrowsCountChanged)
            {
                foreach (Arrow arrow in Tracker.localArrows) Object.Destroy(arrow.arrow);
                Tracker.localArrows = new List<Arrow>();
            }

            foreach (Vector3 position in Tracker.deadBodyPositions)
            {
                if (arrowsCountChanged)
                {
                    Tracker.localArrows.Add(new Arrow(Tracker.color));
                    Tracker.localArrows[index].arrow.SetActive(true);
                }

                if (Tracker.localArrows[index] != null) Tracker.localArrows[index].Update(position);
                index++;
            }
        }
        else if (Tracker.localArrows.Count > 0)
        {
            foreach (Arrow arrow in Tracker.localArrows) Object.Destroy(arrow.arrow);
            Tracker.localArrows = new List<Arrow>();
        }
    }

    public static void playerSizeUpdate(PlayerControl p)
    {
        // Set default player size
        var collider = p.Collider.CastFast<CircleCollider2D>();

        p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        collider.radius = Mini.defaultColliderRadius;
        collider.offset = Mini.defaultColliderOffset * Vector2.down;

        // Set adapted player size to Mini and Morphling
        if (Mini.mini == null || Helpers.isCamoComms() || Camouflager.camouflageTimer > 0f ||
            Helpers.MushroomSabotageActive() || (Mini.mini == Morphling.morphling && Morphling.morphTimer > 0)) return;

        float growingProgress = Mini.growingProgress();
        var scale = (growingProgress * 0.35f) + 0.35f;
        var correctedColliderRadius =
            Mini.defaultColliderRadius * 0.7f /
            scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

        if (p == Mini.mini)
        {
            p.transform.localScale = new Vector3(scale, scale, 1f);
            collider.radius = correctedColliderRadius;
        }

        if (Morphling.morphling != null && p == Morphling.morphling && Morphling.morphTarget == Mini.mini &&
            Morphling.morphTimer > 0f)
        {
            p.transform.localScale = new Vector3(scale, scale, 1f);
            collider.radius = correctedColliderRadius;
        }
    }

    public static void updatePlayerInfo()
    {
        var colorBlindTextMeetingInitialLocalPos = new Vector3(0.3384f, -0.16666f, -0.01f);
        var colorBlindTextMeetingInitialLocalScale = new Vector3(0.9f, 1f, 1f);
        foreach (PlayerControl p in AllPlayers)
        {
            // Colorblind Text in Meeting
            var playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
            if (playerVoteArea != null && playerVoteArea.ColorBlindName.gameObject.active)
            {
                playerVoteArea.ColorBlindName.transform.localPosition =
                    colorBlindTextMeetingInitialLocalPos + new Vector3(0f, 0.4f, 0f);
                playerVoteArea.ColorBlindName.transform.localScale = colorBlindTextMeetingInitialLocalScale * 0.8f;
            }

            // Colorblind Text During the round
            if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText &&
                p.cosmetics.colorBlindText.gameObject.active)
                p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);

            // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus
            p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f);

            //Snitch See Roles
            var snitchFlag = false;
            if (Snitch.snitch != null && Snitch.seeInMeeting && Snitch.canSeeRoles && !Snitch.snitch.Data.IsDead)
            {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
                var numberOfTasks = playerTotal - playerCompleted;
                var completedSnitch = Snitch.seeInMeeting && LocalPlayer.Control == Snitch.snitch &&
                                      numberOfTasks == 0;
                var forImpTeam = p.Data.Role.IsImpostor;
                var forKillerTeam = Snitch.Team == Snitch.includeNeutralTeam.KillNeutral && Helpers.isKiller(p);
                var forEvilTeam = Snitch.Team == Snitch.includeNeutralTeam.EvilNeutral && Helpers.isEvil(p);
                var forNeutraTeam = Snitch.Team == Snitch.includeNeutralTeam.AllNeutral && Helpers.isNeutral(p);
                snitchFlag = completedSnitch && p == (forImpTeam || forKillerTeam || forEvilTeam || forNeutraTeam);
            }

            if (snitchFlag ||
                (Lawyer.lawyerKnowsRole && LocalPlayer.Control == Lawyer.lawyer && p == Lawyer.target) ||
                p == LocalPlayer.Control || LocalPlayer.Data.IsDead ||
                (LocalPlayer.Control == Slueth.slueth &&
                 Slueth.reported.Any(x => x.PlayerId == p.PlayerId)) ||
                (MapOptions.impostorSeeRoles && Spy.spy == null && LocalPlayer.Data.Role.IsImpostor &&
                 !LocalPlayer.Data.IsDead && p == (p.Data.Role.IsImpostor && !p.Data.IsDead)) ||
                (LocalPlayer.Control == Poucher.poucher &&
                 Poucher.killed.Any(x => x.PlayerId == p.PlayerId)))
            {
                var playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                var playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TextMeshPro>() : null;
                if (playerInfo == null)
                {
                    playerInfo = Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                    playerInfo.transform.localPosition += Vector3.up * 0.225f;
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                    playerInfo.color = playerInfo.color.SetAlpha(1f);
                }

                var meetingInfoTransform = playerVoteArea != null
                    ? playerVoteArea.NameText.transform.parent.FindChild("Info")
                    : null;
                var meetingInfo = meetingInfoTransform != null
                    ? meetingInfoTransform.GetComponent<TextMeshPro>()
                    : null;
                if (meetingInfo == null && playerVoteArea != null)
                {
                    meetingInfo = Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                    meetingInfo.transform.localPosition += Vector3.down * 0.2f;
                    meetingInfo.fontSize *= 0.60f;
                    meetingInfo.gameObject.name = "Info";
                }

                // Set player name higher to align in middle
                if (meetingInfo != null && playerVoteArea != null)
                {
                    var playerName = playerVoteArea.NameText;
                    playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                }

                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                var roleNames = RoleInfo.GetRolesString(p, true, false);
                var roleText = RoleInfo.GetRolesString(p, true, MapOptions.ghostsSeeModifier);
                var taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                var playerInfoText = "";
                var meetingInfoText = "";
                if (p == LocalPlayer.Control || (MapOptions.impostorSeeRoles && Spy.spy == null &&
                                                              LocalPlayer.Data.Role.IsImpostor &&
                                                              !LocalPlayer.Data.IsDead &&
                                                              p == (p.Data.Role.IsImpostor && !p.Data.IsDead)))
                {
                    if (p.Data.IsDead) roleNames = roleText;
                    playerInfoText = $"{roleNames}";
                    if (p == Swapper.swapper)
                        playerInfoText = $"{roleNames}" + Helpers.cs(Swapper.color, $" ({Swapper.charges})");
                    if (HudManager.Instance.TaskPanel != null)
                    {
                        var tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP")
                            .GetComponent<TextMeshPro>();
                        tabText.SetText($"Tasks {taskInfo}");
                    }

                    meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                }
                else if (MapOptions.ghostsSeeRoles && MapOptions.ghostsSeeInformation)
                {
                    playerInfoText = $"{roleText} {taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (MapOptions.ghostsSeeInformation)
                {
                    playerInfoText = $"{taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (MapOptions.ghostsSeeRoles || (Lawyer.lawyerKnowsRole &&
                                                          LocalPlayer.Control == Lawyer.lawyer &&
                                                          p == Lawyer.target))
                {
                    playerInfoText = $"{roleText}";
                    meetingInfoText = playerInfoText;
                }

                playerInfo.text = playerInfoText;
                playerInfo.gameObject.SetActive(p.Visible);
                if (meetingInfo != null)
                    meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results
                        ? ""
                        : meetingInfoText;
            }
        }
    }

    public static void securityGuardSetTarget()
    {
        if (SecurityGuard.securityGuard == null || SecurityGuard.securityGuard != LocalPlayer.Control ||
            MapUtilities.CachedShipStatus == null || MapUtilities.CachedShipStatus.AllVents == null) return;

        Vent target = null;
        var truePosition = LocalPlayer.Control.GetTruePosition();
        var closestDistance = float.MaxValue;
        for (var i = 0; i < MapUtilities.CachedShipStatus.AllVents.Length; i++)
        {
            var vent = MapUtilities.CachedShipStatus.AllVents[i];
            if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") ||
                vent.gameObject.name.StartsWith("SealedVent_") ||
                vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 9) continue; // cannot seal submergeds exit only vent!
            var distance = Vector2.Distance(vent.transform.position, truePosition);
            if (distance <= vent.UsableDistance && distance < closestDistance)
            {
                closestDistance = distance;
                target = vent;
            }
        }

        SecurityGuard.ventTarget = target;
    }

    public static void securityGuardUpdate()
    {
        if (SecurityGuard.securityGuard == null || LocalPlayer.Control != SecurityGuard.securityGuard ||
            SecurityGuard.securityGuard.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(SecurityGuard.securityGuard.Data);
        if (playerCompleted == SecurityGuard.rechargedTasks)
        {
            SecurityGuard.rechargedTasks += SecurityGuard.rechargeTasksNumber;
            if (SecurityGuard.maxCharges > SecurityGuard.charges) SecurityGuard.charges++;
        }
    }

    public static void arsonistSetTarget()
    {
        if (Arsonist.arsonist == null || Arsonist.arsonist != LocalPlayer.Control) return;
        List<PlayerControl> untargetables;
        if (Arsonist.douseTarget != null)
        {
            untargetables = [];
            foreach (var cachedPlayer in AllPlayers)
                if (cachedPlayer.PlayerId != Arsonist.douseTarget.PlayerId)
                    untargetables.Add(cachedPlayer);
        }
        else
        {
            untargetables = Arsonist.dousedPlayers;
        }

        Arsonist.currentTarget = setTarget(untargetablePlayers: untargetables);
        if (Arsonist.currentTarget != null) setPlayerOutline(Arsonist.currentTarget, Arsonist.color);
    }

    private static void snitchUpdate()
    {
        if (Snitch.localArrows == null) return;

        foreach (Arrow arrow in Snitch.localArrows) arrow.arrow.SetActive(false);

        if (Snitch.snitch == null || Snitch.snitch.Data.IsDead) return;

        var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
        var numberOfTasks = playerTotal - playerCompleted;

        var snitchIsDead = Snitch.snitch.Data.IsDead;
        var local = LocalPlayer.Control;

        var forImpTeam = local.Data.Role.IsImpostor;
        var forKillerTeam = Snitch.Team == Snitch.includeNeutralTeam.KillNeutral && Helpers.isKiller(local);
        var forEvilTeam = Snitch.Team == Snitch.includeNeutralTeam.EvilNeutral && Helpers.isEvil(local);
        var forNeutraTeam = Snitch.Team == Snitch.includeNeutralTeam.AllNeutral && Helpers.isNeutral(local);
        if (numberOfTasks <= Snitch.taskCountForReveal) Snitch.isRevealed = true;

        if (numberOfTasks <= Snitch.taskCountForReveal && (forImpTeam || forKillerTeam || forEvilTeam || forNeutraTeam))
        {
            if (Snitch.localArrows.Count == 0) Snitch.localArrows.Add(new Arrow(Snitch.color));
            if (Snitch.localArrows.Count != 0 && Snitch.localArrows[0] != null)
            {
                Snitch.localArrows[0].arrow.SetActive(true);
                Snitch.localArrows[0].Update(Snitch.snitch.transform.position);
            }
        }
        else if (local == Snitch.snitch && numberOfTasks == 0 && !snitchIsDead)
        {
            var arrowIndex = 0;
            foreach (PlayerControl p in AllPlayers)
            {
                var arrowForImp = p.Data.Role.IsImpostor;
                if (Mimic.mimic == p) arrowForImp = true;
                var arrowForKillerTeam = Snitch.Team == Snitch.includeNeutralTeam.KillNeutral && Helpers.isKiller(p);
                var arrowForEvilTeam = Snitch.Team == Snitch.includeNeutralTeam.EvilNeutral && Helpers.isEvil(p);
                var arrowForNeutraTeam = Snitch.Team == Snitch.includeNeutralTeam.AllNeutral && Helpers.isNeutral(p);
                var targetsRole = RoleInfo.getRoleInfoForPlayer(p, false).FirstOrDefault();

                if (!p.Data.IsDead && (arrowForImp || arrowForKillerTeam || arrowForEvilTeam || arrowForNeutraTeam))
                {
                    if (arrowIndex >= Snitch.localArrows.Count) Snitch.localArrows.Add(new Arrow(Palette.ImpostorRed));
                    if (arrowIndex < Snitch.localArrows.Count && Snitch.localArrows[arrowIndex] != null)
                    {
                        Snitch.localArrows[arrowIndex].arrow.SetActive(true);
                        if (arrowForImp)
                            Snitch.localArrows[arrowIndex].Update(p.transform.position, Palette.ImpostorRed);
                        else if (arrowForKillerTeam || arrowForEvilTeam || arrowForNeutraTeam)
                            Snitch.localArrows[arrowIndex].Update(p.transform.position,
                                Snitch.teamNeutraUseDifferentArrowColor ? targetsRole.color : Palette.ImpostorRed);
                    }

                    arrowIndex++;
                }
            }
        }
    }

    private static void snitchTextUpdate()
    {
        if (Snitch.localArrows == null) return;
        if (Snitch.snitch == null) return;
        //foreach (Arrow arrow in Snitch.localArrows) arrow.arrow.SetActive(false);
        var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
        var numberOfTasks = playerTotal - playerCompleted;

        var snitchIsDead = Snitch.snitch.Data.IsDead;
        var local = LocalPlayer.Control;

        var forImpTeam = local.Data.Role.IsImpostor;
        var forKillerTeam = Snitch.Team == Snitch.includeNeutralTeam.KillNeutral && Helpers.isKiller(local);
        var forEvilTeam = Snitch.Team == Snitch.includeNeutralTeam.EvilNeutral && Helpers.isEvil(local);
        var forNeutraTeam = Snitch.Team == Snitch.includeNeutralTeam.AllNeutral && Helpers.isNeutral(local);
        if (numberOfTasks <= Snitch.taskCountForReveal && (forImpTeam || forKillerTeam || forEvilTeam || forNeutraTeam))
        {
            if (Snitch.text == null)
            {
                Snitch.text =
                    Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                        FastDestroyableSingleton<HudManager>.Instance.transform);
                Snitch.text.enableWordWrapping = false;
                Snitch.text.transform.localScale = Vector3.one * 0.75f;
                Snitch.text.transform.localPosition += new Vector3(0f, 1.8f, -69f);
                Snitch.text.gameObject.SetActive(true);
            }
            else if (!snitchIsDead)
            {
                Snitch.text.text = "Snitch is alive: " + playerCompleted + "/" + playerTotal;
            }
            else if (snitchIsDead)
            {
                Snitch.text.text = null;
            }
        }
    }

    private static void undertakerDragBodyUpdate()
    {
        if (Undertaker.undertaker == null || Undertaker.undertaker.Data.IsDead) return;
        if (Undertaker.deadBodyDraged != null)
        {
            Vector3 currentPosition = Undertaker.undertaker.transform.position;
            Undertaker.deadBodyDraged.transform.position = currentPosition;
        }
    }

    private static void bountyHunterUpdate()
    {
        if (BountyHunter.bountyHunter == null || LocalPlayer.Control != BountyHunter.bountyHunter) return;

        if (BountyHunter.bountyHunter.Data.IsDead)
        {
            if (BountyHunter.arrow != null || BountyHunter.arrow.arrow != null)
                Object.Destroy(BountyHunter.arrow.arrow);
            BountyHunter.arrow = null;
            if (BountyHunter.cooldownText != null && BountyHunter.cooldownText.gameObject != null)
                Object.Destroy(BountyHunter.cooldownText.gameObject);
            BountyHunter.cooldownText = null;
            BountyHunter.bounty = null;
            foreach (var p in MapOptions.playerIcons.Values)
                if (p != null && p.gameObject != null)
                    p.gameObject.SetActive(false);
            return;
        }

        BountyHunter.arrowUpdateTimer -= Time.fixedDeltaTime;
        BountyHunter.bountyUpdateTimer -= Time.fixedDeltaTime;

        if (BountyHunter.bounty == null || BountyHunter.bountyUpdateTimer <= 0f)
        {
            // Set new bounty
            BountyHunter.bounty = null;
            BountyHunter.arrowUpdateTimer = 0f; // Force arrow to update
            BountyHunter.bountyUpdateTimer = BountyHunter.bountyDuration;
            var possibleTargets = new List<PlayerControl>();
            foreach (PlayerControl p in AllPlayers)
                if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor && p != Spy.spy &&
                    (p != Sidekick.sidekick || !Sidekick.wasTeamRed) && (p != Jackal.jackal || !Jackal.wasTeamRed) &&
                    (p != Mini.mini || Mini.isGrownUp()) && (Lovers.getPartner(BountyHunter.bountyHunter) == null ||
                                                             p != Lovers.getPartner(BountyHunter.bountyHunter)))
                    possibleTargets.Add(p);
            BountyHunter.bounty = possibleTargets[TheOtherUs.rnd.Next(0, possibleTargets.Count)];
            if (BountyHunter.bounty == null) return;

            // Ghost Info
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable);
            writer.Write(LocalPlayer.PlayerId);
            writer.Write((byte)RPCProcedure.GhostInfoTypes.BountyTarget);
            writer.Write(BountyHunter.bounty.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            // Show poolable player
            if (FastDestroyableSingleton<HudManager>.Instance != null &&
                FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
            {
                foreach (var pp in MapOptions.playerIcons.Values) pp.gameObject.SetActive(false);
                if (MapOptions.playerIcons.ContainsKey(BountyHunter.bounty.PlayerId) &&
                    MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
                    MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(true);
            }
        }

        // Hide in meeting
        if (MeetingHud.Instance && MapOptions.playerIcons.ContainsKey(BountyHunter.bounty.PlayerId) &&
            MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
            MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(false);

        // Update Cooldown Text
        if (BountyHunter.cooldownText != null)
        {
            BountyHunter.cooldownText.text = Mathf
                .CeilToInt(Mathf.Clamp(BountyHunter.bountyUpdateTimer, 0, BountyHunter.bountyDuration)).ToString();
            BountyHunter.cooldownText.gameObject.SetActive(!MeetingHud.Instance); // Show if not in meeting
        }

        // Update Arrow
        if (BountyHunter.showArrow && BountyHunter.bounty != null)
        {
            if (BountyHunter.arrow == null) BountyHunter.arrow = new Arrow(Color.red);
            if (BountyHunter.arrowUpdateTimer <= 0f)
            {
                BountyHunter.arrow.Update(BountyHunter.bounty.transform.position);
                BountyHunter.arrowUpdateTimer = BountyHunter.arrowUpdateIntervall;
            }

            BountyHunter.arrow.Update();
        }
    }

    private static void vultureUpdate()
    {
        if (Vulture.vulture == null || LocalPlayer.Control != Vulture.vulture ||
            Vulture.localArrows == null || !Vulture.showArrows) return;
        if (Vulture.vulture.Data.IsDead)
        {
            foreach (Arrow arrow in Vulture.localArrows) Object.Destroy(arrow.arrow);
            Vulture.localArrows = new List<Arrow>();
            return;
        }

        DeadBody[] deadBodies = Object.FindObjectsOfType<DeadBody>();
        var arrowUpdate = Vulture.localArrows.Count != deadBodies.Count();
        var index = 0;

        if (arrowUpdate)
        {
            foreach (Arrow arrow in Vulture.localArrows) Object.Destroy(arrow.arrow);
            Vulture.localArrows = new List<Arrow>();
        }

        foreach (var db in deadBodies)
        {
            if (arrowUpdate)
            {
                Vulture.localArrows.Add(new Arrow(Color.blue));
                Vulture.localArrows[index].arrow.SetActive(true);
            }

            if (Vulture.localArrows[index] != null) Vulture.localArrows[index].Update(db.transform.position);
            index++;
        }
    }

    private static void amnisiacUpdate()
    {
        if (Amnisiac.amnisiac == null || LocalPlayer.Control != Amnisiac.amnisiac ||
            Amnisiac.localArrows == null || !Amnisiac.showArrows) return;
        if (Amnisiac.amnisiac.Data.IsDead)
        {
            foreach (Arrow arrow in Amnisiac.localArrows) Object.Destroy(arrow.arrow);
            Amnisiac.localArrows = new List<Arrow>();
            return;
        }

        DeadBody[] deadBodies = Object.FindObjectsOfType<DeadBody>();
        var arrowUpdate = Amnisiac.localArrows.Count != deadBodies.Count();
        var index = 0;

        if (arrowUpdate)
        {
            foreach (Arrow arrow in Amnisiac.localArrows) Object.Destroy(arrow.arrow);
            Amnisiac.localArrows = new List<Arrow>();
        }

        foreach (var db in deadBodies)
        {
            if (arrowUpdate)
            {
                Amnisiac.localArrows.Add(new Arrow(Color.blue));
                Amnisiac.localArrows[index].arrow.SetActive(true);
            }

            if (Amnisiac.localArrows[index] != null) Amnisiac.localArrows[index].Update(db.transform.position);
            index++;
        }
    }

    private static void radarUpdate()
    {
        if (Radar.radar == null || LocalPlayer.Control != Radar.radar || Radar.localArrows == null ||
            !Radar.showArrows) return;
        if (Radar.radar.Data.IsDead)
        {
            foreach (Arrow arrow in Radar.localArrows) Object.Destroy(arrow.arrow);
            Radar.localArrows = new List<Arrow>();
            return;
        }

        var arrowUpdate = true;
        var index = 0;

        if (arrowUpdate && !LocalPlayer.Data.IsDead)
        {
            foreach (Arrow arrow in Radar.localArrows) Object.Destroy(arrow.arrow);
            Radar.ClosestPlayer = GetClosestPlayer(PlayerControl.LocalPlayer,
                PlayerControl.AllPlayerControls.ToArray().ToList());
            Radar.localArrows = new List<Arrow>();
        }


        foreach (PlayerControl player in AllPlayers)
        {
            if (arrowUpdate && !LocalPlayer.Data.IsDead)
            {
                Radar.localArrows.Add(new Arrow(Radar.color));
                Radar.localArrows[index].arrow.SetActive(true);
            }

            if (Radar.localArrows[index] != null)
                Radar.localArrows[index].Update(Radar.ClosestPlayer.transform.position);
            index++;
        }
    }

    public static PlayerControl GetClosestPlayer(PlayerControl refPlayer, List<PlayerControl> AllPlayers)
    {
        var num = double.MaxValue;
        var refPosition = refPlayer.GetTruePosition();
        PlayerControl result = null;
        foreach (var player in AllPlayers)
        {
            if (player.Data.IsDead || player.PlayerId == refPlayer.PlayerId || !player.Collider.enabled) continue;
            var playerPosition = player.GetTruePosition();
            var distBetweenPlayers = Vector2.Distance(refPosition, playerPosition);
            var isClosest = distBetweenPlayers < num;
            if (!isClosest) continue;
            var vector = playerPosition - refPosition;
            //           if (PhysicsHelpers.AnyNonTriggersBetween(
            //                refPosition, vector.normalized, vector.magnitude, Constants.ShipAndObjectsMask
            //            )) continue;
            num = distBetweenPlayers;
            result = player;
        }

        return result;
    }

    public static PlayerControl GetClosestPlayer(PlayerControl refplayer)
    {
        return GetClosestPlayer(refplayer, PlayerControl.AllPlayerControls.ToArray().ToList());
    }

    public static void SetTarget(
        ref PlayerControl closestPlayer,
        KillButton button,
        float maxDistance = float.NaN,
        List<PlayerControl> targets = null
    )
    {
        if (!button.isActiveAndEnabled) return;

        button.SetTarget(
            SetClosestPlayer(ref closestPlayer, maxDistance, targets)
        );
    }

    public static PlayerControl SetClosestPlayer(
        ref PlayerControl closestPlayer,
        float maxDistance = float.NaN,
        List<PlayerControl> targets = null
    )
    {
        if (float.IsNaN(maxDistance))
            maxDistance =
                GameOptionsData.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
        var player = GetClosestPlayer(
            PlayerControl.LocalPlayer,
            targets ?? PlayerControl.AllPlayerControls.ToArray().ToList()
        );
        var closeEnough = player == null || GetDistBetweenPlayers(PlayerControl.LocalPlayer, player) < maxDistance;
        return closestPlayer = closeEnough ? player : null;
    }

    public static double GetDistBetweenPlayers(PlayerControl player, PlayerControl refplayer)
    {
        var truePosition = refplayer.GetTruePosition();
        var truePosition2 = player.GetTruePosition();
        return Vector2.Distance(truePosition, truePosition2);
    }

    public static void mediumSetTarget()
    {
        if (Medium.medium == null || Medium.medium != LocalPlayer.Control || Medium.medium.Data.IsDead ||
            Medium.deadBodies == null || MapUtilities.CachedShipStatus?.AllVents == null) return;

        DeadPlayer target = null;
        var truePosition = LocalPlayer.Control.GetTruePosition();
        var closestDistance = float.MaxValue;
        var usableDistance = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault().UsableDistance;
        foreach ((DeadPlayer dp, Vector3 ps) in Medium.deadBodies)
        {
            var distance = Vector2.Distance(ps, truePosition);
            if (distance <= usableDistance && distance < closestDistance)
            {
                closestDistance = distance;
                target = dp;
            }
        }

        Medium.target = target;
    }

    private static void morphlingAndCamouflagerUpdate()
    {
        var mushRoomSaboIsActive = Helpers.MushroomSabotageActive();
        if (!mushroomSaboWasActive) mushroomSaboWasActive = mushRoomSaboIsActive;

        if (Helpers.isCamoComms() && !Helpers.isActiveCamoComms())
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.CamouflagerCamouflage, SendOption.Reliable);
            writer.Write(0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.camouflagerCamouflage(0);
        }

        float oldCamouflageTimer = Camouflager.camouflageTimer;
        float oldMorphTimer = Morphling.morphTimer;
        Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
        Morphling.morphTimer = Mathf.Max(0f, Morphling.morphTimer - Time.fixedDeltaTime);

        if (mushRoomSaboIsActive) return;
        if (Helpers.isCamoComms()) return;
        if (Helpers.wasActiveCamoComms() && Camouflager.camouflageTimer <= 0f) Helpers.camoReset();

        // Camouflage reset and set Morphling look if necessary
        if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f)
        {
            Camouflager.resetCamouflage();
            Helpers.camoReset();
            if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null)
            {
                PlayerControl target = Morphling.morphTarget;
                Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                    target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId,
                    target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
            }
        }

        // If the MushRoomSabotage ends while Morph is still active set the Morphlings look to the target's look
        if (mushroomSaboWasActive)
        {
            if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null)
            {
                PlayerControl target = Morphling.morphTarget;
                Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                    target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId,
                    target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
            }

            if (Camouflager.camouflageTimer > 0)
                foreach (PlayerControl player in AllPlayers)
                    player.setLook("", 6, "", "", "", "");
        }

        // Morphling reset (only if camouflage is inactive)
        if (Camouflager.camouflageTimer <= 0f && oldMorphTimer > 0f && Morphling.morphTimer <= 0f &&
            Morphling.morphling != null)
            Morphling.resetMorph();
        mushroomSaboWasActive = false;
    }

    public static void lawyerUpdate()
    {
        if (Lawyer.lawyer == null || Lawyer.lawyer != LocalPlayer.Control) return;

        // Promote to Pursuer
        if (Lawyer.target != null && Lawyer.target.Data.Disconnected && !Lawyer.lawyer.Data.IsDead)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.LawyerPromotesToPursuer, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.lawyerPromotesToPursuer();
        }
    }

    public static void hackerUpdate()
    {
        if (Hacker.hacker == null || LocalPlayer.Control != Hacker.hacker ||
            Hacker.hacker.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(Hacker.hacker.Data);
        if (playerCompleted == Hacker.rechargedTasks)
        {
            Hacker.rechargedTasks += Hacker.rechargeTasksNumber;
            if (Hacker.toolsNumber > Hacker.chargesVitals) Hacker.chargesVitals++;
            if (Hacker.toolsNumber > Hacker.chargesAdminTable) Hacker.chargesAdminTable++;
        }
    }

    // For swapper swap charges        
    public static void swapperUpdate()
    {
        if (Swapper.swapper == null || LocalPlayer.Control != Swapper.swapper ||
            LocalPlayer.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(LocalPlayer.Data);
        if (playerCompleted == Swapper.rechargedTasks)
        {
            Swapper.rechargedTasks += Swapper.rechargeTasksNumber;
            Swapper.charges++;
        }
    }

    private static void pursuerSetTarget()
    {
        if (Pursuer.pursuer == null || Pursuer.pursuer != LocalPlayer.Control) return;
        Pursuer.target = setTarget();
        setPlayerOutline(Pursuer.target, Pursuer.color);
    }

    private static void witchSetTarget()
    {
        if (Witch.witch == null || Witch.witch != LocalPlayer.Control) return;
        List<PlayerControl> untargetables;
        if (Witch.spellCastingTarget != null)
        {
            untargetables = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => x.PlayerId != Witch.spellCastingTarget.PlayerId)
                .ToList(); // Don't switch the target from the the one you're currently casting a spell on
        }
        else
        {
            untargetables =
            [
            ]; // Also target players that have already been spelled, to hide spells that were blanks/blocked by shields
            if (Spy.spy != null && !Witch.canSpellAnyone) untargetables.Add(Spy.spy);
            if (Sidekick.wasTeamRed && !Witch.canSpellAnyone) untargetables.Add(Sidekick.sidekick);
            if (Jackal.wasTeamRed && !Witch.canSpellAnyone) untargetables.Add(Jackal.jackal);
        }

        Witch.currentTarget = setTarget(!Witch.canSpellAnyone, untargetablePlayers: untargetables);
        setPlayerOutline(Witch.currentTarget, Witch.color);
    }

    private static void ninjaSetTarget()
    {
        if (Ninja.ninja == null || Ninja.ninja != LocalPlayer.Control) return;
        List<PlayerControl> untargetables = [];
        if (Spy.spy != null && !Spy.impostorsCanKillAnyone) untargetables.Add(Spy.spy);
        if (Mini.mini != null && !Mini.isGrownUp()) untargetables.Add(Mini.mini);
        if (Sidekick.wasTeamRed && !Spy.impostorsCanKillAnyone) untargetables.Add(Sidekick.sidekick);
        if (Jackal.wasTeamRed && !Spy.impostorsCanKillAnyone) untargetables.Add(Jackal.jackal);
        Ninja.currentTarget =
            setTarget(Spy.spy == null || !Spy.impostorsCanKillAnyone, untargetablePlayers: untargetables);
        setPlayerOutline(Ninja.currentTarget, Ninja.color);
    }

    private static void thiefSetTarget()
    {
        if (Thief.thief == null || Thief.thief != LocalPlayer.Control) return;
        List<PlayerControl> untargetables = [];
        if (Mini.mini != null && !Mini.isGrownUp()) untargetables.Add(Mini.mini);
        Thief.currentTarget = setTarget(untargetablePlayers: untargetables);
        setPlayerOutline(Thief.currentTarget, Thief.color);
    }


    private static void baitUpdate()
    {
        if (!Bait.active.Any()) return;

        // Bait report
        foreach (var entry in new Dictionary<DeadPlayer, float>(Bait.active))
        {
            Bait.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
            if (entry.Value <= 0)
            {
                Bait.active.Remove(entry.Key);
                if (entry.Key.killerIfExisting != null &&
                    entry.Key.killerIfExisting.PlayerId == LocalPlayer.PlayerId)
                {
                    Helpers
                        .handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                    Helpers
                        .handleBomber2ExplodeOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                    RPCProcedure.uncheckedCmdReportDeadBody(entry.Key.killerIfExisting.PlayerId,
                        entry.Key.player.PlayerId);

                    var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                        (byte)CustomRPC.UncheckedCmdReportDeadBody, SendOption.Reliable);
                    writer.Write(entry.Key.killerIfExisting.PlayerId);
                    writer.Write(entry.Key.player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }
    }

    private static void bloodyUpdate()
    {
        if (!Bloody.active.Any()) return;
        foreach (var entry in new Dictionary<byte, float>(Bloody.active))
        {
            var player = Helpers.playerById(entry.Key);
            var bloodyPlayer = Helpers.playerById(Bloody.bloodyKillerMap[player.PlayerId]);

            Bloody.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
            if (entry.Value <= 0 || player.Data.IsDead)
            {
                Bloody.active.Remove(entry.Key);
                continue; // Skip the creation of the next blood drop, if the killer is dead or the time is up
            }

            new BloodyTrail(player, bloodyPlayer);
        }
    }

    // Mini set adapted button cooldown for Vampire, Sheriff, Jackal, Sidekick, Warlock, Cleaner
    public static void miniCooldownUpdate()
    {
        if (Mini.mini != null && LocalPlayer.Control == Mini.mini)
        {
            var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            HudManagerStartPatch.sheriffKillButton.MaxTimer = Sheriff.cooldown * multiplier;
            HudManagerStartPatch.vampireKillButton.MaxTimer = Vampire.cooldown * multiplier;
            HudManagerStartPatch.jackalKillButton.MaxTimer = Jackal.cooldown * multiplier;
            HudManagerStartPatch.sidekickKillButton.MaxTimer = Sidekick.cooldown * multiplier;
            HudManagerStartPatch.warlockCurseButton.MaxTimer = Warlock.cooldown * multiplier;
            HudManagerStartPatch.cleanerCleanButton.MaxTimer = Cleaner.cooldown * multiplier;
            HudManagerStartPatch.witchSpellButton.MaxTimer =
                (Witch.cooldown + Witch.currentCooldownAddition) * multiplier;
            HudManagerStartPatch.ninjaButton.MaxTimer = Ninja.cooldown * multiplier;
            HudManagerStartPatch.thiefKillButton.MaxTimer = Thief.cooldown * multiplier;
        }
    }

    public static void trapperUpdate()
    {
        if (Trapper.trapper == null || LocalPlayer.Control != Trapper.trapper ||
            Trapper.trapper.Data.IsDead) return;
        var (playerCompleted, _) = TasksHandler.taskInfo(Trapper.trapper.Data);
        if (playerCompleted == Trapper.rechargedTasks)
        {
            Trapper.rechargedTasks += Trapper.rechargeTasksNumber;
            if (Trapper.maxCharges > Trapper.charges) Trapper.charges++;
        }
    }

    private static void hunterUpdate()
    {
        if (!HideNSeek.isHideNSeekGM) return;
        var minutes = (int)HideNSeek.timer / 60;
        var seconds = (int)HideNSeek.timer % 60;
        var suffix = $" {minutes:00}:{seconds:00}";

        if (HideNSeek.timerText == null)
        {
            var roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
            if (roomTracker != null)
            {
                var gameObject = Object.Instantiate(roomTracker.gameObject);

                gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                HideNSeek.timerText = gameObject.GetComponent<TMP_Text>();

                // Use local position to place it in the player's view instead of the world location
                gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                if (DataManager.Settings.Gameplay.StreamerMode)
                    gameObject.transform.localPosition = new Vector3(0, 2f, gameObject.transform.localPosition.z);
            }
        }
        else
        {
            if (HideNSeek.isWaitingTimer)
            {
                HideNSeek.timerText.text = "<color=#0000cc>" + suffix + "</color>";
                HideNSeek.timerText.color = Color.blue;
            }
            else
            {
                HideNSeek.timerText.text = "<color=#FF0000FF>" + suffix + "</color>";
                HideNSeek.timerText.color = Color.red;
            }
        }

        if (HideNSeek.isHunted() && !Hunted.taskPunish && !HideNSeek.isWaitingTimer)
        {
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(LocalPlayer.Data);
            var numberOfTasks = playerTotal - playerCompleted;
            if (numberOfTasks == 0)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                    (byte)CustomRPC.ShareTimer, SendOption.Reliable);
                writer.Write(HideNSeek.taskPunish);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shareTimer(HideNSeek.taskPunish);

                Hunted.taskPunish = true;
            }
        }

        if (!HideNSeek.isHunter()) return;

        var playerId = LocalPlayer.PlayerId;
        foreach (var arrow in Hunter.localArrows) arrow.arrow.SetActive(false);
        if (Hunter.arrowActive)
        {
            var arrowIndex = 0;
            foreach (PlayerControl p in AllPlayers)
                if (!p.Data.IsDead && !p.Data.Role.IsImpostor)
                {
                    if (arrowIndex >= Hunter.localArrows.Count) Hunter.localArrows.Add(new Arrow(Color.blue));
                    if (arrowIndex < Hunter.localArrows.Count && Hunter.localArrows[arrowIndex] != null)
                    {
                        Hunter.localArrows[arrowIndex].arrow.SetActive(true);
                        Hunter.localArrows[arrowIndex].Update(p.transform.position, Color.blue);
                    }

                    arrowIndex++;
                }
        }
    }

    private static void cultistUpdate()
    {
        if (Cultist.localArrows == null) return;

        foreach (Arrow arrow in Cultist.localArrows) arrow.arrow.SetActive(false);

        if (Cultist.cultist == null || Cultist.cultist.Data.IsDead) return;


        if (LocalPlayer.Control == Cultist.cultist)
        {
            var arrowIndex = 0;
            foreach (PlayerControl p in AllPlayers)
            {
                var arrowForImp = p == Follower.follower;
                if (!p.Data.IsDead && arrowForImp)
                {
                    if (arrowIndex >= Cultist.localArrows.Count)
                        Cultist.localArrows.Add(new Arrow(Palette.ImpostorRed));
                    if (arrowIndex < Cultist.localArrows.Count && Cultist.localArrows[arrowIndex] != null)
                    {
                        Cultist.localArrows[arrowIndex].arrow.SetActive(true);
                        Cultist.localArrows[arrowIndex].Update(p.transform.position, Palette.ImpostorRed);
                    }

                    arrowIndex++;
                }
            }
        }
    }

    private static void followerUpdate()
    {
        if (Follower.localArrows == null) return;

        foreach (Arrow arrow in Follower.localArrows) arrow.arrow.SetActive(false);

        if (Follower.follower == null || Follower.follower.Data.IsDead) return;


        if (LocalPlayer.Control == Follower.follower)
        {
            var arrowIndex = 0;
            foreach (PlayerControl p in AllPlayers)
            {
                var arrowForImp = p == Cultist.cultist;
                if (!p.Data.IsDead && arrowForImp)
                {
                    if (arrowIndex >= Follower.localArrows.Count)
                        Follower.localArrows.Add(new Arrow(Palette.ImpostorRed));
                    if (arrowIndex < Follower.localArrows.Count && Follower.localArrows[arrowIndex] != null)
                    {
                        Follower.localArrows[arrowIndex].arrow.SetActive(true);
                        Follower.localArrows[arrowIndex].Update(p.transform.position, Palette.ImpostorRed);
                    }

                    arrowIndex++;
                }
            }
        }
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
            GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        // Mini and Morphling shrink
        if (!PropHunt.isPropHuntGM) playerSizeUpdate(__instance);

        // set position of colorblind text
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            //pc.cosmetics.colorBlindText.gameObject.transform.localPosition = new Vector3(0, 0, -0.0001f);
        }

        if (LocalPlayer.Control == __instance)
        {
            // Update player outlines
            setBasePlayerOutlines();

            // Update Role Description
            Helpers.refreshRoleDescription(__instance);

            // Update Player Info
            updatePlayerInfo();

            //Update pet visibility
            setPetVisibility();

            // Time Master
            bendTimeUpdate();
            // Morphling
            morphlingSetTarget();
            // PrivateInvestigator
            privateInvestigatorSetTarget();
            // Medic
            medicSetTarget();
            // Bomber2
            bomber2SetTarget();
            // Set Werewolf Target
            werewolfSetTarget();
            // Shifter
            shifterSetTarget();
            // Sheriff
            sheriffSetTarget();
            // Deputy
            deputySetTarget();
            deputyUpdate();
            // Detective
            detectiveUpdateFootPrints();
            // Tracker
            trackerSetTarget();
            // Vampire
            vampireSetTarget();
            Garlic.UpdateAll();
            Trap.Update();
            // Eraser
            eraserSetTarget();
            // Engineer
            engineerUpdate();
            // Tracker
            trackerUpdate();
            // Jackal
            jackalSetTarget();
            // Sidekick
            sidekickSetTarget();
            // Impostor
            impostorSetTarget();
            // Warlock
            warlockSetTarget();
            // Check for deputy promotion on Sheriff disconnect
            deputyCheckPromotion();
            // Check for sidekick promotion on Jackal disconnect
            sidekickCheckPromotion();
            // SecurityGuard
            securityGuardSetTarget();
            securityGuardUpdate();
            // Arsonist
            arsonistSetTarget();
            // Snitch
            snitchUpdate();
            snitchTextUpdate();
            // BodyGuard
            bodyGuardSetTarget();
            // undertaker
            undertakerDragBodyUpdate();
            // Amnisiac
            amnisiacUpdate();
            // BountyHunter
            bountyHunterUpdate();
            // Vulture
            vultureUpdate();
            radarUpdate();
            // Medium
            mediumSetTarget();
            // Morphling and Camouflager
            morphlingAndCamouflagerUpdate();
            // Lawyer
            lawyerUpdate();
            // Pursuer
            pursuerSetTarget();
            // Blackmailer
            blackMailerSetTarget();
            // Witch
            witchSetTarget();
            // Cultist
            cultistUpdate();
            followerUpdate();
            //Cultist
            cultistSetFollower();
            // Ninja
            ninjaSetTarget();
            NinjaTrace.UpdateAll();
            ninjaUpdate();
            swooperUpdate();
            // Thief
            thiefSetTarget();

            hackerUpdate();
            swapperUpdate();
            // Hacker
            hackerUpdate();
            // Trapper
            trapperUpdate();

            // -- MODIFIER--
            // Bait
            baitUpdate();
            // Bloody
            bloodyUpdate();
            // mini (for the cooldowns)
            miniCooldownUpdate();
            // Chameleon (invis stuff, timers)
            Chameleon.update();
            Bomb.update();

            // -- GAME MODE --
            hunterUpdate();
            PropHunt.update();
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
internal class PlayerPhysicsWalkPlayerToPatch
{
    private static Vector2 offset = Vector2.zero;

    public static void Prefix(PlayerPhysics __instance)
    {
        var correctOffset = !Helpers.isCamoComms() && Camouflager.camouflageTimer <= 0f &&
                            !Helpers.MushroomSabotageActive() && (__instance.myPlayer == Mini.mini ||
                                                                  (Morphling.morphling != null &&
                                                                   __instance.myPlayer == Morphling.morphling &&
                                                                   Morphling.morphTarget == Mini.mini &&
                                                                   Morphling.morphTimer > 0f));
        correctOffset = correctOffset && !(Mini.mini == Morphling.morphling && Morphling.morphTimer > 0f);
        if (correctOffset)
        {
            var currentScaling = (Mini.growingProgress() + 1) * 0.5f;
            __instance.myPlayer.Collider.offset = currentScaling * Mini.defaultColliderOffset * Vector2.down;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
internal class PlayerControlCmdReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (HideNSeek.isHideNSeekGM || PropHunt.isPropHuntGM) return false;
        Helpers.handleVampireBiteOnBodyReport();
        Helpers.handleBomber2ExplodeOnBodyReport();
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(LocalPlayer.Control.CmdReportDeadBody))]
internal class BodyReportPatch
{
    private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
    {
        // Medic or Detective report
        var isMedicReport = Medic.medic != null && Medic.medic == LocalPlayer.Control &&
                            __instance.PlayerId == Medic.medic.PlayerId;
        var isDetectiveReport = Detective.detective != null &&
                                Detective.detective == LocalPlayer.Control &&
                                __instance.PlayerId == Detective.detective.PlayerId;
        var isSluethReport = Slueth.slueth != null && Slueth.slueth == LocalPlayer.Control &&
                             __instance.PlayerId == Slueth.slueth.PlayerId;
        if (isMedicReport || isDetectiveReport)
        {
            var deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

            if (deadPlayer != null && deadPlayer.killerIfExisting != null)
            {
                var timeSinceDeath = (float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
                var msg = "";

                if (isMedicReport)
                {
                    msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
                }
                else if (isDetectiveReport)
                {
                    if (timeSinceDeath < Detective.reportNameDuration * 1000)
                    {
                        msg = $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.Data.PlayerName}!";
                    }
                    else if (timeSinceDeath < Detective.reportColorDuration * 1000)
                    {
                        var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting) ? "lighter" : "darker";
                        msg = $"Body Report: The killer appears to be a {typeOfColor} color!";
                    }
                    else
                    {
                        msg = "Body Report: The corpse is too old to gain information from!";
                    }
                }

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(LocalPlayer.Control,
                            msg);

                        // Ghost Info
                        var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                            (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable);
                        writer.Write(LocalPlayer.PlayerId);
                        writer.Write((byte)RPCProcedure.GhostInfoTypes.DetectiveOrMedicInfo);
                        writer.Write(msg);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }

                    if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                }
            }
        }

        if (isSluethReport)
        {
            var reported = Helpers.playerById(target.PlayerId);
            Slueth.reported.Add(reported);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderPlayerPatch
{
    public static bool resetToCrewmate;
    public static bool resetToDead;

    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Allow everyone to murder players
        resetToCrewmate = !__instance.Data.Role.IsImpostor;
        resetToDead = __instance.Data.IsDead;
        __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
        __instance.Data.IsDead = false;
    }

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Collect dead player info
        var deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Kill, __instance);
        deadPlayers.Add(deadPlayer);

        // Reset killer to crewmate if resetToCrewmate
        if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
        if (resetToDead) __instance.Data.IsDead = true;

        // Remove fake tasks when player dies
        if (target.hasFakeTasks() || target == Lawyer.lawyer || target == Pursuer.pursuer || target == Thief.thief)
            target.clearAllTasks();

        // First kill (set before lover suicide)
        if (MapOptions.firstKillName == "") MapOptions.firstKillName = target.Data.PlayerName;

        // Lover suicide trigger on murder
        if ((Lovers.lover1 != null && target == Lovers.lover1) || (Lovers.lover2 != null && target == Lovers.lover2))
        {
            PlayerControl otherLover = target == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
            if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
            {
                otherLover.MurderPlayer(otherLover);
                overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoverSuicide);
            }
        }

        // Sidekick promotion trigger on murder
        if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead &&
            target == Jackal.jackal && Jackal.jackal == LocalPlayer.Control)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.SidekickPromotes, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.sidekickPromotes();
        }

        // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
        if (target == Lawyer.target && AmongUsClient.Instance.AmHost && Lawyer.lawyer != null)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.LawyerPromotesToPursuer, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.lawyerPromotesToPursuer();

            // Undertaker Button Sync
            if (Undertaker.undertaker != null && LocalPlayer.Control == Undertaker.undertaker &&
                __instance == Undertaker.undertaker && HudManagerStartPatch.undertakerDragButton != null)
                HudManagerStartPatch.undertakerDragButton.Timer = Undertaker.dragingDelaiAfterKill;
        }

        // Seer show flash and add dead player position
        if (Seer.seer != null && (LocalPlayer.Control == Seer.seer || Helpers.shouldShowGhostInfo()) &&
            !Seer.seer.Data.IsDead && Seer.seer != target && Seer.mode <= 1)
            Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f), message: "Seer Info: Someone Died");
        if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(target.transform.position);

        // Tracker store body positions
        if (Tracker.deadBodyPositions != null) Tracker.deadBodyPositions.Add(target.transform.position);

        // Medium add body
        if (Medium.deadBodies != null)
            Medium.futureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(deadPlayer, target.transform.position));

        // Set bountyHunter cooldown
        if (BountyHunter.bountyHunter != null && LocalPlayer.Control == BountyHunter.bountyHunter &&
            __instance == BountyHunter.bountyHunter)
        {
            if (target == BountyHunter.bounty)
            {
                BountyHunter.bountyHunter.SetKillTimer(BountyHunter.bountyKillCooldown);
                BountyHunter.bountyUpdateTimer = 0f; // Force bounty update
            }
            else
            {
                BountyHunter.bountyHunter.SetKillTimer(
                    GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + BountyHunter.punishmentTime);
            }
        }

        // Mini Set Impostor Mini kill timer (Due to mini being a modifier, all "SetKillTimers" must have happened before this!)
        if (Mini.mini != null && __instance == Mini.mini && __instance == LocalPlayer.Control)
        {
            var multiplier = 1f;
            if (Mini.mini != null && LocalPlayer.Control == Mini.mini)
                multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            Mini.mini.SetKillTimer(__instance.killTimer * multiplier);
        }

        // Cleaner Button Sync
        if (Cleaner.cleaner != null && LocalPlayer.Control == Cleaner.cleaner &&
            __instance == Cleaner.cleaner && HudManagerStartPatch.cleanerCleanButton != null)
            HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.cleaner.killTimer;

        // Witch Button Sync
        if (Witch.triggerBothCooldowns && Witch.witch != null && LocalPlayer.Control == Witch.witch &&
            __instance == Witch.witch && HudManagerStartPatch.witchSpellButton != null)
            HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;

        // Warlock Button Sync
        if (Warlock.warlock != null && LocalPlayer.Control == Warlock.warlock &&
            __instance == Warlock.warlock && HudManagerStartPatch.warlockCurseButton != null)
            if (Warlock.warlock.killTimer > HudManagerStartPatch.warlockCurseButton.Timer)
                HudManagerStartPatch.warlockCurseButton.Timer = Warlock.warlock.killTimer;
        // Ninja Button Sync
        if (Ninja.ninja != null && LocalPlayer.Control == Ninja.ninja && __instance == Ninja.ninja &&
            HudManagerStartPatch.ninjaButton != null)
            HudManagerStartPatch.ninjaButton.Timer = HudManagerStartPatch.ninjaButton.MaxTimer;

        // Bait
        if (Bait.bait.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
        {
            var reportDelay = (float)rnd.Next((int)Bait.reportDelayMin, (int)Bait.reportDelayMax + 1);
            Bait.active.Add(deadPlayer, reportDelay);

            if (Bait.showKillFlash && __instance == LocalPlayer.Control)
                Helpers.showFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
        }

        // Add Bloody Modifier
        if (Bloody.bloody.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.Bloody, SendOption.Reliable);
            writer.Write(__instance.PlayerId);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.bloody(__instance.PlayerId, target.PlayerId);
        }

        // VIP Modifier
        if (Vip.vip.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
        {
            var color = Color.yellow;
            if (Vip.showColor)
            {
                color = Color.white;
                if (target.Data.Role.IsImpostor) color = Color.red;
                else if (RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().isNeutral) color = Color.blue;
            }

            Helpers.showFlash(color, 1.5f);
        }

        // HideNSeek
        if (HideNSeek.isHideNSeekGM)
        {
            var visibleCounter = 0;
            var bottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(-0.25f, -0.25f, 0);
            foreach (PlayerControl p in AllPlayers)
            {
                if (!MapOptions.playerIcons.ContainsKey(p.PlayerId) || p.Data.Role.IsImpostor) continue;
                if (p.Data.IsDead || p.Data.Disconnected)
                {
                    MapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                }
                else
                {
                    MapOptions.playerIcons[p.PlayerId].transform.localPosition =
                        bottomLeft + (Vector3.right * visibleCounter * 0.35f);
                    visibleCounter++;
                }
            }
        }

        // Snitch
        if (Snitch.snitch != null && LocalPlayer.PlayerId == Snitch.snitch.PlayerId &&
            MapBehaviourPatch.herePoints.Keys.Any(x => x.PlayerId == target.PlayerId))
            foreach (var a in MapBehaviourPatch.herePoints.Where(x => x.Key.PlayerId == target.PlayerId))
            {
                Object.Destroy(a.Value);
                MapBehaviourPatch.herePoints.Remove(a.Key);
            }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
internal class PlayerControlSetCoolDownPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
        if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f) return false;
        var multiplier = 1f;
        var addition = 0f;
        if (Mini.mini != null && LocalPlayer.Control == Mini.mini)
            multiplier = Mini.isGrownUp() ? 0.66f : 2f;
        if (BountyHunter.bountyHunter != null && LocalPlayer.Control == BountyHunter.bountyHunter)
            addition = BountyHunter.punishmentTime;

        __instance.killTimer = Mathf.Clamp(time, 0f,
            (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier) + addition);
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer,
            (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier) + addition);
        return false;
    }
}

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
internal class KillAnimationCoPerformKillPatch
{
    public static bool hideNextAnimation;

    public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source,
        [HarmonyArgument(1)] ref PlayerControl target)
    {
        if (hideNextAnimation)
            source = target;
        hideNextAnimation = false;
    }
}

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
internal class KillAnimationSetMovementPatch
{
    private static int? colorId;

    public static void Prefix(PlayerControl source, bool canMove)
    {
        var color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
        if (Morphling.morphling != null && source.Data.PlayerId == Morphling.morphling.PlayerId)
        {
            var index = Palette.PlayerColors.IndexOf(color);
            if (index != -1) colorId = index;
        }
    }

    public static void Postfix(PlayerControl source, bool canMove)
    {
        if (colorId.HasValue) source.RawSetColor(colorId.Value);
        colorId = null;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class ExilePlayerPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        // Collect dead player info
        var deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Exile, null);
        deadPlayers.Add(deadPlayer);


        // Remove fake tasks when player dies
        if (__instance.hasFakeTasks() || __instance == Lawyer.lawyer || __instance == Pursuer.pursuer ||
            __instance == Thief.thief)
            __instance.clearAllTasks();

        // Lover suicide trigger on exile
        if ((Lovers.lover1 != null && __instance == Lovers.lover1) ||
            (Lovers.lover2 != null && __instance == Lovers.lover2))
        {
            PlayerControl otherLover = __instance == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
            if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
            {
                otherLover.Exiled();
                overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoverSuicide);
            }
        }

        // Sidekick promotion trigger on exile
        if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead &&
            __instance == Jackal.jackal && Jackal.jackal == LocalPlayer.Control)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                (byte)CustomRPC.SidekickPromotes, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.sidekickPromotes();
        }

        // Pursuer promotion trigger on exile & suicide (the host sends the call such that everyone recieves the update before a possible game End)
        if (Lawyer.lawyer != null && __instance == Lawyer.target)
        {
            PlayerControl lawyer = Lawyer.lawyer;
            if (AmongUsClient.Instance.AmHost &&
                ((Lawyer.target != Jester.jester && !Lawyer.isProsecutor) || Lawyer.targetWasGuessed))
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                    (byte)CustomRPC.LawyerPromotesToPursuer, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer();
            }

            if (!Lawyer.targetWasGuessed && !Lawyer.isProsecutor)
            {
                if (Lawyer.lawyer != null) Lawyer.lawyer.Exiled();
                if (Pursuer.pursuer != null) Pursuer.pursuer.Exiled();

                var writer = AmongUsClient.Instance.StartRpcImmediately(LocalPlayer.Control.NetId,
                    (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable);
                writer.Write(LocalPlayer.PlayerId);
                writer.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                writer.Write(lawyer.PlayerId);
                writer.Write((byte)DeadPlayer.CustomDeathReason.LawyerSuicide);
                writer.Write(lawyer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                overrideDeathReasonAndKiller(lawyer, DeadPlayer.CustomDeathReason.LawyerSuicide,
                    lawyer); // TODO: only executed on host?!
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        var shouldInvert = Invert.invert.FindAll(x => x.PlayerId == LocalPlayer.PlayerId).Count > 0 &&
                           Invert.meetings > 0;
        if (__instance.AmOwner &&
            AmongUsClient.Instance &&
            AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
            !LocalPlayer.Data.IsDead &&
            shouldInvert &&
            GameData.Instance &&
            __instance.myPlayer.CanMove)
            __instance.body.velocity *= -1;
        if (__instance.AmOwner &&
            AmongUsClient.Instance &&
            AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
            !LocalPlayer.Data.IsDead &&
            GameData.Instance &&
            __instance.myPlayer.CanMove &&
            Flash.flash.Any(x => x.PlayerId == LocalPlayer.PlayerId))
            __instance.body.velocity *= Flash.speed;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.IsFlashlightEnabled))]
public static class IsFlashlightEnabledPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
            return true;
        __result = false;

        return false;
    }
}*/

/*[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AdjustLighting))]
public static class AdjustLight
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (__instance == null || CachedPlayer.LocalPlayer == null) return true;

        var hasFlashlight = !CachedPlayer.LocalPlayer.Data.IsDead ;
        __instance.SetFlashlightInputMethod();
        __instance.lightSource.SetupLightingForGameplay(hasFlashlight, Lighter.flashlightWidth,
            __instance.TargetFlashlight.transform);

        return false;
    }
}*/

/*[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [
    typeof(PlayerControl), typeof(DisconnectReasons)
])]
public static class GameDataHandleDisconnectPatch
{
    public static void Prefix(GameData __instance, PlayerControl player, DisconnectReasons reason)
    {
        if (MeetingHud.Instance) MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, player.PlayerId);
    }
}*/
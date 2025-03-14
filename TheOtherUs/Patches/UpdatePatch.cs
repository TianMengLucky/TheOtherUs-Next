namespace TheOtherUs.Patches;

/*
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal class HudManagerUpdatePatch
{
    private static readonly Dictionary<byte, (string name, Color color)> TagColorDict = new();

    private static void resetNameTagsAndColors()
    {
        var localPlayer = LocalPlayer.Control;
        var myData = LocalPlayer.NetPlayerInfo;
        var amImpostor = myData.Role.IsImpostor;
        var morphTimerNotUp = Morphling.morphTimer > 0f;
        var morphTargetNotNull = Morphling.morphTarget != null;

        var dict = TagColorDict;
        dict.Clear();

        foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            var player = data.Object;
            var text = data.PlayerName;
            Color color;
            if (player)
            {
                var playerName = text;
                if (morphTimerNotUp && morphTargetNotNull && Morphling.morphling == player)
                    playerName = Morphling.morphTarget.Data.PlayerName;
                var nameText = player.cosmetics.nameText;

                nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                nameText.color = nameText.color.SetAlpha(Chameleon.visibility(player.PlayerId));
            }
            else
            {
                color = Color.white;
            }


            dict.Add(data.PlayerId, (text, color));
        }

        if (MeetingHud.Instance != null)
            foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
            {
                var data = dict[playerVoteArea.TargetPlayerId];
                var text = playerVoteArea.NameText;
                text.text = data.name;
                text.color = data.color;
            }
    }

    private static void setPlayerNameColor(PlayerControl p, Color color)
    {
        p.cosmetics.nameText.color = color.SetAlpha(Chameleon.visibility(p.PlayerId));
        if (MeetingHud.Instance != null)
            foreach (var player in MeetingHud.Instance.playerStates)
                if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                    player.NameText.color = color;
    }

    private static void updateBlindReport()
    {
        if (Blind.blind != null && LocalPlayer.Control == Blind.blind)
            DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
        // Sadly the report button cannot be hidden due to preventing R to report
    }

    private static void setNameColors()
    {
        var localPlayer = LocalPlayer.Control;
        var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer, false).FirstOrDefault();
        setPlayerNameColor(localPlayer, localRole.color);

        /*if (Jester.jester != null && Jester.jester == localPlayer)
            setPlayerNameColor(Jester.jester, Jester.color);
        else if (Mayor.mayor != null && Mayor.mayor == localPlayer)
            setPlayerNameColor(Mayor.mayor, Mayor.color);
        else if (Engineer.engineer != null && Engineer.engineer == localPlayer)
            setPlayerNameColor(Engineer.engineer, Engineer.color);
        else if (Sheriff.sheriff != null && Sheriff.sheriff == localPlayer) {
            setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            if (Deputy.deputy != null && Deputy.knowsSheriff) {
                setPlayerNameColor(Deputy.deputy, Deputy.color);
            }
        } else if (Deputy.deputy != null && Deputy.deputy == localPlayer) {
            setPlayerNameColor(Deputy.deputy, Deputy.color);
            if (Sheriff.sheriff != null && Deputy.knowsSheriff) {
                setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            }
        }#1#

        if (Sheriff.sheriff != null && Sheriff.sheriff == localPlayer)
        {
            setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            if (Deputy.deputy != null && Deputy.knowsSheriff) setPlayerNameColor(Deputy.deputy, Sheriff.color);
        }
        /*else if (Portalmaker.portalmaker != null && Portalmaker.portalmaker == localPlayer)
            setPlayerNameColor(Portalmaker.portalmaker, Portalmaker.color);
        else if (Lighter.lighter != null && Lighter.lighter == localPlayer)
            setPlayerNameColor(Lighter.lighter, Lighter.color);
        else if (Detective.detective != null && Detective.detective == localPlayer)
            setPlayerNameColor(Detective.detective, Detective.color);
        else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == localPlayer)
            setPlayerNameColor(TimeMaster.timeMaster, TimeMaster.color);
        else if (Medic.medic != null && Medic.medic == localPlayer)
            setPlayerNameColor(Medic.medic, Medic.color);
        else if (Shifter.shifter != null && Shifter.shifter == localPlayer)
            setPlayerNameColor(Shifter.shifter, Shifter.color);
        else if (Swapper.swapper != null && Swapper.swapper == localPlayer)
            setPlayerNameColor(Swapper.swapper, Swapper.color);
        else if (Seer.seer != null && Seer.seer == localPlayer)
            setPlayerNameColor(Seer.seer, Seer.color);
        else if (Hacker.hacker != null && Hacker.hacker == localPlayer)
            setPlayerNameColor(Hacker.hacker, Hacker.color);
        else if (Tracker.tracker != null && Tracker.tracker == localPlayer)
            setPlayerNameColor(Tracker.tracker, Tracker.color);
        else if (Snitch.snitch != null && Snitch.snitch == localPlayer)
            setPlayerNameColor(Snitch.snitch, Snitch.color);#1#
        else if (Jackal.jackal != null && Jackal.jackal == localPlayer)
        {
            // Jackal can see his sidekick
            setPlayerNameColor(Jackal.jackal, Jackal.color);
            if (Sidekick.sidekick != null) setPlayerNameColor(Sidekick.sidekick, Jackal.color);
            if (Jackal.fakeSidekick != null) setPlayerNameColor(Jackal.fakeSidekick, Jackal.color);
        }
        /*else if (Spy.spy != null && Spy.spy == localPlayer) {
            setPlayerNameColor(Spy.spy, Spy.color);
        } else if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == localPlayer) {
            setPlayerNameColor(SecurityGuard.securityGuard, SecurityGuard.color);
        } else if (Arsonist.arsonist != null && Arsonist.arsonist == localPlayer) {
            setPlayerNameColor(Arsonist.arsonist, Arsonist.color);
        } else if (Guesser.niceGuesser != null && Guesser.niceGuesser == localPlayer) {
            setPlayerNameColor(Guesser.niceGuesser, Guesser.color);
        } else if (Guesser.evilGuesser != null && Guesser.evilGuesser == localPlayer) {
            setPlayerNameColor(Guesser.evilGuesser, Palette.ImpostorRed);
        } else if (Vulture.vulture != null && Vulture.vulture == localPlayer) {
            setPlayerNameColor(Vulture.vulture, Vulture.color);
        } else if (Medium.medium != null && Medium.medium == localPlayer) {
            setPlayerNameColor(Medium.medium, Medium.color);
        } else if (Trapper.trapper != null && Trapper.trapper == localPlayer) {
            setPlayerNameColor(Trapper.trapper, Trapper.color);
        } else if (Lawyer.lawyer != null && Lawyer.lawyer == localPlayer) {
            setPlayerNameColor(Lawyer.lawyer, Lawyer.color);
        } else if (Pursuer.pursuer != null && Pursuer.pursuer == localPlayer) {
            setPlayerNameColor(Pursuer.pursuer, Pursuer.color);
        }#1#

        if (Snitch.snitch != null)
        {
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
            var numberOfTasks = playerTotal - playerCompleted;
            var snitchIsDead = Snitch.snitch.Data.IsDead;

            var forImp = localPlayer.Data.Role.IsImpostor;
            var forKillerTeam = Snitch.Team == Snitch.includeNeutralTeam.KillNeutral && Helpers.isKiller(localPlayer);
            var forEvilTeam = Snitch.Team == Snitch.includeNeutralTeam.EvilNeutral && Helpers.isEvil(localPlayer);
            var forNeutraTeam = Snitch.Team == Snitch.includeNeutralTeam.AllNeutral && Helpers.isNeutral(localPlayer);
            if (numberOfTasks <= Snitch.taskCountForReveal)
                foreach (PlayerControl p in AllPlayers)
                    if (forImp || forKillerTeam || forEvilTeam || forNeutraTeam)
                        setPlayerNameColor(Snitch.snitch, Snitch.color);
            if (numberOfTasks == 0 && Snitch.seeInMeeting && !snitchIsDead)
                foreach (PlayerControl p in AllPlayers)
                {
                    var TargetsImp = p.Data.Role.IsImpostor;
                    var TargetsKillerTeam = Snitch.Team == Snitch.includeNeutralTeam.KillNeutral && Helpers.isKiller(p);
                    var TargetsEvilTeam = Snitch.Team == Snitch.includeNeutralTeam.EvilNeutral && Helpers.isEvil(p);
                    var TargetsNeutraTeam = Snitch.Team == Snitch.includeNeutralTeam.AllNeutral && Helpers.isNeutral(p);
                    var targetsRole = RoleInfo.getRoleInfoForPlayer(p, false).FirstOrDefault();
                    if (localPlayer == Snitch.snitch &&
                        (TargetsImp || TargetsKillerTeam || TargetsEvilTeam || TargetsNeutraTeam))
                    {
                        if (Snitch.teamNeutraUseDifferentArrowColor)
                            setPlayerNameColor(p, targetsRole.color);
                        else
                            setPlayerNameColor(p, Palette.ImpostorRed);
                    }
                }
        }

        // No else if here, as a Lover of team Jackal needs the colors
        if (Sidekick.sidekick != null && Sidekick.sidekick == localPlayer)
        {
            // Sidekick can see the jackal
            setPlayerNameColor(Sidekick.sidekick, Sidekick.color);
            if (Jackal.jackal != null) setPlayerNameColor(Jackal.jackal, Jackal.color);
        }

        // No else if here, as the Impostors need the Spy name to be colored
        if (Spy.spy != null && localPlayer.Data.Role.IsImpostor) setPlayerNameColor(Spy.spy, Spy.color);
        if (Sidekick.sidekick != null && Sidekick.wasTeamRed && localPlayer.Data.Role.IsImpostor)
            setPlayerNameColor(Sidekick.sidekick, Spy.color);
        if (Jackal.jackal != null && Jackal.wasTeamRed && localPlayer.Data.Role.IsImpostor)
            setPlayerNameColor(Jackal.jackal, Spy.color);

        // Crewmate roles with no changes: Mini
        // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
    }

    private static void setNameTags()
    {
        // Mafia
        if (LocalPlayer != null && LocalPlayer.Data.Role.IsImpostor)
        {
            foreach (PlayerControl player in AllPlayers)
                if (Godfather.godfather != null && Godfather.godfather == player)
                    player.cosmetics.nameText.text = player.Data.PlayerName + " (G)";
                else if (Mafioso.mafioso != null && Mafioso.mafioso == player)
                    player.cosmetics.nameText.text = player.Data.PlayerName + " (M)";
                else if (Janitor.janitor != null && Janitor.janitor == player)
                    player.cosmetics.nameText.text = player.Data.PlayerName + " (J)";
            if (MeetingHud.Instance != null)
                foreach (var player in MeetingHud.Instance.playerStates)
                    if (Godfather.godfather != null && Godfather.godfather.PlayerId == player.TargetPlayerId)
                        player.NameText.text = Godfather.godfather.Data.PlayerName + " (G)";
                    else if (Mafioso.mafioso != null && Mafioso.mafioso.PlayerId == player.TargetPlayerId)
                        player.NameText.text = Mafioso.mafioso.Data.PlayerName + " (M)";
                    else if (Janitor.janitor != null && Janitor.janitor.PlayerId == player.TargetPlayerId)
                        player.NameText.text = Janitor.janitor.Data.PlayerName + " (J)";
        }

        // Lovers
        if (Lovers.lover1 != null && Lovers.lover2 != null && (Lovers.lover1 == LocalPlayer.Control ||
                                                               Lovers.lover2 == LocalPlayer.Control))
        {
            var suffix = Helpers.cs(Lovers.color, " ♥");
            Lovers.lover1.cosmetics.nameText.text += suffix;
            Lovers.lover2.cosmetics.nameText.text += suffix;

            if (MeetingHud.Instance != null)
                foreach (var player in MeetingHud.Instance.playerStates)
                    if (Lovers.lover1.PlayerId == player.TargetPlayerId ||
                        Lovers.lover2.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
        }

        // Lawyer or Prosecutor
        var localIsLawyer = Lawyer.lawyer != null && Lawyer.target != null &&
                            Lawyer.lawyer == PlayerControl.LocalPlayer;
        var localIsKnowingTarget = Lawyer.lawyer != null && !Lawyer.isProsecutor && Lawyer.target != null &&
                                   Lawyer.targetKnows && Lawyer.target == PlayerControl.LocalPlayer;
        if (localIsLawyer || (localIsKnowingTarget && !Lawyer.lawyer.Data.IsDead))
        {
            //Color color = Lawyer.color;
            //Control target = Lawyer.target;
            var suffix = Helpers.cs(Lawyer.color, " §");
            Lawyer.target.cosmetics.nameText.text += suffix;

            if (MeetingHud.Instance != null)
                foreach (var player in MeetingHud.Instance.playerStates)
                    if (player.TargetPlayerId == Lawyer.target.PlayerId)
                        player.NameText.text += suffix;
        }

        // Former Thief
        if (Thief.formerThief != null && (Thief.formerThief == LocalPlayer.Control ||
                                          LocalPlayer.Control.Data.IsDead))
        {
            var suffix = Helpers.cs(Thief.color, " $");
            Thief.formerThief.cosmetics.nameText.text += suffix;
            if (MeetingHud.Instance != null)
                foreach (var player in MeetingHud.Instance.playerStates)
                    if (player.TargetPlayerId == Thief.formerThief.PlayerId)
                        player.NameText.text += suffix;
        }

        // Display lighter / darker color for all alive players
        if (LocalPlayer == null || MeetingHud.Instance == null || !MapOptions.showLighterDarker) return;
        {
            foreach (var player in MeetingHud.Instance.playerStates)
            {
                var target = Helpers.playerById(player.TargetPlayerId);
                if (target != null) player.NameText.text += $" ({(DIYColor.IsLighter(target.Data.Color) ? "L" : "D")})";
            }
        }
    }

    private static void updateShielded()
    {
        if (Medic.shielded == null) return;

        if (Medic.shielded.Data.IsDead || Medic.medic == null || Medic.medic.Data.IsDead) Medic.shielded = null;
    }

    private static void timerUpdate()
    {
        var dt = Time.deltaTime;
        Hacker.hackerTimer -= dt;
        Trickster.lightsOutTimer -= dt;
        Tracker.corpsesTrackingTimer -= dt;
        Ninja.invisibleTimer -= dt;
        Jackal.swoopTimer -= dt;
        HideNSeek.timer -= dt;
        foreach (byte key in Deputy.handcuffedKnows.Keys)
            Deputy.handcuffedKnows[key] -= dt;
    }

    public static void miniUpdate()
    {
        if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() ||
            (Mini.mini == Morphling.morphling && Morphling.morphTimer > 0f) ||
            (Mini.mini == Ninja.ninja && Ninja.isInvisble) || SurveillanceMinigamePatch.nightVisionIsActive ||
            (Mini.mini == Jackal.jackal && Jackal.isInvisable) || Helpers.isActiveCamoComms()) return;

        float growingProgress = Mini.growingProgress();
        var scale = (growingProgress * 0.35f) + 0.35f;
        var suffix = "";
        if (growingProgress != 1f)
            suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>";
        if (!Mini.isGrowingUpInMeeting && MeetingHud.Instance != null && Mini.ageOnMeetingStart != 0 &&
            !(Mini.ageOnMeetingStart >= 18))
            suffix = " <color=#FAD934FF>(" + Mini.ageOnMeetingStart + ")</color>";

        Mini.mini.cosmetics.nameText.text += suffix;
        if (MeetingHud.Instance != null)
            foreach (var player in MeetingHud.Instance.playerStates)
                if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                    player.NameText.text += suffix;

        if (Morphling.morphling != null && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f)
            Morphling.morphling.cosmetics.nameText.text += suffix;
    }

    private static void updateImpostorKillButton(HudManager __instance)
    {
        if (!LocalPlayer.Data.Role.IsImpostor) return;
        if (MeetingHud.Instance)
        {
            __instance.KillButton.Hide();
            return;
        }

        var enabled = true;
        if (Vampire.vampire != null && Vampire.vampire == LocalPlayer.Control)
            enabled = false;
        else if (Mafioso.mafioso != null && Mafioso.mafioso == LocalPlayer.Control &&
                 Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
            enabled = false;
        else if (Janitor.janitor != null && Janitor.janitor == LocalPlayer.Control)
            enabled = false;
        else if (Cultist.cultist != null && Cultist.cultist == LocalPlayer.Control &&
                 Cultist.needsFollower) enabled = false;

        if (enabled) __instance.KillButton.Show();
        else __instance.KillButton.Hide();

        if (Deputy.handcuffedKnows.ContainsKey(LocalPlayer.PlayerId) &&
            Deputy.handcuffedKnows[LocalPlayer.PlayerId] > 0) __instance.KillButton.Hide();
    }

    private static void updateReportButton(HudManager __instance)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
        if ((Deputy.handcuffedKnows.ContainsKey(LocalPlayer.PlayerId) &&
             Deputy.handcuffedKnows[LocalPlayer.PlayerId] > 0) ||
            MeetingHud.Instance) __instance.ReportButton.Hide();
        else if (!__instance.ReportButton.isActiveAndEnabled) __instance.ReportButton.Show();
    }

    private static void updateVentButton(HudManager __instance)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
        if ((Deputy.handcuffedKnows.ContainsKey(LocalPlayer.PlayerId) &&
             Deputy.handcuffedKnows[LocalPlayer.PlayerId] > 0) ||
            MeetingHud.Instance) __instance.ImpostorVentButton.Hide();
        else if (LocalPlayer.Control.roleCanUseVents() &&
                 !__instance.ImpostorVentButton.isActiveAndEnabled) __instance.ImpostorVentButton.Show();
    }

    private static void updateUseButton(HudManager __instance)
    {
        if (MeetingHud.Instance) __instance.UseButton.Hide();
    }

    private static void updateSabotageButton(HudManager __instance)
    {
        if (MeetingHud.Instance || MapOptions.gameMode == CustomGameModes.HideNSeek ||
            MapOptions.gameMode == CustomGameModes.PropHunt) __instance.SabotageButton.Hide();
    }

    private static void updateMapButton(HudManager __instance)
    {
        if (Trapper.trapper == null || !(LocalPlayer.PlayerId == Trapper.trapper.PlayerId) ||
            __instance == null || __instance.MapButton.HeldButtonSprite == null) return;
        __instance.MapButton.HeldButtonSprite.color = Trapper.playersOnMap.Any() ? Trapper.color : Color.white;
    }

    private static void Postfix(HudManager __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        //壁抜け
        if (Input.GetKeyDown(KeyCode.LeftControl))
            if ((AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
                 AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                && player.CanMove)
                player.Collider.offset = new Vector2(0f, 127f);
        //壁抜け解除
        if (player.Collider.offset.y == 127f)
            if (!Input.GetKey(KeyCode.LeftControl) || AmongUsClient.Instance.IsGameStarted)
                player.Collider.offset = new Vector2(0f, -0.3636f);
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
            GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        CustomButton.HudUpdate();
        resetNameTagsAndColors();
        setNameColors();
        updateShielded();
        setNameTags();

        // Impostors
        updateImpostorKillButton(__instance);
        // Timer updates
        timerUpdate();
        // Mini
        miniUpdate();

        // Deputy Sabotage, Use and Vent Button Disabling
        updateReportButton(__instance);
        updateVentButton(__instance);
        // Meeting hide buttons if needed (used for the map usage, because closing the map would show buttons)
        updateSabotageButton(__instance);
        updateUseButton(__instance);
        updateBlindReport();
        updateMapButton(__instance);
        if (!MeetingHud.Instance) __instance.AbilityButton?.Update();

        // Fix dead player's pets being visible by just always updating whether the pet should be visible at all.
        foreach (PlayerControl target in AllPlayers)
        {
            var pet = target.GetPet();
            if (pet != null)
                pet.Visible = ((PlayerControl.LocalPlayer.Data.IsDead && target.Data.IsDead) || !target.Data.IsDead) &&
                              !target.inVent;
        }
    }
}*/
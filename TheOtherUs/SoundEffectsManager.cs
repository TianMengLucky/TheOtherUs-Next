﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TheOtherUs;

// Class to preload all audio/sound effects that are contained in the embedded resources.
// The effects are made available through the soundEffects Dict / the get and the play methods.
public static class SoundEffectsManager

{
    private static Dictionary<string, AudioClip> soundEffects = new();

    public static void Load()
    {
        soundEffects = new Dictionary<string, AudioClip>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        foreach (var resourceName in resourceNames)
            if (resourceName.Contains("TheOtherUs.Resources.SoundEffects.") && resourceName.Contains(".raw"))
                soundEffects.Add(resourceName, UnityHelper.loadAudioClipFromResources(resourceName));
    }

    public static AudioClip get(string path)
    {
        // Convenience: As as SoundEffects are stored in the same folder, allow using just the name as well
        if (!path.Contains('.')) path = "TheOtherUs.Resources.SoundEffects." + path + ".raw";
        return soundEffects.GetValueOrDefault(path);
    }


    public static void play(string path, float volume = 0.8f, bool loop = false)
    {
        var clipToPlay = get(path);
        stop(path);
        if (!Constants.ShouldPlaySfx() || clipToPlay == null) return;
        var source = SoundManager.Instance.PlaySound(clipToPlay, false, volume);
        source.loop = loop;
    }

    public static void playAtPosition(string path, Vector2 position, float maxDuration = 15f, float range = 5f,
        bool loop = false)
    {
        if (!Constants.ShouldPlaySfx()) return;
        var clipToPlay = get(path);

        var source = SoundManager.Instance.PlaySound(clipToPlay, false);
        source.loop = loop;
        HudManager.Instance.StartCoroutine(Effects.Lerp(maxDuration, new Action<float>(p =>
        {
            if (source == null) return;
            if ((int)p == 1) source.Stop();
            float volume;
            var distance = Vector2.Distance(position, LocalPlayer.Control.GetTruePosition());
            if (distance < range)
                volume = 1f - (distance / range);
            else
                volume = 0f;
            source.volume = volume;
        })));
    }

    public static void stop(string path)
    {
        var soundToStop = get(path);
        if (soundToStop == null) return;
        if (Constants.ShouldPlaySfx())
            SoundManager.Instance.StopSound(soundToStop);
    }

    public static void stopAll()
    {
        if (soundEffects == null) return;
        foreach (var path in soundEffects.Keys) stop(path);
    }
}
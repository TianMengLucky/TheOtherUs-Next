﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;

namespace TheOtherUs.Modules;

public class ModUpdater(IntPtr ptr) : MonoBehaviour(ptr)
{
    /*public const string RepositoryOwner = "SpexGH";
    public const string RepositoryName = "TheOtherUs";

    private bool _busy;
    public List<GithubRelease> Releases;
    private bool showPopUp = true;
    public static ModUpdater Instance { get; private set; }

    public void Awake()
    {
        if (Instance) Destroy(Instance);
        Instance = this;
        foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old")) File.Delete(file);
    }

    private void Start()
    {
        if (_busy) return;
        this.StartCoroutine(CoCheckForUpdate());
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)OnSceneLoaded);
    }


    [HideFromIl2Cpp]
    public void StartDownloadRelease(GithubRelease release)
    {
        if (_busy) return;
        this.StartCoroutine(CoDownloadRelease(release));
    }

    [HideFromIl2Cpp]
    private IEnumerator CoCheckForUpdate()
    {
        _busy = true;
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl($"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases");
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone) yield return new WaitForEndOfFrame();

        if (www.isNetworkError || www.isHttpError) yield break;

        Releases = JsonSerializer.Deserialize<List<GithubRelease>>(www.downloadHandler.text);
        www.downloadHandler.Dispose();
        www.Dispose();
        if (Releases.Any()) Releases.Sort(SortReleases);
        _busy = false;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoDownloadRelease(GithubRelease release)
    {
        _busy = true;

        var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
        popup.TextAreaTMP.fontSize *= 0.7f;
        popup.TextAreaTMP.enableAutoSizing = false;

        popup.Show();

        var button = popup.transform.GetChild(2).gameObject;
        button.SetActive(false);
        popup.TextAreaTMP.text = "Updating TOU\nPlease wait...";

        var asset = release.Assets.Find(FilterPluginAsset);
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl(asset.DownloadUrl);
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            var stars = Mathf.CeilToInt(www.downloadProgress * 10);
            var progress =
                $"Updating TOU\nPlease wait...\nDownloading...\n{new string((char)0x25A0, stars) + new string((char)0x25A1, 10 - stars)}";
            popup.TextAreaTMP.text = progress;
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            popup.TextAreaTMP.text = "Update wasn't successful\nTry again later,\nor update manually.";
            yield break;
        }

        popup.TextAreaTMP.text = "Updating TOU\nPlease wait...\n\nDownload complete\ncopying file...";

        var filePath = Path.Combine(Paths.PluginPath, asset.Name);

        if (File.Exists(filePath + ".old")) File.Delete(filePath + "old");
        if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

        var persistTask = File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
        var hasError = false;
        while (!persistTask.IsCompleted)
        {
            if (persistTask.Exception != null)
            {
                hasError = true;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        www.downloadHandler.Dispose();
        www.Dispose();

        if (!hasError) popup.TextAreaTMP.text = "TheOtherUs\nupdated successfully\nPlease restart the game.";
        button.SetActive(true);
        _busy = false;
    }

    [HideFromIl2Cpp]
    private static bool FilterLatestRelease(GithubRelease release)
    {
        return release.IsNewer(TheOtherRolesPlugin.version) && release.Assets.Any(FilterPluginAsset);
    }

    [HideFromIl2Cpp]
    private static bool FilterPluginAsset(GithubAsset asset)
    {
        return asset.Name == "TheOtherUs.dll";
    }

    [HideFromIl2Cpp]
    private static int SortReleases(GithubRelease a, GithubRelease b)
    {
        if (a.IsNewer(b.Version)) return -1;
        return b.IsNewer(a.Version) ? 1 : 0;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_busy || scene.name != "MainMenu") return;
        var latestRelease = Releases.FirstOrDefault();
        if (latestRelease == null || latestRelease.Version <= TheOtherRolesPlugin.version)
            return;

        var template = GameObject.Find("ExitGameButton");
        if (!template) return;

        var button = Instantiate(template, null);
        var buttonTransform = button.transform;
        //buttonTransform.localPosition = new Vector3(-2f, -2f);
        button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.458f, 0.124f);

        var passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((Action)(() =>
        {
            StartDownloadRelease(latestRelease);
            button.SetActive(false);
        }));

        var text = button.transform.GetComponentInChildren<TMP_Text>();
        var t = "Update TOU";
        StartCoroutine(Effects.Lerp(0.1f, (Action<float>)(p => text.SetText(t))));
        passiveButton.OnMouseOut.AddListener((Action)(() => text.color = Color.red));
        passiveButton.OnMouseOver.AddListener((Action)(() => text.color = Color.white));
        var announcement =
            $"<size=150%>A new THE OTHER US update to {latestRelease.Tag} is available</size>\n{latestRelease.Description}";
        var mgr = FindObjectOfType<MainMenuManager>(true);
        if (showPopUp)
            mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: "TOU Update",
                date: latestRelease.PublishedAt));
        showPopUp = false;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoShowAnnouncement(string announcement, bool show = true, string shortTitle = "TOU Update",
        string title = "", string date = "")
    {
        var mgr = FindObjectOfType<MainMenuManager>(true);
        var popUpTemplate = FindObjectOfType<AnnouncementPopUp>(true);
        if (popUpTemplate == null)
        {
            Error("couldnt show credits, popUp is null");
            yield return null;
        }

        var popUp = Instantiate(popUpTemplate);

        popUp.gameObject.SetActive(true);

        Announcement creditsAnnouncement = new()
        {
            Id = "touAnnouncement",
            Language = 0,
            Number = 6969,
            Title = title == "" ? "The Other Us Announcement" : title,
            ShortTitle = shortTitle,
            SubTitle = "",
            PinState = false,
            Date = date == "" ? DateTime.Now.Date.ToString() : date,
            Text = announcement
        };
        mgr.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p =>
        {
            if (p == 1)
            {
                var backup = DataManager.Player.Announcements.allAnnouncements;
                DataManager.Player.Announcements.allAnnouncements =
                    new Il2CppSystem.Collections.Generic.List<Announcement>();
                popUp.Init(false);
                DataManager.Player.Announcements.SetAnnouncements(new[] { creditsAnnouncement });
                popUp.CreateAnnouncementList();
                popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                popUp.visibleAnnouncements.Get(0).PassiveButton.OnClick.RemoveAllListeners();
                DataManager.Player.Announcements.allAnnouncements = backup;
            }
        })));
    }*/
}

public class GithubRelease
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("tag_name")] public string Tag { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("draft")] public bool Draft { get; set; }

    [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

    [JsonPropertyName("created_at")] public string CreatedAt { get; set; }

    [JsonPropertyName("published_at")] public string PublishedAt { get; set; }

    [JsonPropertyName("body")] public string Description { get; set; }

    [JsonPropertyName("assets")] public List<GithubAsset> Assets { get; set; }

    public Version Version
    {
        get
        {
            var text = Tag;
            if (text.Contains('v')) text = text.Replace("v", string.Empty);
            return Version.TryParse(text, out var ver) ? ver : new Version(1, 0, 0);
        }
    }

    public bool IsNewer(Version version)
    {
        return Version > version;
    }
}

public class GithubAsset
{
    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("size")] public int Size { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }
}
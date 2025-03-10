using UnityEngine;

namespace TheOtherUs.Modules;

public class BepInExUpdater : MonoBehaviour
{
    /*public const string RequiredBepInExVersion = "6.0.0-be.688+49015217f3becf052d33fa4658ac19229f5daa3a";

    public const string BepInExDownloadURL =
        "https://builds.bepinex.dev/projects/bepinex_be/688/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.688%2B4901521.zip";

    public static bool UpdateRequired => Paths.BepInExVersion.ToString() != RequiredBepInExVersion;

    public void Awake()
    {
        Message("BepInEx Update Required...");
        Message($"{Paths.BepInExVersion}, {RequiredBepInExVersion} ");
        this.StartCoroutine(CoUpdate());
    }

    [HideFromIl2Cpp]
    public IEnumerator CoUpdate()
    {
        Task.Run(() => MessageBox(GetForegroundWindow(), "Required BepInEx update is downloading, please wait...",
            "The Other Roles", 0));
        var www = UnityWebRequest.Get(BepInExDownloadURL);
        yield return www.Send();
        if (www.isNetworkError || www.isHttpError)
        {
            Error(www.error);
            yield break;
        }

        var zipPath = Path.Combine(Paths.GameRootPath, ".bepinex_update");
        File.WriteAllBytes(zipPath, www.downloadHandler.data);


        var tempPath = Path.Combine(Path.GetTempPath(), "TheOtherUpdater.exe");
        var asm = Assembly.GetExecutingAssembly();
        var exeName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("TheOtherUpdater.exe"));

        using (var resource = asm.GetManifestResourceStream(exeName))
        {
            using (var file = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                resource!.CopyTo(file);
            }
        }

        var startInfo = new ProcessStartInfo(tempPath, $"--game-path \"{Paths.GameRootPath}\" --zip \"{zipPath}\"");
        startInfo.UseShellExecute = false;
        Process.Start(startInfo);
        Application.Quit();
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBoxTimeout(IntPtr hwnd, string text, string title, uint type, short wLanguageId,
        int milliseconds);*/
}

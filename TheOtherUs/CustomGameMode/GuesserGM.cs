namespace TheOtherUs.CustomGameMode;

internal class GuesserGM : CustomModeBase
{
    /*// Guesser Gamemode
    public static List<GuesserGM> guessers = [];
    public static Color color = new Color32(255, 255, 0, byte.MaxValue);

    public PlayerControl guesser;
    public int shots = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNumberOfShots.getFloat());

    public GuesserGM(PlayerControl player)
    {
        guesser = player;
        guessers.Add(this);
    }

    public static int remainingShots(byte playerId, bool shoot = false)
    {
        var g = guessers.FindLast(x => x.guesser.PlayerId == playerId);
        if (g == null) return 0;
        if (shoot) g.shots--;
        return g.shots;
    }

    public static void clear(byte playerId)
    {
        var g = guessers.FindLast(x => x.guesser.PlayerId == playerId);
        if (g == null) return;
        g.guesser = null;
        g.shots = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNumberOfShots.getFloat());

        guessers.Remove(g);
    }

    public static void clearAndReload()
    {
        guessers = [];
    }

    public static bool isGuesser(byte playerId)
    {
        return guessers.FindAll(x => x.guesser.PlayerId == playerId).Count > 0;
    }*/
}
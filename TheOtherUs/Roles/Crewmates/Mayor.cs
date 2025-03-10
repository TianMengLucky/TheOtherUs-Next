using UnityEngine;

namespace TheOtherUs.Roles.Crewmates;

[RegisterRole]
public class Mayor : RoleBase
{
    public static readonly RoleInfo roleInfo = new()
    {
        Name = nameof(Mayor),
        Color = new Color32(32, 77, 66, byte.MaxValue),
        CreateRoleController = n => new MayorController(n),
        DescriptionText = "Your vote counts twice",
        IntroInfo = "Your vote counts twice",
        GetRole = Get<Mayor>,
        RoleClassType = typeof(Mayor),
        RoleId = RoleId.Mayor,
        RoleTeam = RoleTeam.Crewmate,
        RoleType = CustomRoleType.Main
    };
    
    public class MayorController(PlayerControl player) : RoleControllerBase(player)
    {
        public override RoleBase _RoleBase => Get<Mayor>();
    }

    public bool canSeeVoteColors;
    public Color color = new Color32(32, 77, 66, byte.MaxValue);
    public Minigame emergency;
    public Sprite emergencySprite;
    public PlayerControl mayor;
    public int mayorChooseSingleVote;
    public bool meetingButton = true;
    public int remoteMeetingsLeft = 1;
    public int tasksNeededToSeeVoteColors;

    public bool voteTwice = true;

    public override RoleInfo RoleInfo { get; protected set; } = roleInfo;
    public override CustomRoleOption roleOption { get; set; }

    public Sprite getMeetingSprite()
    {
        if (emergencySprite) return emergencySprite;
        emergencySprite = UnityHelper.loadSpriteFromResources("TheOtherUs.Resources.EmergencyButton.png", 550f);
        return emergencySprite;
    }

    public override void ClearAndReload()
    {
        mayor = null;
        emergency = null;
        emergencySprite = null;
        remoteMeetingsLeft = Mathf.RoundToInt(CustomOptionHolder.mayorMaxRemoteMeetings);
        canSeeVoteColors = CustomOptionHolder.mayorCanSeeVoteColors;
        tasksNeededToSeeVoteColors = (int)CustomOptionHolder.mayorTasksNeededToSeeVoteColors;
        meetingButton = CustomOptionHolder.mayorMeetingButton;
        mayorChooseSingleVote = CustomOptionHolder.mayorChooseSingleVote;
        voteTwice = true;
    }
}
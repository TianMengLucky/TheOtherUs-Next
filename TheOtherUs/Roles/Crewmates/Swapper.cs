using UnityEngine;

namespace TheOtherUs.Roles.Crewmates;

[RegisterRole]
public class Swapper : RoleBase
{
    public bool canCallEmergency;
    public bool canFixSabotages;
    public bool canOnlySwapOthers;
    public int charges;

    public byte playerId1 = byte.MaxValue;
    public byte playerId2 = byte.MaxValue;
    public float rechargedTasks;
    public float rechargeTasksNumber;
    private ResourceSprite spriteCheck = new("SwapperCheck.png", 150f);
    public PlayerControl swapper;

    public override RoleInfo RoleInfo { get; protected set; } = new()
    {
        Name = nameof(Swapper),
        Color = new Color32(134, 55, 86, byte.MaxValue),
        DescriptionText = "Swap votes",
        IntroInfo = "Swap votes to exile the <color=#FF1919FF>Impostors</color>",
        GetRole = Get<Swapper>,
        RoleClassType = typeof(Swapper),
        RoleId = RoleId.Swapper,
        RoleTeam = RoleTeam.Crewmate,
        RoleType = CustomRoleType.Main,
        CreateRoleController = n => new SwapperController(n)
    };
    
    public class SwapperController(PlayerControl player) : RoleControllerBase(player)
    {
        public override RoleBase _RoleBase => Get<Swapper>();
    }
    public override CustomRoleOption roleOption { get; set; }

    public override void ClearAndReload()
    {
        swapper = null;
        playerId1 = byte.MaxValue;
        playerId2 = byte.MaxValue;
        canCallEmergency = CustomOptionHolder.swapperCanCallEmergency;
        canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers;
        canFixSabotages = CustomOptionHolder.swapperCanFixSabotages;
        charges = Mathf.RoundToInt(CustomOptionHolder.swapperSwapsNumber);
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber);
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber);
    }
}
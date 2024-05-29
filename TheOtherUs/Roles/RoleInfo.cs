using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherUs.Roles;

public class RoleInfo
{
    private static readonly List<RoleInfo> _AllRoleInfo = [];

    public RoleInfo()
    {
        _AllRoleInfo.Add(this);
    }

    public static IReadOnlyList<RoleInfo> AllRoleInfo => _AllRoleInfo;

    public Color Color { get; set; }
    public string Name { get; set; }
    public RoleId RoleId { get; set; }
    public string Description { get; set; }
    public string IntroInfo { get; set; }
    public RoleTeam RoleTeam { get; set; }
    public CustomRoleType RoleType { get; set; } = CustomRoleType.Main;
    public Func<RoleBase> GetRole { get; set; }

    public Func<PlayerControl, RoleControllerBase> CreateRoleController { get; set; }

    public Type RoleClassType { get; set; }
}

public enum RoleTeam
{
    Crewmate,
    Impostor,
    Neutral,
    Special
}

[Flags]
public enum CustomRoleType
{
    Main = 1,
    Modifier = 2,
    ModeRole = 5,
    MainAndModifier = Main | Modifier 
}
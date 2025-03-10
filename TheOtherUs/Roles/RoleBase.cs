using System;
using System.Collections.Generic;

namespace TheOtherUs.Roles;

public abstract class RoleBase : IDisposable
{
    
    public int RoleIndex => CustomRoleManager.Instance._RoleBases.IndexOf(this);

    public string ReadmeText = string.Empty;

    public abstract RoleInfo RoleInfo { get; protected set; }
    public abstract CustomRoleOption roleOption { get; set; }
    public List<RoleControllerBase> Controllers { get; protected set; } = [];
    public string ClassName => RoleInfo.RoleClassType.Name;
    public Type ClassType => RoleInfo.RoleClassType;

    public RoleTeam Team => RoleInfo.RoleTeam;

    public CustomRoleType CustomRoleType => RoleInfo.RoleType;

    public virtual bool CanUseVent { get; set; } = false;
    public virtual bool EnableAssign { get; set; } = true;
    public virtual bool IsVanilla { get; set; } = false;
    public virtual bool HasImpostorVision { get; set; } = false;
    public virtual bool IsKiller { get; set; } = false;
    public virtual bool IsEvil { get; set; } = false;
    public virtual bool HasTask { get; set; } = true;
    public virtual bool CanDoTask { get; set; } = true;
    public virtual RoleWinBase RoleWin { get; set; }
    
    public virtual void Dispose()
    {
    }

    public virtual bool CanAssign()
    {
        return true;
    }

    public virtual void ClearAndReload()
    {
    }

    public virtual void OptionCreate()
    {
    }

    public virtual void ButtonCreate(HudManager manager)
    {
    }


    public virtual void ResetCustomButton()
    {
    }
#nullable enable
    public Type? PathType { get; protected set; } = null;
}

public abstract class VanillaRole : RoleBase
{
    public override bool IsVanilla { get; set; } = true;
    public virtual Type? RoleType { get; set; }
    public RoleBehaviour? RoleBehaviour { get; set; }
}

public interface Invisable
{
    public bool isInvisable { get; set; }
}
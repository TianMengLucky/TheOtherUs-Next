using System;
using System.Collections.Generic;

namespace TheOtherUs.Roles.Modifier;

[RegisterRole]
public class Multitasker : RoleBase
{
    public List<PlayerControl> multitasker = [];

    public override RoleInfo RoleInfo { get; protected set; }
    public override Type RoleType { get; protected set; }

    public override void ClearAndReload()
    {
        multitasker = [];
    }
}
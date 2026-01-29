using System;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;
using UnityEngine;

namespace Scp053.Components;

public abstract class Scp053Component : CustomRole
{
    public abstract override string Name { get; set; }
    public abstract override string Description { get; set; }
    public override string CustomInfo { get; set; } = "SCP-053";
    public abstract override RoleTypeId Role { get; set; }
    public override Vector3 Scale { get; set; } = new(0.8f, 0.8f, 0.8f);
    public override int MaxHealth { get; set; } = 100;
}
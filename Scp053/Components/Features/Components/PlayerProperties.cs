using Exiled.API.Features;
using Scp053.Components.Features.Components.Interfaces;
using UnityEngine;

namespace Scp053.Components.Features.Components;

public class PlayerProperties(Scp053Properties scp053Properties) : IPropertyModule
{
    public Scp053Properties Scp053Properties { get; } = scp053Properties;

    public GameObject HighlightPrefab { get; set; }
    public bool IsInEscapingProcess { get; set; }
    public bool IsInAnomalyRange { get; set; }

    public Player Cuffer { get; set; }
}
using Exiled.API.Features;
using Scp053.Components.Features.Components;
using UnityEngine;

namespace Scp053.Components.Features;

public sealed class Scp053Properties() : MonoBehaviour
{
    private void Awake()
    {
        Player = Player.Get(gameObject);
        PlayerProperties = new PlayerProperties(this);
    }
    
    public Player Player { get; private set; }
    public PlayerProperties PlayerProperties { get; private set; }
    
    public void ResetProperties()
    {
        Destroy(PlayerProperties.HighlightPrefab);
        PlayerProperties.HighlightPrefab = null;
        PlayerProperties.IsInEscapingProcess = false;
        PlayerProperties.Cuffer = null;
        PlayerProperties.IsInAnomalyRange = false;
        Player.IsUsingStamina = true;
    }
}
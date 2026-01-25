using System;
using Exiled.API.Features;
using Scp053.Events.EventArgs.Player;

namespace Scp053.Events
{
    public partial class PlayerEvents
    {
        public event Action<PlayerFullConnectedEventArgs> PlayerFullConnected;
    }

    public partial class PlayerEvents
    {
        public void InvokePlayerFullConnected(Player player)
        {
            var ev = new PlayerFullConnectedEventArgs(player);
            PlayerFullConnected?.Invoke(ev);
        }
    }
}
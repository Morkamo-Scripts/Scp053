namespace Scp053.Events.EventArgs.Player
{
    public class PlayerFullConnectedEventArgs(Exiled.API.Features.Player player)
    {
        public Exiled.API.Features.Player Player => player;
    }
}
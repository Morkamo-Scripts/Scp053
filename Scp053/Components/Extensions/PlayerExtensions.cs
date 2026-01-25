using CommandSystem;
using Exiled.API.Features;
using Scp053.Components.Features;

namespace Scp053.Components.Extensions;

public static class PlayerExtensions
{
    public static Player AsPlayer(this ICommandSender sender)
        => Player.Get(sender);

    public static Scp053Properties Scp053(this Player player)
        => player.ReferenceHub.GetComponent<Scp053Properties>();
}
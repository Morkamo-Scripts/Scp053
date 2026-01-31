using System;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using Scp053.Components.Features;
using Scp053.Events;

namespace Scp053
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Morkamo";
        public override string Name => "SCP-053";
        public override string Prefix => Name;
        public override Version Version => new(1, 1, 0);
        public override Version RequiredExiledVersion { get; } = new(9, 12, 6);
        
        public static Plugin Instance;
        public static Harmony Harmony;
        
        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Config.Scp053ClassD.Register();
            Config.Scp053Chaos.Register();
            Config.Scp053Ntf.Register();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Config.Scp053ClassD.Unregister();
            Config.Scp053Chaos.Unregister();
            Config.Scp053Ntf.Unregister();
            Instance = null;
            base.OnDisabled();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC)
                return;
            
            if (ev.Player.ReferenceHub.gameObject.GetComponent<Scp053Properties>() != null)
                return;

            ev.Player.ReferenceHub.gameObject.AddComponent<Scp053Properties>();
            
            EventManager.PlayerEvents.InvokePlayerFullConnected(ev.Player);
        }
    }
}
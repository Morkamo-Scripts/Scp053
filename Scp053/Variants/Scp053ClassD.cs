using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdvancedCassie.Components.Extensions;
using AdvancedInterfaces.Components;
using AdvancedInterfaces.Events;
using AdvancedInterfaces.Events.EventArgs.Player;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp330;
using Exiled.Events.EventArgs.Scp939;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using RueI.API;
using RueI.API.Elements;
using Scp053.Components;
using Scp053.Components.Extensions;
using Scp053.Components.Features;
using UnityEngine;
using events = Exiled.Events.Handlers;
using Object = UnityEngine.Object;
using Scp079Role = Exiled.API.Features.Roles.Scp079Role;

namespace Scp053.Variants;

public class Scp053ClassD : Scp053Component
{
    public override uint Id { get; set; } = 10;
    public override string Name { get; set; } = "SCP-053-ClassD";

    public override string Description { get; set; } = "<size=30><b><color=#00ffae>Ты теперь <b>SCP-053 (Маленькая девочка)</color></b>\n" +
                                                       "<color=#ff001e>Вы не сможете спрятаться от SCP-939!</color>\n" +
                                                       "<color=#ff3381>~ <i>Твои умения - лекарство для одних и яд для других.</i></color>\n" +
                                                       "<color=#ff8c00>Лучший иммунитет - невосприимчива к любым негативным эффектам.</color>\n" +
                                                       "<color=#ff8120>Сова - эффект легкого ночного зрения.</color>\n" +
                                                       "<color=#ff7640>Панацея - пассивное лечения себя (1Hp/0.5s) и союзников (1Hp/s).</color>\n" +
                                                       "<color=#ff6b60>Зеркало - возвращает 15% получаемого урона атакующему игроку.</color>\n" +
                                                       "<color=#ff5f7c>Сила жизни - вы можете отразить 100% критического урона \nраз в 60 секунд.</color>\n" +
                                                       "<color=#ff548f>Любовь к сладкому - вы можете взять до 6 конфет SCP-330.</color>\n" +
                                                       "<color=#ff3b68>Хороший доктор - медикаменты действуют эффективнее на вас \nи ваших союзников.</color>\n" +
                                                       "<color=#0883ff><size=40><b>Если SCP-053 связали, то она позитивно влияет\nна всю команду связавшего её игрока!</b></color></size>";

    public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
    
    private float _lastDamageTime;
    private const float CooldownSeconds = 60f;
    private const float AnomalyRangeRadius = 5;
    private bool _isScp191Died;

    private Coroutine _hsProcessor;
    private Coroutine _stProcessor;
    private Coroutine _hProcessor;
    private Coroutine _arProcessor;
    private Coroutine _phProcessor;
    private Coroutine _cProcessor;

    protected override void SubscribeEvents()
    {
        EventManager.PlayerEvents.CustomRoleTypeDied += OnCustomRoleDied;
        events.Player.Spawned += OnSpawned;
        events.Player.Hurt += OnHurt;
        events.Player.ReceivingEffect += OnReceivingEffect;
        events.Player.Dying += OnDying;
        events.Player.UsingItem += OnUsingItem;
        events.Scp939.ValidatingVisibility += OnValidatingVisibilityTargetFor939;
        events.Player.Died += OnDied;
        events.Player.Escaping += OnEscaping;
        events.Player.Escaped += OnEscaped;
        events.Scp330.InteractingScp330 += OnIntercatingWithScp330;
        events.Player.UsingItemCompleted += UsingItemComplete;
        events.Player.ChangingRole += OnChangingRole;
        LabApi.Events.Handlers.PlayerEvents.Cuffed += OnCuffed;
        LabApi.Events.Handlers.PlayerEvents.Uncuffed += OnUncuffed;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        EventManager.PlayerEvents.CustomRoleTypeDied -= OnCustomRoleDied;
        events.Player.Spawned -= OnSpawned;
        events.Player.Hurt -= OnHurt;
        events.Player.ReceivingEffect -= OnReceivingEffect;
        events.Player.Dying -= OnDying;
        events.Player.UsingItem -= OnUsingItem;
        events.Scp939.ValidatingVisibility -= OnValidatingVisibilityTargetFor939;
        events.Player.Died -= OnDied;
        events.Player.Escaping -= OnEscaping;
        events.Player.Escaped -= OnEscaped;
        events.Scp330.InteractingScp330 -= OnIntercatingWithScp330;
        events.Player.UsingItemCompleted -= UsingItemComplete;
        events.Player.ChangingRole -= OnChangingRole;
        LabApi.Events.Handlers.PlayerEvents.Cuffed -= OnCuffed;
        LabApi.Events.Handlers.PlayerEvents.Uncuffed -= OnUncuffed;
        base.UnsubscribeEvents();
    }
    
    protected override void RoleRemoved(Player player)
    {
        base.RoleRemoved(player);
        player.AdvancedCassie().PlayerProperties.IsCustomScp = false;
        Object.Destroy(player.Scp053().PlayerProperties.HighlightPrefab);
        player.Scp053().PlayerProperties.HighlightPrefab = null;
    }
    
    private void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (Check(ev.Player))
            ev.Player.Scp053().ResetProperties();
    }
    
    private void OnSpawned(SpawnedEventArgs ev)
    {
        Timing.CallDelayed(0.1f, () =>
        {
            if (ev.Player.IsNPC || !Check(ev.Player))
                return;
            
            var props = ev.Player.Scp053().PlayerProperties;
            
            if (props.HighlightPrefab != null)
            {
                Object.Destroy(props.HighlightPrefab);
                props.HighlightPrefab = null;
            }

            if (_hsProcessor != null)
            {
                CoroutineRunner.Stop(_hsProcessor);
                _hsProcessor = null;
            }
            
            if (_stProcessor != null)
            {
                CoroutineRunner.Stop(_stProcessor);
                _stProcessor = null;
            }
            
            if (_hProcessor != null)
            {
                CoroutineRunner.Stop(_hProcessor);
                _hProcessor = null;
            }
            
            if (_arProcessor != null)
            {
                CoroutineRunner.Stop(_arProcessor);
                _arProcessor = null;
            }
            
            if (_phProcessor != null)
            {
                CoroutineRunner.Stop(_phProcessor);
                _phProcessor = null;
            }
            
            if (_cProcessor != null)
            {
                CoroutineRunner.Stop(_cProcessor);
                _cProcessor = null;
            }

            _hsProcessor = CoroutineRunner.Run(HealSelfProcessor(ev.Player));
            _stProcessor = CoroutineRunner.Run(StaminaProcessor(ev.Player));
            _hProcessor = CoroutineRunner.Run(HintsProcessor(ev.Player));
            _arProcessor = CoroutineRunner.Run(AnomalyRangeProcessor(ev.Player));
            _phProcessor = CoroutineRunner.Run(PlayerHealthProcessor(ev.Player));
            _cProcessor = CoroutineRunner.Run(CuffProcessor(ev.Player));
            
            ev.Player.EnableEffect(EffectType.NightVision, 15);
            ev.Player.EnableEffect(EffectType.Vitality, 255);
            ev.Player.Scale = new Vector3(0.8f, 0.8f, 0.8f);
            ev.Player.AdvancedCassie().PlayerProperties.IsCustomScp = true;
            
            Scp079Role.TurnedPlayers.Add(ev.Player);
            
            InitGlow(ev.Player);
        });
    }
    
    private void UsingItemComplete(UsingItemCompletedEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;
        
        if (ev.Usable.Type == ItemType.Medkit)
        {
            ev.Player.Heal(35f);
        }
    }
    
    private void OnIntercatingWithScp330(InteractingScp330EventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (ev.UsageCount < 6)
            ev.ShouldSever = false;

        ev.Candy = Enum.GetValues(typeof(CandyKindID)).ToArray<CandyKindID>()
            .Where(c => 
                c != CandyKindID.Pink && 
                c != CandyKindID.Black &&
                c != CandyKindID.White &&
                c != CandyKindID.Brown &&
                c != CandyKindID.Evil &&
                c != CandyKindID.Gray &&
                c != CandyKindID.None &&
                c != CandyKindID.Orange)
            .GetRandomValue();
    }
    
    private void OnReceivingEffect(ReceivingEffectEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsConnected || ev.Player.IsNPC || !Check(ev.Player))
            return;
        
        if (ev.Effect.GetEffectType().IsNegative() && ev.Intensity != 0)
        {
            ev.IsAllowed = false;
            return;
        }
        
        if (ev.Effect.GetEffectType() == EffectType.NightVision && ev.Intensity == 0)
        {
            ev.IsAllowed = false;
            return;
        }
        
        if (ev.Effect.GetEffectType() == EffectType.Vitality && ev.Intensity < 255 && ev.Duration > 0)
            ev.IsAllowed = false;
    }
    
    private void OnValidatingVisibilityTargetFor939(ValidatingVisibilityEventArgs ev)
    {
        if (Check(ev.Target))
        {
            ev.IsLateSeen = true;
            ev.IsAllowed = true;
        }
    }
    
    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (Plugin.Instance.Config.NotAllowedItems.Contains(ev.Usable.Type))
        {
            ev.IsAllowed = false;
                
            RueDisplay.Get(ev.Player).Show(
                new Tag(),
                new BasicElement(900, "<size=50><b><color=#ff7d00>Этот предмет запрещен для использования у\n\n SCP-053</color></b></size>"), 3);
                
            Timing.CallDelayed(3.1f, () => RueDisplay.Get(ev.Player).Update());

            foreach (var player in ev.Player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(player).Show(
                    new Tag(),
                    new BasicElement(900, "<size=50><b><color=#ff7d00>Этот предмет запрещен для использования у\n\n SCP-053</color></b></size>"), 3);
                    
                Timing.CallDelayed(3.1f, () => RueDisplay.Get(player).Update());
            }
        }
    }
    
    private void OnHurt(HurtEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;
        
        if (ev.Attacker is null)
            return;
        
        if (ev.Attacker.IsScp && ev.Attacker.Role != RoleTypeId.Scp0492)
            return;
        
        ev.Attacker.Hurt(ev.Amount * 0.15f, "SCP-053 (Зеркальный урон)");
    }

    private void OnDying(DyingEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (ev.Player.IsEffectActive<AntiScp207>())
            return;
        
        var now = Time.time;
        if (now - _lastDamageTime < CooldownSeconds)
            return;
        
        _lastDamageTime = now;

        ev.IsAllowed = false;
        ev.Player.Health = 1f;
        ev.Player.PlayShieldBreakSound();
        
        ev.Player.EnableEffect(EffectType.DamageReduction, 255, 2f);

        Timing.CallDelayed(0.2f, () =>
        {
            if (!ev.Player.IsDead)
            {
                RueDisplay.Get(ev.Player).Show(
                    new Tag(),
                    new BasicElement(900,
                        "<size=40><b><color=#5effec>Ваше умение 'Зеркало' отразило критический урон!</color></b></size>"),
                    5);

                Timing.CallDelayed(5.3f, () => RueDisplay.Get(ev.Player).Update());
            }
        });

        foreach (var player in ev.Player.CurrentSpectatingPlayers)
        {
            if (!ev.Player.IsDead)
            {
                RueDisplay.Get(player).Show(
                    new Tag(),
                    new BasicElement(900,
                        "<size=40><b><color=#5effec>Умение 'Зеркало' отразило критический урон\n\nдля SCP-053!</color></b></size>"),
                    5);

                Timing.CallDelayed(5.3f, () => RueDisplay.Get(player).Update());
            }
        }
        
        var effector = new GameObject()
        {
            transform =
            {
                position = ev.Player.Transform.position - new Vector3(0, 0.65f, 0)
            }
        };
        
        HighlightManager.ProceduralExplosionParticles(effector, 
            new Color32(20, 90, 255, 255), 
            40,
            new Vector3(1f, 1f, 1f), 
            0.25f, 
            10, 
            2, 4, 2);
    }
    
    private void OnDied(DiedEventArgs ev)
    {
        if (!Check(ev.Player))
            return;
        
        ev.Player.IsUsingStamina = true;
        
        EventManager.PlayerEvents.InvokeCustomRoleDied(ev.Player, CustomRoleType.Scp053);
    }
    
    private void OnCustomRoleDied(CustomRoleDiedEventArgs ev)
    {
        if (ev.Role == CustomRoleType.Scp191)
            _isScp191Died = true;

        if (ev.Role == CustomRoleType.Scp053)
            ev.Player.AdvancedCassie().PlayerProperties.IsCustomScp = false;
    }
    
    private void OnEscaping(EscapingEventArgs ev)
    {
        if (Check(ev.Player))
            ev.Player.Scp053().PlayerProperties.IsInEscapingProcess = true;
    }
    
    private void OnEscaped(EscapedEventArgs ev)
    {
        var props = ev.Player.Scp053().PlayerProperties;
        if (props.IsInEscapingProcess)
        {
            props.IsInEscapingProcess = false;
            
            if (ev.EscapeScenario == EscapeScenario.ClassD)
                Get(11)?.AddRole(ev.Player);
            else if (ev.EscapeScenario == EscapeScenario.CuffedClassD)
            {
                Get(12)?.AddRole(ev.Player);
            }
        }
    }

    private void OnCuffed(PlayerCuffedEventArgs ev)
    {
        if (ev.Target.IsNpc || !Check(ev.Target))
            return;

        Player.Get(ev.Target).Scp053().PlayerProperties.Cuffer = ev.Player;
    }
    
    private void OnUncuffed(PlayerUncuffedEventArgs ev)
    {
        if (ev.Target.IsNpc || !Check(ev.Target))
            return;

        Player.Get(ev.Target).Scp053().PlayerProperties.Cuffer = null;
    }

    private void InitGlow(Player player)
    {
        var properties = player.Scp053().PlayerProperties;
        properties.HighlightPrefab = new GameObject()
        {
            transform =
            {
                position = player.Transform.position - new Vector3(0, 0.65f, 0)
            }
        };
        properties.HighlightPrefab.transform.SetParent(player.Transform);
        
        HighlightManager.ProceduralParticles(properties.HighlightPrefab, 
            new Color32(20, 200, 255, 255), 0, 0.05f,
            new(1.2f, 1.2f, 1.2f), 0.125f, 12, 8, 60, 1f);
    }
    
    private void Apply053Effect(Player player, bool isDamage)
    {
        var effector = new GameObject
        {
            transform =
            {
                position = player.Transform.position - new Vector3(0, 0.7f, 0)
            }
        };

        if (isDamage)
        {
            if (player.CurrentRoom.Type == RoomType.Lcz914)
                return;
            
            player.Hurt(1f, "SCP-053 (Когнитивный урон)");

            HighlightManager.ProceduralExplosionParticles(
                effector,
                new Color32(255, 40, 50, 255),
                3,
                new Vector3(0.4f, 0.4f, 0.4f),
                0.15f,
                8,
                1, 1, 1);
        }
        else
        {
            if (player.Health >= player.MaxHealth)
                return;
            
            player.Heal(1f);
            
            HighlightManager.ProceduralExplosionParticles(
                effector,
                new Color32(50, 255, 130, 255),
                3,
                new Vector3(0.4f, 0.4f, 0.4f),
                0.15f,
                8,
                1, 1, 1);
        }
    }

    private IEnumerator HealSelfProcessor(Player player)
    {
        while (player.IsConnected && Check(player))
        {
            player.Heal(1f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator StaminaProcessor(Player player)
    {
        while (player.IsConnected && Check(player))
        {
            if (_isScp191Died && player.IsUsingStamina)
                player.ResetStamina();
            
            player.IsUsingStamina = !_isScp191Died;
            yield return new WaitForSeconds(0.25f);
        }
    }
    
    private IEnumerator HintsProcessor(Player player)
    {
        RueDisplay.Get(player).Show(
            new Tag(),
            new BasicElement(600, Description), 35);

        Timing.CallDelayed(35.1f, () => RueDisplay.Get(player).Update());
            
        foreach (var spec in player.CurrentSpectatingPlayers)
        {
            RueDisplay.Get(spec).Show(
                new Tag(),
                new BasicElement(600, Description), 35);
            
            Timing.CallDelayed(35.1f, () => RueDisplay.Get(spec).Update());
        }
        
        while (player.IsConnected && player.IsAlive && Check(player))
        {
            foreach (var spec in player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(spec).Show(
                    new Tag(),
                    new BasicElement(120, "<align=right><size=30><b><color=#ffc233>Игрок играет за SCP-053</color></b></size>"), 1.1f);
                    
                Timing.CallDelayed(1.2f, () => RueDisplay.Get(spec).Update());
            }
                
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator AnomalyRangeProcessor(Player owner)
    {
        while (owner.IsConnected && Check(owner))
        {
            var center = owner.Position;

            foreach (var player in Player.List.Where(pl => pl.IsHuman || pl.Role.Type == RoleTypeId.Scp0492))
            {
                if (!player.IsAlive)
                    continue;

                if (player == owner)
                    continue;

                if (Vector3.Distance(player.Position, center) > AnomalyRangeRadius)
                {
                    player.Scp053().PlayerProperties.IsInAnomalyRange = false;
                    Scp079Role.TurnedPlayers.Remove(player);
                    continue;
                }

                player.Scp053().PlayerProperties.IsInAnomalyRange = true;
                Scp079Role.TurnedPlayers.Add(player);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator PlayerHealthProcessor(Player owner)
    {
        while (owner.IsConnected && Check(owner))
        {
            var cuffer = owner.Scp053().PlayerProperties.Cuffer;

            foreach (var player in Player.List.Where(pl =>
                         pl.IsAlive &&
                         pl.Scp053().PlayerProperties.IsInAnomalyRange &&
                         pl != owner))
            {
                bool isFriendly = cuffer == null
                    ? player.LeadingTeam == owner.LeadingTeam
                    : player.LeadingTeam == cuffer.LeadingTeam;

                Apply053Effect(player, isDamage: !isFriendly);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator CuffProcessor(Player player)
    {
        while (player.IsConnected && Check(player))
        {
            if (!player.IsCuffed)
                player.Scp053().PlayerProperties.Cuffer = null;
            
            yield return new WaitForSeconds(1f);
        }
    }
}
using Content.Client.Backmen.UserInterface.Systems.Targeting.Widgets;
using Content.Client.Gameplay;
using Content.Shared.Targeting;
using Content.Client.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;

namespace Content.Client.Backmen.UserInterface.Systems.Targeting;

public sealed class TargetingUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetingSystem>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private TargetingComponent? _targetingComponent;
    private TargetingControl? TargetingControl => UIManager.GetActiveUIWidgetOrNull<TargetingControl>();

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.TargetingStartup += AddTargetingControl;
        system.TargetingShutdown += RemoveTargetingControl;
        system.TargetChange += CycleTarget;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.TargetingStartup -= AddTargetingControl;
        system.TargetingShutdown -= RemoveTargetingControl;
        system.TargetChange -= CycleTarget;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (TargetingControl != null)
        {
            TargetingControl.SetVisible(_targetingComponent != null);

            if (_targetingComponent != null)
                TargetingControl.SetColors(_targetingComponent.Target);
        }
    }

    public void AddTargetingControl(TargetingComponent component)
    {
        _targetingComponent = component;

        if (TargetingControl != null)
        {
            TargetingControl.SetVisible(_targetingComponent != null);

            if (_targetingComponent != null)
                TargetingControl.SetColors(_targetingComponent.Target);
        }

    }

    public void RemoveTargetingControl()
    {
        if (TargetingControl != null)
            TargetingControl.SetVisible(false);

        _targetingComponent = null;
    }

    public void CycleTarget(TargetBodyPart bodyPart)
    {
        if (_playerManager.LocalEntity is not { } user
        || _entManager.GetComponent<TargetingComponent>(user) is not { } targetingComponent
        || TargetingControl == null)
            return;

        var player = _entManager.GetNetEntity(user);
        if (bodyPart != targetingComponent.Target)
        {
            var msg = new TargetChangeEvent(player, bodyPart);
            _net.SendSystemNetworkMessage(msg);
            TargetingControl?.SetColors(bodyPart);
        }
    }


}
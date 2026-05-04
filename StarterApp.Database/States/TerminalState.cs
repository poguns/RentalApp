using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Rental completed successfully. Terminal state — no further transitions.
/// </summary>
public class CompletedState : RentalStateBase
{
    public override string StateName => RentalStatus.Completed;
}

/// <summary>
/// Rental was rejected by the owner. Terminal state — no further transitions.
/// </summary>
public class RejectedState : RentalStateBase
{
    public override string StateName => RentalStatus.Rejected;
}
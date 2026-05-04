using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Rental has been requested but not yet reviewed by the owner.
/// Valid transitions: Approved, Rejected
/// </summary>
public class RequestedState : RentalStateBase
{
    public override string StateName => RentalStatus.Pending;

    public override IRentalState Approve(Rental rental)
    {
        rental.Status = RentalStatus.Approved;
        rental.UpdatedAt = DateTime.UtcNow;
        return new ApprovedState();
    }

    public override IRentalState Reject(Rental rental)
    {
        rental.Status = RentalStatus.Rejected;
        rental.UpdatedAt = DateTime.UtcNow;
        return new RejectedState();
    }
}
using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Item is overdue — end date has passed but item hasn't been returned.
/// Valid transitions: Returned
/// </summary>
public class OverdueState : RentalStateBase
{
    public override string StateName => RentalStatus.Overdue;

    public override IRentalState MarkReturned(Rental rental)
    {
        rental.Status = RentalStatus.Returned;
        rental.UpdatedAt = DateTime.UtcNow;
        return new ReturnedState();
    }
}
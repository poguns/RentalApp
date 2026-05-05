using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Rental has been approved by the owner.
/// Valid transitions: OutForRent
/// </summary>
public class ApprovedState : RentalStateBase
{
    public override string StateName => RentalStatus.Approved;

    public override IRentalState MarkOutForRent(Rental rental)
    {
        if (rental.StartDate.Date > DateTime.Today)
            throw new InvalidOperationException(
                "Cannot mark as out for rent before the start date.");

        rental.Status = RentalStatus.OutForRent;
        rental.UpdatedAt = DateTime.UtcNow;
        return new OutForRentState();
    }
}
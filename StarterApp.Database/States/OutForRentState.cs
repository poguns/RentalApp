using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Item is currently with the borrower.
/// Valid transitions: Returned
/// </summary>
public class OutForRentState : RentalStateBase
{
    public override string StateName => RentalStatus.OutForRent;

    public override IRentalState MarkReturned(Rental rental)
    {
        rental.Status = RentalStatus.Returned;
        rental.UpdatedAt = DateTime.UtcNow;
        return new ReturnedState();
    }
}
using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Item has been returned by the borrower.
/// Valid transitions: Completed
/// </summary>
public class ReturnedState : RentalStateBase
{
    public override string StateName => RentalStatus.Returned;

    public override IRentalState Complete(Rental rental)
    {
        rental.Status = RentalStatus.Completed;
        rental.UpdatedAt = DateTime.UtcNow;
        return new CompletedState();
    }
}
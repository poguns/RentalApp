using StarterApp.Database.Models;

namespace StarterApp.Database.States;

public abstract class RentalStateBase : IRentalState
{
    public abstract string StateName { get; }

    public virtual IRentalState Approve(Rental rental) =>
        throw new InvalidOperationException(
            $"Cannot approve a rental in '{StateName}' state.");

    public virtual IRentalState Reject(Rental rental) =>
        throw new InvalidOperationException(
            $"Cannot reject a rental in '{StateName}' state.");

    public virtual IRentalState MarkOutForRent(Rental rental) =>
        throw new InvalidOperationException(
            $"Cannot mark as out for rent in '{StateName}' state.");

    public virtual IRentalState MarkReturned(Rental rental) =>
        throw new InvalidOperationException(
            $"Cannot mark as returned in '{StateName}' state.");

    public virtual IRentalState Complete(Rental rental) =>
        throw new InvalidOperationException(
            $"Cannot complete a rental in '{StateName}' state.");
}
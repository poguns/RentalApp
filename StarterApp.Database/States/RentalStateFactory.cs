using StarterApp.Database.Models;

namespace StarterApp.Database.States;

/// <summary>
/// Factory that resolves the correct state object from a rental's current status string.
/// </summary>
public static class RentalStateFactory
{
    public static IRentalState GetState(string status) => status switch
    {
        RentalStatus.Pending    => new RequestedState(),
        RentalStatus.Approved   => new ApprovedState(),
        RentalStatus.OutForRent => new OutForRentState(),
        RentalStatus.Returned   => new ReturnedState(),
        RentalStatus.Completed  => new CompletedState(),
        RentalStatus.Rejected   => new RejectedState(),
        _ => throw new ArgumentException($"Unknown rental status: {status}")
    };
}
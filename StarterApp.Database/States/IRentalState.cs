using StarterApp.Database.Models;

namespace StarterApp.Database.States;

public interface IRentalState
{
    string StateName { get; }
    IRentalState Approve(Rental rental);
    IRentalState Reject(Rental rental);
    IRentalState MarkOutForRent(Rental rental);
    IRentalState MarkReturned(Rental rental);
    IRentalState Complete(Rental rental);
    IRentalState MarkOverdue(Rental rental);
}
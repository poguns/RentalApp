using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Database.States;

namespace StarterApp.Services;

/// <summary>
/// Detects rentals that have passed their end date and transitions them to Overdue.
/// Call CheckForOverdueRentalsAsync periodically or on app resume.
/// </summary>
public class OverdueService
{
    private readonly IRentalRepository _rentalRepository;

    public OverdueService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public async Task<List<Rental>> CheckForOverdueRentalsAsync()
    {
        var allRentals = await _rentalRepository.GetAllAsync();

        var overdueRentals = allRentals
            .Where(r => r.Status == RentalStatus.OutForRent
                     && r.EndDate.Date < DateTime.Today)
            .ToList();

        foreach (var rental in overdueRentals)
        {
            var state = RentalStateFactory.GetState(rental.Status);
            state.MarkOverdue(rental);
            await _rentalRepository.UpdateAsync(rental);
        }

        return overdueRentals;
    }
}
using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.States;

namespace StarterApp.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authService;

    public RentalService(IRentalRepository rentalRepository, IItemRepository itemRepository, IAuthenticationService authService)
    {
        _rentalRepository = rentalRepository;
        _itemRepository = itemRepository;
        _authService = authService;
    }

    public async Task<bool> CanRentItem(int itemId, DateTime startDate, DateTime endDate)
    {
        // Check for date overlaps with existing approved rentals
        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId);
        return !existingRentals.Any(r =>
            r.Status == RentalStatus.Approved &&
            r.StartDate < endDate &&
            r.EndDate > startDate);
    }

    public async Task<Rental> RequestRental(int itemId, int borrowerId, DateTime startDate, DateTime endDate)
    {
        // Validate dates
        if (startDate >= endDate)
            throw new ArgumentException("End date must be after start date");

        if (startDate < DateTime.Today)
            throw new ArgumentException("Start date cannot be in the past");

        // Check availability
        var available = await CanRentItem(itemId, startDate, endDate);
        if (!available)
            throw new InvalidOperationException("Item is not available for these dates");

        var rental = new Rental
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            StartDate = startDate,
            EndDate = endDate,
            Status = RentalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return await _rentalRepository.CreateAsync(rental);
    }

    public Task<List<Rental>> GetIncomingRentalsAsync()
    {
        var ownerId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");
        return _rentalRepository.GetByOwnerIdAsync(ownerId);
    }

    public Task<List<Rental>> GetOutgoingRentalsAsync()
    {
        var borrowerId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");
        return _rentalRepository.GetByBorrowerIdAsync(borrowerId);
    }

    public async Task ApproveRental(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        if (rental.Item?.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the item owner can approve rentals");

        var state = RentalStateFactory.GetState(rental.Status);
        state.Approve(rental);
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task RejectRental(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        if (rental.Item?.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the item owner can reject rentals");

        var state = RentalStateFactory.GetState(rental.Status);
        state.Reject(rental);
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task ReturnRental(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        var state = RentalStateFactory.GetState(rental.Status);
        state.MarkReturned(rental);
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task MarkOutForRentAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        var state = RentalStateFactory.GetState(rental.Status);
        state.MarkOutForRent(rental);
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task CompleteRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        var state = RentalStateFactory.GetState(rental.Status);
        state.Complete(rental);
        await _rentalRepository.UpdateAsync(rental);
    }
}
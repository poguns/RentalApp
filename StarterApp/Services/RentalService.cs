using StarterApp.Database.Models;
using StarterApp.Database.Data.Repositories;

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
            r.Status == "Approved" &&
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
            Status = "Pending",
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

        //rental status
        if (!RentalStatus.CanTransitionTo(rental.Status, RentalStatus.Approved))
            throw new InvalidOperationException(
                $"Cannot approve a rental with status: {rental.Status}");

        // Only the item owner can approve
        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        if (rental.Item?.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the item owner can approve rentals");

        rental.Status = RentalStatus.Approved;
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task RejectRental(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        if (!RentalStatus.CanTransitionTo(rental.Status, RentalStatus.Rejected))
            throw new InvalidOperationException(
                $"Cannot reject a rental with status: {rental.Status}");

        var currentUserId = _authService.CurrentUser?.Id
            ?? throw new InvalidOperationException("Not authenticated");

        if (rental.Item?.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the item owner can reject rentals");

        rental.Status = RentalStatus.Rejected;
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task ReturnRental(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        rental.Status = RentalStatus.Returned;
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task MarkOutForRentAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        if (!RentalStatus.CanTransitionTo(rental.Status, RentalStatus.OutForRent))
            throw new InvalidOperationException(
                $"Cannot mark as out for rent with status: {rental.Status}");

        if (rental.StartDate.Date > DateTime.Today)
            throw new InvalidOperationException(
                "Cannot mark as out for rent before the start date");

        rental.Status = RentalStatus.OutForRent;
        await _rentalRepository.UpdateAsync(rental);
    }

    public async Task CompleteRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found");

        if (!RentalStatus.CanTransitionTo(rental.Status, RentalStatus.Completed))
            throw new InvalidOperationException(
                $"Cannot complete a rental with status: {rental.Status}");

        rental.Status = RentalStatus.Completed;
        await _rentalRepository.UpdateAsync(rental);
    }
}
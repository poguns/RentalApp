using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarterApp.Database.Models;

public class Rental
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Required]
    public int BorrowerId { get; set; }

    [ForeignKey(nameof(BorrowerId))]
    public User? Borrower { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Calculations for number of days and cost, not stored in the database
    [NotMapped]
    public int TotalDays => (EndDate - StartDate).Days;

    [NotMapped]
    public decimal TotalCost => TotalDays * (Item?.DailyRate ?? 0);
}
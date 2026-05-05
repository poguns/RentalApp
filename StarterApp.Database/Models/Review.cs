using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarterApp.Database.Models;

public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Required]
    public int ReviewerId { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public User? Reviewer { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //calculated helper for review
    [NotMapped]
    public string RatingDisplay => new string('★', Rating) + new string('☆', 5 - Rating);
}
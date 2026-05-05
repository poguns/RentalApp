using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarterApp.Database.Models;

public static class RentalStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string OutForRent = "OutForRent";
    public const string Returned = "Returned";
    public const string Completed = "Completed";
    public const string Overdue = "Overdue";

    //transitions map, what each status moves to
    public static readonly Dictionary<string, List<string>> ValidTransitions = new()
    {
        { Pending,    new List<string> { Approved, Rejected } },
        { Approved,   new List<string> { OutForRent } },
        { OutForRent, new List<string> { Returned, Overdue } },
        { Returned,   new List<string> { Completed } },
        { Overdue, new List<string> { Returned } },
        { Rejected,   new List<string>() },
        { Completed,  new List<string>() }
    };

    public static bool CanTransitionTo(string currentStatus, string newStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var validNext)
               && validNext.Contains(newStatus);
    }
}
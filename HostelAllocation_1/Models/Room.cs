using System.ComponentModel.DataAnnotations;

namespace HostelAllocation.Models;

public class Room
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Room number is required.")]
    [StringLength(20, ErrorMessage = "Room number cannot exceed 20 characters.")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room type is required.")]
    [RegularExpression("^(Single|Shared)$",
        ErrorMessage = "Room type must be Single or Shared.")]
    public string RoomType { get; set; } = string.Empty;

    [Range(1, int.MaxValue,
        ErrorMessage = "Capacity must be greater than zero.")]
    public int Capacity { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Occupied beds cannot be negative.")]
    public int OccupiedBeds { get; set; }

    [Range(typeof(decimal), "0", "999999999",
        ErrorMessage = "Monthly fee must be non-negative.")]
    public decimal MonthlyFee { get; set; }

    public bool IsValidOccupiedBeds()
    {
        return OccupiedBeds <= Capacity;
    }
}
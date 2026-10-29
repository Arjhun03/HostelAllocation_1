using System.ComponentModel.DataAnnotations;

namespace HostelAllocation.Models;

public class Student
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Student name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Register number is required.")]
    [StringLength(20)]
    public string RegisterNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(50)]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}
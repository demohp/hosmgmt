namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a login entry in the system.
/// </summary>
public class LoginTable
{
    public int LoginId { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Type { get; set; } // 1=Patient, 2=Doctor, 3=Admin

    // Navigation properties
    public Patient? Patient { get; set; }
}

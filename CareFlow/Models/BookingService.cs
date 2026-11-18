namespace CareFlow.Models;

public class BookingService
{
    public int BookingId { get; set; }

    public int ServiceId { get; set; }

    public Booking Booking { get; set; } = null!;

    public DiagnosticService DiagnosticService { get; set; } = null!;
}
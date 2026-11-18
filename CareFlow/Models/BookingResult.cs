namespace CareFlow.Models;

public class BookingResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public int? BookingId { get; init; }
}
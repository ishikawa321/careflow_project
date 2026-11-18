namespace CareFlow.Models;

public class ServiceAvailability
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }

    public int BookedCount { get; set; }

    public int RemainingCapacity => DailyCapacity - BookedCount;

    public bool IsAvailable => RemainingCapacity > 0;
}
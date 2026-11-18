using System.Collections.Generic;

namespace CareFlow.Models;

public class DiagnosticService
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }

    public ICollection<BookingService> BookingServices { get; set; }
        = new List<BookingService>();
}
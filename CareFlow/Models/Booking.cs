using System;
using System.Collections.Generic;

namespace CareFlow.Models;

public class Booking
{
    public int BookingId { get; set; }

    public int PatientId { get; set; }

    public DateOnly BookingDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;

    public ICollection<BookingService> BookingServices { get; set; }
        = new List<BookingService>();
}
using System;
using System.Collections.Generic;

namespace CareFlow.Models;

public class BookingSummary
{
    public int BookingId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateOnly BookingDate { get; set; }

    public List<string> ServiceNames { get; set; } = new();

    public string ServicesText => string.Join(", ", ServiceNames);

    public string BookingDateText => BookingDate.ToString("yyyy-MM-dd");
}
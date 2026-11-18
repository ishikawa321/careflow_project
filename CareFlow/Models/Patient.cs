using System.Collections.Generic;

namespace CareFlow.Models;

public class Patient
{
    public int PatientId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}
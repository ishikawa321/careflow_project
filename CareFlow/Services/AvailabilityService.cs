using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareFlow.Data;
using CareFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Services;

public class AvailabilityService
{
    private readonly IDbContextFactory<CareFlowDbContext> _contextFactory;

    public AvailabilityService(
        IDbContextFactory<CareFlowDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ServiceAvailability>> GetAvailabilityAsync(
        DateOnly bookingDate,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.DiagnosticServices
            .AsNoTracking()
            .OrderBy(service => service.Name)
            .Select(service => new ServiceAvailability
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                DailyCapacity = service.DailyCapacity,

                BookedCount = service.BookingServices.Count(
                    bookingService =>
                        bookingService.Booking.BookingDate == bookingDate)
            })
            .ToListAsync(cancellationToken);
    }
}
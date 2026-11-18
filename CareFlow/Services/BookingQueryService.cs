using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareFlow.Data;
using CareFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Services;

public class BookingQueryService
{
    private readonly IDbContextFactory<CareFlowDbContext> _contextFactory;

    public BookingQueryService(
        IDbContextFactory<CareFlowDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Patient>> GetPatientsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BookingSummary>> GetBookingsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        var bookings = await context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Patient)
            .Include(booking => booking.BookingServices)
                .ThenInclude(item => item.DiagnosticService)
            .OrderByDescending(booking => booking.BookingDate)
            .ThenByDescending(booking => booking.BookingId)
            .ToListAsync(cancellationToken);

        return bookings.Select(booking => new BookingSummary
        {
            BookingId = booking.BookingId,
            PatientName = booking.Patient.FullName,
            BookingDate = booking.BookingDate,

            ServiceNames = booking.BookingServices
                .OrderBy(item => item.DiagnosticService.Name)
                .Select(item => item.DiagnosticService.Name)
                .ToList()
        }).ToList();
    }
}
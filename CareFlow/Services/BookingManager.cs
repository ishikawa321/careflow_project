using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareFlow.Data;
using CareFlow.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Services;

public class BookingManager
{
    private readonly IDbContextFactory<CareFlowDbContext> _contextFactory;

    public BookingManager(
        IDbContextFactory<CareFlowDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<BookingResult> CreateBookingAsync(
        int patientId,
        DateOnly bookingDate,
        IReadOnlyCollection<int> serviceIds,
        CancellationToken cancellationToken = default)
    {
        if (patientId <= 0)
        {
            return Failure("Please select an existing patient.");
        }

        if (bookingDate < DateOnly.FromDateTime(DateTime.Now))
        {
            return Failure("The booking date cannot be in the past.");
        }

        if (serviceIds is null)
        {
            return Failure("Please select one to three services.");
        }

        // Copy the selection so it cannot change during processing.
        int[] selectedIds = serviceIds.ToArray();

        if (selectedIds.Length < 1 || selectedIds.Length > 3)
        {
            return Failure("Please select one to three services.");
        }

        if (selectedIds.Distinct().Count() != selectedIds.Length)
        {
            return Failure(
                "The same service cannot be selected more than once.");
        }

        try
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync(
                    cancellationToken);

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    cancellationToken);

            bool patientExists = await context.Patients
                .AnyAsync(
                    patient => patient.PatientId == patientId,
                    cancellationToken);

            if (!patientExists)
            {
                return Failure("The selected patient does not exist.");
            }

            // Read fresh availability inside the transaction.
            var services = await context.DiagnosticServices
                .AsNoTracking()
                .Where(service => selectedIds.Contains(service.ServiceId))
                .Select(service => new
                {
                    service.ServiceId,
                    service.Name,
                    service.DailyCapacity,

                    BookedCount = service.BookingServices.Count(
                        item => item.Booking.BookingDate == bookingDate)
                })
                .ToListAsync(cancellationToken);

            if (services.Count != selectedIds.Length)
            {
                return Failure(
                    "One or more selected services do not exist.");
            }

            var fullService = services.FirstOrDefault(
                service => service.BookedCount >= service.DailyCapacity);

            if (fullService is not null)
            {
                return Failure(
                    $"{fullService.Name} is fully booked for that date. " +
                    "No booking was saved.");
            }

            var booking = new Booking
            {
                PatientId = patientId,
                BookingDate = bookingDate,
                CreatedAtUtc = DateTime.UtcNow
            };

            foreach (int serviceId in selectedIds)
            {
                booking.BookingServices.Add(new BookingService
                {
                    ServiceId = serviceId
                });
            }

            context.Bookings.Add(booking);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new BookingResult
            {
                Success = true,
                BookingId = booking.BookingId,
                Message = $"Booking #{booking.BookingId} confirmed."
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException exception)
        {
            Debug.WriteLine(exception);

            return Failure(
                "The booking could not be saved. " +
                "Please refresh availability and try again.");
        }
        catch (SqliteException exception)
        {
            Debug.WriteLine(exception);

            return Failure(
                "The database is busy or unavailable. " +
                "Please try again shortly.");
        }
    }

    private static BookingResult Failure(string message)
    {
        return new BookingResult
        {
            Success = false,
            Message = message
        };
    }
}
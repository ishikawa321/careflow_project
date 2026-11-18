using CareFlow.Models;
using CareFlow.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace CareFlow.ViewModels;

public partial class CareFlowViewModel : ViewModelBase
{
    private readonly AvailabilityService _availabilityService;
    private readonly BookingManager _bookingManager;
    private readonly BookingQueryService _queryService;

    public ObservableCollection<Patient> Patients { get; } = new();

    public ObservableCollection<ServiceSelectionViewModel> Services
    { get; } = new();

    public ObservableCollection<BookingSummary> Bookings { get; } = new();

    [ObservableProperty]
    private Patient? selectedPatient;

    [ObservableProperty]
    private DateTimeOffset? selectedDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string message = "Click Refresh to load patient and service data.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmBookingCommand))]
    private bool isBusy;

    public bool IsNotBusy => !IsBusy;

    public CareFlowViewModel(
        AvailabilityService availabilityService,
        BookingManager bookingManager,
        BookingQueryService queryService)
    {
        _availabilityService = availabilityService;
        _bookingManager = bookingManager;
        _queryService = queryService;
    }

    private bool CanRun() => !IsBusy;

    partial void OnSelectedDateChanged(DateTimeOffset? value)
    {
        Services.Clear();
        Message = "Date changed. Click Refresh to load availability.";
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RefreshAsync()
    {
        if (SelectedDate is null)
        {
            Message = "Please select a booking date.";
            return;
        }

        var date = DateOnly.FromDateTime(SelectedDate.Value.DateTime);
        int? previousPatientId = SelectedPatient?.PatientId;

        IsBusy = true;
        Message = "Loading...";
        Services.Clear();

        try
        {
            // SQLite work runs off the UI thread.
            var patients = await Task.Run(
                () => _queryService.GetPatientsAsync());

            Patients.Clear();

            foreach (var patient in patients)
            {
                Patients.Add(patient);
            }

            SelectedPatient = Patients.FirstOrDefault(
                patient => patient.PatientId == previousPatientId);

            await LoadAvailabilityAsync(date);
            await LoadBookingsAsync();

            Message = "Data loaded. Select a patient and one to three services.";
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            Services.Clear();

            Message =
                "Could not load the data. Check the database setup and retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task ConfirmBookingAsync()
    {
        if (SelectedPatient is null)
        {
            Message = "Please select a patient.";
            return;
        }

        if (SelectedDate is null)
        {
            Message = "Please select a booking date.";
            return;
        }

        int patientId = SelectedPatient.PatientId;
        var date = DateOnly.FromDateTime(SelectedDate.Value.DateTime);

        int[] serviceIds = Services
            .Where(service => service.IsSelected)
            .Select(service => service.ServiceId)
            .ToArray();

        IsBusy = true;
        Message = "Checking and saving your booking...";

        try
        {
            var result = await Task.Run(
                () => _bookingManager.CreateBookingAsync(
                    patientId, date, serviceIds));

            Message = result.Message;

            if (result.Success)
            {
                foreach (var service in Services)
                {
                    service.IsSelected = false;
                }
            }

            try
            {
                await LoadAvailabilityAsync(date);
                await LoadBookingsAsync();
            }
            catch (Exception refreshException)
            {
                Debug.WriteLine(refreshException);
                Services.Clear();

                // Keep the booking result if refreshing the screen fails.
                Message = result.Message +
                    " The screen could not refresh. Click Refresh to reload.";
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);

            Message =
                "An unexpected error occurred. Refresh and check the " +
                "booking list before trying again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAvailabilityAsync(DateOnly date)
    {
        var availability = await Task.Run(
            () => _availabilityService.GetAvailabilityAsync(date));

        Services.Clear();

        foreach (var service in availability)
        {
            Services.Add(new ServiceSelectionViewModel(service));
        }
    }

    private async Task LoadBookingsAsync()
    {
        var bookings = await Task.Run(
            () => _queryService.GetBookingsAsync());

        Bookings.Clear();

        foreach (var booking in bookings)
        {
            Bookings.Add(booking);
        }
    }
}
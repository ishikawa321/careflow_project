using CareFlow.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CareFlow.ViewModels;

public partial class ServiceSelectionViewModel : ViewModelBase
{
    public int ServiceId { get; }

    public string Name { get; }

    public int RemainingCapacity { get; }

    public bool IsAvailable => RemainingCapacity > 0;

    public string DisplayText =>
        $"{Name} — {RemainingCapacity} slot(s) remaining";

    [ObservableProperty]
    private bool isSelected;

    public ServiceSelectionViewModel(ServiceAvailability availability)
    {
        ServiceId = availability.ServiceId;
        Name = availability.Name;
        RemainingCapacity = availability.RemainingCapacity;
    }
}
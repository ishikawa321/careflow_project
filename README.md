# CareFlow — Diagnostic Booking Management

ITSD81 Performance Task 4

## Pair members

- Member 1: Steven A. Malnegro
- Member 2: Hervybjorn Tanamal

## Overview

CareFlow is a desktop application for booking diagnostic services for existing patients. Each booking contains one to three distinct services for one date.

The application checks availability again during confirmation. The booking and its selected services are committed together. An unsuccessful operation does not leave a partial booking.

## Technologies

- C# and .NET 10
- Avalonia UI and XAML
- MVVM with CommunityToolkit.Mvvm
- Entity Framework Core 10
- SQLite
- Microsoft dependency injection

## Requirements

- .NET 10 SDK
- Internet access for the initial package and tool restore
- Visual Studio with .NET 10 support, or the .NET command-line tools

## Setup and run

Open PowerShell in the folder containing this README.

Enter the application folder:

```powershell
cd .\CareFlow
```

Restore packages and the project’s local EF Core tool:

```powershell
dotnet restore
dotnet tool restore
```

Create or update the database:

```powershell
dotnet ef database update
```

The existing migrations create the tables and insert fictional patients and sample diagnostic services. Do not create new migrations merely to run the project.

Build and run:

```powershell
dotnet build
dotnet run
```

## Database location

The database is stored at:

```text
%LOCALAPPDATA%\CareFlow\careflow.db
```

The application and migration tools use the same location. On another Windows account or computer, setup creates a separate database.

Do not delete this file when testing persistence.

## Sample data

Patients:

- Alex Demo
- Jamie Sample
- Taylor Test

Diagnostic services and daily capacities:

- Complete Blood Count: 5
- Urinalysis: 5
- Chest X-Ray: 3
- ECG: 1

All patient information is fictional. Applying previously applied migrations does not duplicate the seed records.

## Using the application

1. Select today or a future booking date.
2. Click Refresh.
3. Select an existing patient.
4. Select one to three available services.
5. Click Confirm Booking.
6. Review the feedback and confirmed-booking list.

After changing the date, click Refresh again. Availability applies to the selected date; the confirmed-booking list displays all saved bookings.

## Architecture

- Models represent persisted entities and result data.
- Views define the interface in XAML.
- ViewModels manage selections, commands, loading state, and feedback.
- AvailabilityService calculates remaining capacity from saved bookings.
- BookingManager validates requests and coordinates the transaction.
- BookingQueryService retrieves patients and booking information.
- CareFlowDbContext defines the database mappings.
- App.axaml.cs registers dependencies and connects the main ViewModel.
- CareFlowDbContextFactory supports migration commands.

Database connections and EF Core configuration are not managed by Views or ViewModels.

## Transaction behavior

BookingManager starts a transaction before querying current availability. It verifies the patient and services, checks capacity, adds one booking with all selected services, saves, and commits.

If execution exits before a successful commit, the uncommitted transaction is disposed and rolled back.

Daily capacity remains fixed. Remaining capacity is calculated by subtracting saved service bookings for the selected date.

## Scope

Confirmed bookings are permanent. Cancellation, rescheduling, patient registration, authentication, billing, and reporting are not implemented.

## Troubleshooting

If a build reports locked files, close running CareFlow windows and stop debugging before rebuilding.

If data fails to load, close the application and run:

```powershell
dotnet ef database update
```

Then reopen CareFlow and click Refresh.
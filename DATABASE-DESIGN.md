# CareFlow Database Design

## Tables and keys

| Table | Primary key | Other columns and foreign keys |
|---|---|---|
| Patients | PatientId | FullName |
| DiagnosticServices | ServiceId | Name, DailyCapacity |
| Bookings | BookingId | PatientId → Patients.PatientId, BookingDate, CreatedAtUtc |
| BookingServices | BookingId + ServiceId | BookingId → Bookings.BookingId; ServiceId → DiagnosticServices.ServiceId |

## Relationships

```text
Patients
   1
   |
   many
Bookings
   1
   |
   many
BookingServices
   many
   |
   1
DiagnosticServices
```

- One patient can have many bookings.
- Each booking belongs to one patient.
- Each booking contains service-link records.
- Each service-link record refers to one diagnostic service.
- A diagnostic service can appear in many different bookings.

## Constraints

- Foreign keys require referenced patients, bookings, and services to exist.
- The composite primary key in BookingServices prevents the same service from appearing twice in a saved booking.
- DailyCapacity cannot be negative.
- Required patient and service names cannot be null.
- Restricted deletion prevents deleting referenced records while dependent records remain.
- BookingDate has an index to support date-based queries.

The booking service enforces the one-to-three-service limit, rejects duplicate request entries, rejects past dates, and checks remaining capacity. These rules are not all enforced by table constraints alone.

## Capacity calculation

For one diagnostic service and one selected date:

```text
Remaining capacity =
DailyCapacity − Count of BookingServices linked to Bookings on that date
```

Each selected service consumes one slot. Booking CBC and ECG together consumes one CBC slot and one ECG slot.

There is no separate stored remaining-capacity counter.

## Transaction boundary

The transaction covers:

1. Database validation of the patient and selected services.
2. Reading current service usage for the requested date.
3. Inserting the booking and all service-link records.
4. Saving and committing.

If the transaction fails before commit, no partial booking remains.

## Non-table classes

ServiceAvailability, BookingSummary, and BookingResult are application data objects. They are not mapped database tables.
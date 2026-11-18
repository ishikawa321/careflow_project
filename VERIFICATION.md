# CareFlow Verification Checklist

Testers: Steven A. Malnegro & Hervybjorn Tanamal

Test date(s): November 17, 2026

## Acceptance scenarios

| ID | Procedure performed | Observed result | Status |
|---|---|---|---|
| AC-01 | Refreshed availability for an unused date, then compared it with November 19, 2026 after a booking. | Unused-date capacities were 3, 5, 1, and 5 for Chest X-Ray, CBC, ECG, and Urinalysis. November 19 showed CBC 4 and ECG 0. | Passed |
| AC-02 | Booked CBC and ECG together for Alex Demo on November 19, 2026. | Booking #1 contained both services under one booking number. | Passed |
| AC-03 | Submitted requests without a patient, without services, with four services, and with a past date. | Understandable validation messages appeared. No new bookings or capacity reductions were observed. | Passed |
| AC-04 | Opened two windows on the same unused future date. Window A selected CBC and ECG. Window B booked the last ECG slot. Window A then submitted without refreshing. | Window A rejected the whole request. ECG remained at 0 and CBC remained at 5. Only Window B’s booking was added for that test. | Passed |
| AC-05 | Temporarily triggered an exception after SaveChangesAsync and before CommitAsync using an environment flag. Attempted a CBC and Urinalysis booking for Taylor Test. | No new booking appeared. Both services retained 5 slots. After removing the test setup and restarting, the failed booking remained absent. | Passed |
| AC-06 | Closed and reopened the application after confirming Booking #1, selected November 19, 2026, and refreshed. | Booking #1 and both services remained visible. CBC remained at 4 and ECG at 0. | Passed |

## Rollback test details

The temporary failure was placed between:

```csharp
await context.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);
```

The test caused an exception after database writes but before transaction commit. The uncommitted transaction was disposed and rolled back.

After testing:

- The CAREFLOW_TEST_FAIL_BEFORE_COMMIT environment variable was removed.
- The temporary failure code was removed.
- The application was rebuilt and reopened.
- Existing confirmed bookings remained available.

## Evidence and limitations

Verification used application interactions, refreshed availability, booking retrieval, and restart checks against the persistent SQLite database.

Screenshots document visible results but do not independently prove transaction behavior. The transaction boundary in BookingManager and the controlled failure test provide additional supporting evidence.

The following service-level cases have not yet been separately exercised:

- Duplicate service IDs supplied directly to BookingManager.
- A nonexistent patient ID supplied directly to BookingManager.
- A nonexistent service ID supplied directly to BookingManager.

The implementation contains checks for these cases, but they should not be described as independently tested until they are executed.

## Scope of verification

The stale-availability test used two local application windows. Advanced distributed concurrency and performance under heavy load were not tested.

Any test booking accidentally confirmed during setup remains a valid saved booking. Verification comparisons account for existing records rather than assuming the database contains only one booking.
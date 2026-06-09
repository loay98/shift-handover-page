# Shift Handover Page

An ASP.NET Core MVC demo for supervisor shift management.

## What it does

- Supervisor authentication with ASP.NET Core Identity.
- Shift claiming with claim ownership enforced in the controller.
- Shift logging for accidents, incidents, and manpower updates.
- Shift closing with a final summary and no further edits after close.
- PDF report generation for closed shifts.

## Demo accounts

- `supervisor1@demo.local` / `Demo123!`
- `supervisor2@demo.local` / `Demo123!`

## Run locally

```bash
dotnet run
```

## ERD overview

The application centers on four tables:

- `AspNetUsers` stores authenticated supervisors.
- `Shifts` stores the shift record, claim state, close state, and closure summary.
- `ShiftLogs` stores accident, incident, and manpower log entries linked back to a shift.
- `AspNetRoles` plus the Identity join tables assign supervisors to the `Supervisor` role.

Relationships:

- One user can claim many shifts over time.
- One shift can have many logs.
- Each log belongs to exactly one shift.
- Only the claiming supervisor can add logs or close the shift while it is claimed.

## Notes for the interview

- The report PDF is generated from closed-shift data only.
- The demo uses a local SQLite database file named `shift-handover.db`.
- The home page exposes current shift counts and recent activity for a quick walkthrough.
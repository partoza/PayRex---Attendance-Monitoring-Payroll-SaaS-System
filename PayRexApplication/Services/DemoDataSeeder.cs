using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using PayRexApplication.Models;

namespace PayRexApplication.Services;

/// <summary>
/// Seeds operational demo data (attendance, deductions, benefits, leave requests)
/// for the Demo Company so payroll computation produces meaningful results.
/// Runs once at startup; skips if data already exists.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        const int demoCompanyId = 2;

        // Skip if attendance data already exists for demo company
        if (await db.AttendanceRecords.AnyAsync(a => a.CompanyId == demoCompanyId))
            return;

        var employees = await db.Employees
            .Where(e => e.CompanyId == demoCompanyId && e.Status == EmployeeStatus.Active)
            .ToListAsync();

        if (!employees.Any()) return;

        var rng = new Random(42); // deterministic seed for reproducibility
        var now = DateTime.UtcNow;

        // ── Attendance: last 30 working days for all employees ──
        var attendanceRecords = new List<AttendanceRecord>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (int dayOffset = 30; dayOffset >= 1; dayOffset--)
        {
            var date = today.AddDays(-dayOffset);
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            foreach (var emp in employees)
            {
                // 90% present, 5% late, 5% absent
                var roll = rng.Next(100);
                if (roll >= 95) continue; // absent - no record

                var isLate = roll >= 90;
                var timeIn = isLate
                    ? new TimeOnly(8, rng.Next(20, 45), 0)  // late: 8:20 - 8:44
                    : new TimeOnly(7, rng.Next(40, 59), 0); // on time: 7:40 - 7:59

                var timeOut = new TimeOnly(17, rng.Next(0, 30), 0); // 17:00 - 17:29

                var hoursWorked = (decimal)(timeOut.ToTimeSpan() - timeIn.ToTimeSpan()).TotalHours;
                var overtime = hoursWorked > 9m ? Math.Round(hoursWorked - 9m, 2) : 0m;
                var lateMinutes = isLate ? (int)(timeIn.ToTimeSpan() - new TimeSpan(8, 0, 0)).TotalMinutes : 0;

                attendanceRecords.Add(new AttendanceRecord
                {
                    EmployeeId = emp.EmployeeNumber,
                    CompanyId = demoCompanyId,
                    Date = date,
                    TimeIn = timeIn,
                    TimeOut = timeOut,
                    TotalHoursWorked = Math.Round(hoursWorked, 2),
                    OvertimeHours = overtime,
                    LateMinutes = lateMinutes,
                    Status = isLate ? "Late" : "Present",
                    Source = AttendanceSource.Qr,
                    CreatedAt = now
                });
            }
        }

        db.AttendanceRecords.AddRange(attendanceRecords);

        // ── Employee Deductions ──
        var deductions = new List<EmployeeDeduction>();
        foreach (var emp in employees)
        {
            // Cash advance for some employees
            if (rng.Next(100) < 40)
            {
                deductions.Add(new EmployeeDeduction
                {
                    EmployeeId = emp.EmployeeNumber,
                    Name = "Cash Advance",
                    Amount = rng.Next(5, 20) * 100m, // 500 - 1900
                    Recurring = false,
                    Description = "Cash advance deduction",
                    CreatedAt = now
                });
            }

            // Uniform deduction for staff
            if (rng.Next(100) < 30)
            {
                deductions.Add(new EmployeeDeduction
                {
                    EmployeeId = emp.EmployeeNumber,
                    Name = "Uniform",
                    Amount = 250.00m,
                    Recurring = true,
                    Description = "Monthly uniform deduction",
                    CreatedAt = now
                });
            }
        }
        db.EmployeeDeductions.AddRange(deductions);

        // ── Employee Benefits ──
        var benefits = new List<EmployeeBenefit>();
        foreach (var emp in employees)
        {
            // Meal allowance for all
            benefits.Add(new EmployeeBenefit
            {
                EmployeeId = emp.EmployeeNumber,
                Name = "Meal Allowance",
                Amount = 1500.00m,
                IsTaxable = false,
                Description = "Monthly meal allowance",
                CreatedAt = now
            });

            // Transportation allowance for some
            if (rng.Next(100) < 60)
            {
                benefits.Add(new EmployeeBenefit
                {
                    EmployeeId = emp.EmployeeNumber,
                    Name = "Transportation Allowance",
                    Amount = 1000.00m,
                    IsTaxable = false,
                    Description = "Monthly transportation allowance",
                    CreatedAt = now
                });
            }
        }
        db.EmployeeBenefits.AddRange(benefits);

        // ── Leave Requests (a few approved, a few pending) ──
        var leaveRequests = new List<LeaveRequest>();
        var reviewer = await db.Users.FirstOrDefaultAsync(u => u.CompanyId == demoCompanyId && u.Role == UserRole.Hr);
        var reviewerId = reviewer?.UserId;

        // Pick a few employees for leave requests
        for (int i = 0; i < Math.Min(4, employees.Count); i++)
        {
            var emp = employees[i];
            var startDate = today.AddDays(-rng.Next(5, 20));
            var endDate = startDate.AddDays(rng.Next(1, 3));

            // Calculate business days
            int totalDays = 0;
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }
            if (totalDays == 0) totalDays = 1;

            var isApproved = i < 2; // first 2 approved, rest pending
            leaveRequests.Add(new LeaveRequest
            {
                EmployeeId = emp.EmployeeNumber,
                CompanyId = demoCompanyId,
                LeaveType = i % 2 == 0 ? LeaveType.Vacation : LeaveType.Sick,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                Reason = i % 2 == 0 ? "Family vacation" : "Feeling unwell",
                Status = isApproved ? LeaveStatus.Approved : LeaveStatus.Pending,
                ReviewedBy = isApproved ? reviewerId : null,
                ReviewRemarks = isApproved ? "Approved" : null,
                ReviewedAt = isApproved ? now : null,
                CreatedAt = now
            });
        }
        db.LeaveRequests.AddRange(leaveRequests);

        await db.SaveChangesAsync();
    }
}

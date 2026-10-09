using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClinicDbContext _context;

        public UnitOfWork(ClinicDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesWithAppointmentConflictHandlingAsync(cancellationToken);
        }

        private async Task<int> SaveChangesWithAppointmentConflictHandlingAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                var businessRule = GetAppointmentConflict(exception);
                if (businessRule is null)
                {
                    throw;
                }

                throw businessRule;
            }
        }

        private static BusinessRuleException? GetAppointmentConflict(Exception exception)
        {
            for (Exception? current = exception; current is not null; current = current.InnerException)
            {
                if (current is not SqlException sqlException)
                {
                    continue;
                }

                foreach (SqlError error in sqlException.Errors)
                {
                    if (error.Number is not (2601 or 2627))
                    {
                        continue;
                    }

                    if (error.Message.Contains(
                        "UX_Appointments_Doctor_Scheduled_Date",
                        StringComparison.Ordinal))
                    {
                        return new BusinessRuleException(
                            "Doctor already has a scheduled appointment at this time.");
                    }

                    if (error.Message.Contains(
                        "UX_Appointments_Patient_Scheduled_Date",
                        StringComparison.Ordinal))
                    {
                        return new BusinessRuleException(
                            "Patient already has a scheduled appointment at this time.");
                    }
                }
            }

            return null;
        }
    }
}

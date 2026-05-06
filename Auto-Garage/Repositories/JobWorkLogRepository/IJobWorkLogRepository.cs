using Auto_Garage.Models.DomainModels.JobWorkLogModel;

namespace Auto_Garage.Repositories.JobWorkLogRepository
{
    public interface IJobWorkLogRepository
    {
        Task<List<JobWorkLog>> GetByBookingIdAsync(Guid bookingId);
        Task AddAsync(JobWorkLog item);
        Task DeleteAsync(Guid id);
        Task<JobWorkLog?> GetByIdAsync(Guid id);
        Task SaveChangesAsync();
    }
}
using Auto_Garage.Models.DtoModels.BookingDtos;

namespace Auto_Garage.Services.BookingService
{
    public interface IBookingService
    {
        Task<List<BookingResponseDto>> GetAllAsync(string customerId);
        Task<BookingDetailResponseDto> GetByIdAsync(Guid id, string customerId);
        Task<BookingResponseDto> CreateAsync(string customerId, CreateBookingRequestDto dto);
        Task<BookingResponseDto> UpdateAsync(Guid id, string customerId, UpdateBookingRequestDto dto);
        Task CancelAsync(Guid id, string customerId);
    }
}
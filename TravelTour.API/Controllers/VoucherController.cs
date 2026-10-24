using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ITourRepository _tourRepository;
        private readonly ICustomerRepository _customerRepository;

        public VoucherController(
            IBookingRepository bookingRepository,
            ITourRepository tourRepository,
            ICustomerRepository customerRepository)
        {
            _bookingRepository = bookingRepository;
            _tourRepository = tourRepository;
            _customerRepository = customerRepository;
        }

        // GET: api/Voucher/4
        [HttpGet("{bookingId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> GenerateVoucher(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy booking"
                });
            }

            var tour = await _tourRepository.GetByIdAsync(booking.TourId);

            if (tour == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tour"
                });
            }

            var customer = await _customerRepository.GetByIdAsync(booking.CustomerId);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy khách hàng"
                });
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header()
                        .Text("VOUCHER ĐẶT TOUR")
                        .FontSize(24)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .PaddingTop(20)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text(
                                $"Mã Booking: {booking.BookingId}");

                            column.Item().Text(
                                $"Khách hàng: {customer.FullName}");

                            column.Item().Text(
                                $"Tour: {tour.TourName}");

                            column.Item().Text(
                                $"Địa điểm: {tour.Destination}");

                            column.Item().Text(
                                $"Ngày đặt: {booking.BookingDate:dd/MM/yyyy HH:mm}");

                            if (booking.DepartureDate.HasValue)
                            {
                                column.Item().Text(
                                    $"Ngày khởi hành: {booking.DepartureDate.Value:dd/MM/yyyy}");
                            }

                            column.Item().Text(
                                $"Số người: {booking.NumberOfPeople}");

                            column.Item().Text(
                                $"Tổng tiền: {booking.TotalPrice:N0} VNĐ");

                            column.Item().Text(
                                $"Trạng thái: {booking.Status}")
                                .Bold();

                            column.Item()
                                .PaddingTop(20)
                                .Text("Cảm ơn quý khách đã sử dụng dịch vụ!")
                                .AlignCenter();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("HackMeoww - Travel Tour");
                });
            }).GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Voucher_Booking_{bookingId}.pdf"
            );
        }
    }
}
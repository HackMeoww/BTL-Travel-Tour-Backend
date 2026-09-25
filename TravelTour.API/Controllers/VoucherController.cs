using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelTour.API.Data;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VoucherController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Voucher/4
        [HttpGet("{bookingId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> GenerateVoucher(int bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy booking"
                });
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.TourId == booking.TourId);

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == booking.CustomerId);

            if (tour == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tour"
                });
            }

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
                        .Text("TravelTour");
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
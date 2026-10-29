using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ITourRepository _tourRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPaymentRepository _paymentRepository;

        public BookingController(
            IBookingRepository bookingRepository,
            ITourRepository tourRepository,
            ICustomerRepository customerRepository,
            IPaymentRepository paymentRepository)
        {
            _bookingRepository = bookingRepository;
            _tourRepository = tourRepository;
            _customerRepository = customerRepository;
            _paymentRepository = paymentRepository;
        }

        // GET: api/Booking
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return Ok(bookings);
        }

        // GET: api/Booking/1
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            return Ok(booking);
        }

        // GET: api/Booking/customer/1
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByCustomer(int customerId)
        {
            var bookings = await _bookingRepository.GetByCustomerIdAsync(customerId);
            return Ok(bookings);
        }

        // GET: api/Booking/my
        [HttpGet("my")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetMyBookings()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được tài khoản đăng nhập." });
            }

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {
                return Ok(Array.Empty<Booking>());
            }

            var bookings = await _bookingRepository.GetByCustomerIdAsync(customer.CustomerId);
            return Ok(bookings);
        }

        // POST: api/Booking
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
        {
            var tour = await _tourRepository.GetByIdAsync(booking.TourId);

            if (tour == null)
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            var customer = await _customerRepository.GetByIdAsync(booking.CustomerId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            if (booking.NumberOfPeople <= 0)
            {
                return BadRequest(new { message = "Số người phải lớn hơn 0" });
            }

            booking.BookingDate = DateTime.Now;
            if (!booking.DepartureDate.HasValue)
            {
                booking.DepartureDate = DateTime.Today.AddDays(1);
            }
            booking.TotalPrice = tour.Price * booking.NumberOfPeople;
            booking.Status = "Pending";

            var bookingId = await _bookingRepository.CreateAsync(booking);
            booking.BookingId = bookingId;

            return CreatedAtAction(
                nameof(GetBooking),
                new { id = booking.BookingId },
                booking
            );
        }

        // PUT: api/Booking/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBooking(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return BadRequest(new { message = "BookingId không khớp" });
            }

            if (!await _bookingRepository.ExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            await _bookingRepository.UpdateAsync(booking);

            return NoContent();
        }

        // POST: api/Booking/1/cancel
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            if (booking.Status == "Cancelled")
            {
                return BadRequest(new { message = "Booking đã được hủy trước đó" });
            }

            if (booking.Status == "Refunded")
            {
                return BadRequest(new { message = "Booking đã được hoàn tiền" });
            }

            await _bookingRepository.UpdateStatusAsync(id, "Cancelled");
            booking.Status = "Cancelled";

            return Ok(new
            {
                message = "Hủy booking thành công",
                bookingId = booking.BookingId,
                status = booking.Status
            });
        }

        // POST: api/Booking/1/refund
        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> RefundBooking(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            if (booking.Status != "Cancelled")
            {
                return BadRequest(new
                {
                    message = "Chỉ có booking đã hủy mới được hoàn tiền"
                });
            }

            var payment = await _paymentRepository.GetByBookingIdAsync(booking.BookingId);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thanh toán của booking"
                });
            }

            if (payment.Status == "Refunded")
            {
                return BadRequest(new
                {
                    message = "Thanh toán đã được hoàn tiền trước đó"
                });
            }

            if (payment.Status != "Paid")
            {
                return BadRequest(new
                {
                    message = "Thanh toán chưa ở trạng thái Paid"
                });
            }

            await _paymentRepository.UpdateStatusAsync(payment.PaymentId, "Refunded");
            await _bookingRepository.UpdateStatusAsync(booking.BookingId, "Refunded");

            payment.Status = "Refunded";
            booking.Status = "Refunded";

            return Ok(new
            {
                message = "Hoàn tiền thành công",
                bookingId = booking.BookingId,
                refundAmount = payment.Amount,
                paymentStatus = payment.Status,
                bookingStatus = booking.Status
            });
        }
    }
}
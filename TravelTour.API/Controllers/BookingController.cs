using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Booking
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings.ToListAsync();
        }

        // GET: api/Booking/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            return booking;
        }

        // POST: api/Booking
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
        {
            var tour = await _context.Tours.FindAsync(booking.TourId);

            if (tour == null)
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            var customer = await _context.Customers.FindAsync(booking.CustomerId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            if (booking.NumberOfPeople <= 0)
            {
                return BadRequest(new { message = "Số người phải lớn hơn 0" });
            }

            booking.BookingDate = DateTime.Now;
            booking.TotalPrice = tour.Price * booking.NumberOfPeople;
            booking.Status = "Pending";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

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

            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound(new { message = "Không tìm thấy booking" });
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Booking/1/cancel
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

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

            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();

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
            var booking = await _context.Bookings.FindAsync(id);

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

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == booking.BookingId);

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

            payment.Status = "Refunded";
            booking.Status = "Refunded";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Hoàn tiền thành công",
                bookingId = booking.BookingId,
                refundAmount = payment.Amount,
                paymentStatus = payment.Status,
                bookingStatus = booking.Status
            });
        }

        // DELETE: api/Booking/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
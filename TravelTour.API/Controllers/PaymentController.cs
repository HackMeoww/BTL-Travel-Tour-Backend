using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;

        public PaymentController(IPaymentRepository paymentRepository, IBookingRepository bookingRepository)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
        }

        // GET: api/Payment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return Ok(payments);
        }

        // GET: api/Payment/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound(new { message = "Không tìm thấy thanh toán" });
            }

            return Ok(payment);
        }

        // GET: api/Payment/booking/1
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<Payment>> GetPaymentByBooking(int bookingId)
        {
            var payment = await _paymentRepository.GetByBookingIdAsync(bookingId);

            if (payment == null)
            {
                return NotFound(new { message = "Không tìm thấy thanh toán cho booking này" });
            }

            return Ok(payment);
        }

        // POST: api/Payment
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
        {
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy booking"
                });
            }

            var existingPayment = await _paymentRepository.GetByBookingIdAsync(payment.BookingId);

            if (existingPayment != null && existingPayment.Status == "Paid")
            {
                return BadRequest(new
                {
                    message = "Booking này đã được thanh toán"
                });
            }

            payment.PaymentDate = DateTime.Now;
            payment.Amount = booking.TotalPrice;
            payment.Status = "Paid";

            var invoice = new Invoice
            {
                BookingId = booking.BookingId,
                InvoiceDate = DateTime.Now,
                Amount = booking.TotalPrice,
                PaymentStatus = "Paid"
            };

            var paymentId = await _paymentRepository.CreatePaymentTransactionAsync(payment, invoice, booking.BookingId, "Confirmed");
            payment.PaymentId = paymentId;

            return CreatedAtAction(
                nameof(GetPayment),
                new { id = payment.PaymentId },
                payment
            );
        }

        // PUT: api/Payment/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdatePayment(int id, Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return BadRequest(new { message = "PaymentId không khớp" });
            }

            if (!await _paymentRepository.ExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy thanh toán" });
            }

            await _paymentRepository.UpdateAsync(payment);

            return NoContent();
        }

        // DELETE: api/Payment/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound(new { message = "Không tìm thấy thanh toán" });
            }

            await _paymentRepository.DeleteAsync(id);

            return NoContent();
        }

        // GET: api/Payment/revenue
        [HttpGet("revenue")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetRevenue()
        {
            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync();

            return Ok(new
            {
                message = "Thống kê doanh thu thành công",
                totalRevenue = totalRevenue
            });
        }
    }
}
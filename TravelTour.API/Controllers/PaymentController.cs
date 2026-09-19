using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Payment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return await _context.Payments.ToListAsync();
        }

        // GET: api/Payment/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound(new { message = "Không tìm thấy thanh toán" });
            }

            return payment;
        }

        // POST: api/Payment
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
        {
            var booking = await _context.Bookings.FindAsync(payment.BookingId);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            payment.PaymentDate = DateTime.Now;
            payment.Amount = booking.TotalPrice;
            payment.Status = "Paid";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

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

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
                {
                    return NotFound(new { message = "Không tìm thấy thanh toán" });
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Payment/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound(new { message = "Không tìm thấy thanh toán" });
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}
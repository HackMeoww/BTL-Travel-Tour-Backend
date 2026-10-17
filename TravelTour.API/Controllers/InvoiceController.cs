using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.DTOs;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IBookingRepository _bookingRepository;

        public InvoiceController(IInvoiceRepository invoiceRepository, IBookingRepository bookingRepository)
        {
            _invoiceRepository = invoiceRepository;
            _bookingRepository = bookingRepository;
        }

        // GET: api/Invoice
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return Ok(invoices);
        }

        // GET: api/Invoice/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            return Ok(invoice);
        }

        // GET: api/Invoice/1/detail
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<InvoiceDetailDto>> GetInvoiceDetail(int id)
        {
            var detail = await _invoiceRepository.GetDetailByIdAsync(id);

            if (detail == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin chi tiết hóa đơn" });
            }

            return Ok(detail);
        }

        // POST: api/Invoice
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
        {
            var booking = await _bookingRepository.GetByIdAsync(invoice.BookingId);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            invoice.InvoiceDate = DateTime.Now;
            invoice.Amount = booking.TotalPrice;
            invoice.PaymentStatus = "Pending";

            var invoiceId = await _invoiceRepository.CreateAsync(invoice);
            invoice.InvoiceId = invoiceId;

            return CreatedAtAction(
                nameof(GetInvoice),
                new { id = invoice.InvoiceId },
                invoice
            );
        }

        // PUT: api/Invoice/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateInvoice(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceId)
            {
                return BadRequest(new { message = "InvoiceId không khớp" });
            }

            if (!await _invoiceRepository.ExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            await _invoiceRepository.UpdateAsync(invoice);

            return NoContent();
        }

        // DELETE: api/Invoice/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            await _invoiceRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
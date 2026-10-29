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

        public InvoiceController(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
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
    }
}
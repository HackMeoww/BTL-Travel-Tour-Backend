using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Review
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        // GET: api/Review/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            }

            return review;
        }

        // POST: api/Review
        [HttpPost]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<Review>> CreateReview(Review review)
        {
            var tour = await _context.Tours.FindAsync(review.TourId);

            if (tour == null)
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            var customer = await _context.Customers.FindAsync(review.CustomerId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest(new { message = "Rating phải từ 1 đến 5" });
            }

            review.CreatedAt = DateTime.Now;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetReview),
                new { id = review.ReviewId },
                review
            );
        }

        // PUT: api/Review/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> UpdateReview(int id, Review review)
        {
            if (id != review.ReviewId)
            {
                return BadRequest(new { message = "ReviewId không khớp" });
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest(new { message = "Rating phải từ 1 đến 5" });
            }

            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound(new { message = "Không tìm thấy đánh giá" });
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Review/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
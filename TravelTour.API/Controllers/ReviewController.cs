using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITourRepository _tourRepository;

        public ReviewController(
            IReviewRepository reviewRepository,
            ICustomerRepository customerRepository,
            ITourRepository tourRepository)
        {
            _reviewRepository = reviewRepository;
            _customerRepository = customerRepository;
            _tourRepository = tourRepository;
        }

        // GET: api/Review
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            return Ok(reviews);
        }

        // GET: api/Review/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            }

            return Ok(review);
        }

        // GET: api/Review/tour/1
        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByTour(int tourId)
        {
            var reviews = await _reviewRepository.GetByTourIdAsync(tourId);
            return Ok(reviews);
        }

        // POST: api/Review
        [HttpPost]
        [Authorize(Roles = "Customer,Staff,Admin")]
        public async Task<ActionResult<Review>> CreateReview(Review review)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được tài khoản đăng nhập." });
            }

            var isAdmin = User.IsInRole("Admin");

            var customer = await _customerRepository.GetByIdAsync(review.CustomerId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            if (!isAdmin && customer.UserId != userId)
            {
                return Forbid();
            }

            var tourExists = await _tourRepository.ExistsAsync(review.TourId);

            if (!tourExists)
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest(new { message = "Rating phải từ 1 đến 5" });
            }

            review.CreatedAt = DateTime.Now;

            var reviewId = await _reviewRepository.CreateAsync(review);
            review.ReviewId = reviewId;

            return CreatedAtAction(
                nameof(GetReview),
                new { id = review.ReviewId },
                review
            );
        }

        // PUT: api/Review/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> UpdateReview(int id, Review input)
        {
            if (id != input.ReviewId)
            {
                return BadRequest(new { message = "ReviewId không khớp" });
            }

            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được tài khoản đăng nhập." });
            }

            if (!User.IsInRole("Admin"))
            {
                var customer = await _customerRepository.GetByIdAsync(review.CustomerId);

                if (customer == null || customer.UserId != userId)
                {
                    return Forbid();
                }

                if (input.CustomerId != review.CustomerId)
                {
                    return BadRequest(new { message = "Không được thay đổi khách hàng của đánh giá." });
                }
            }

            if (input.Rating < 1 || input.Rating > 5)
            {
                return BadRequest(new { message = "Rating phải từ 1 đến 5" });
            }

            review.Rating = input.Rating;
            review.Comment = input.Comment ?? string.Empty;

            await _reviewRepository.UpdateAsync(review);

            return NoContent();
        }

        // DELETE: api/Review/1
        [HttpDelete("{id}")]
 
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được tài khoản đăng nhập." });
            }

            if (!User.IsInRole("Admin"))
            {
                var customer = await _customerRepository.GetByIdAsync(review.CustomerId);

                if (customer == null || customer.UserId != userId)
                {
                    return Forbid();
                }
            }

            await _reviewRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
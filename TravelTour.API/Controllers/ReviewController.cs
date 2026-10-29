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

        // GET: api/Review/details (Dành cho Quản lý Đánh giá - Admin & Staff)
        [HttpGet("details")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<ReviewDetailDto>>> GetReviewDetails()
        {
            var details = await _reviewRepository.GetAllDetailsAsync();
            return Ok(details);
        }

        // GET: api/Review/5
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
            var isStaff = User.IsInRole("Staff");

            var customer = await _customerRepository.GetByIdAsync(review.CustomerId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            if (!isAdmin && !isStaff && customer.UserId != userId)
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

            return Ok(review);
        }

        // PUT: api/Review/5 (Admin & Staff có thể sửa / cập nhật nội dung đánh giá)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] Review review)
        {
            if (id != review.ReviewId)
            {
                return BadRequest(new { message = "Mã đánh giá không khớp" });
            }

            var exists = await _reviewRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá cần cập nhật" });
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest(new { message = "Rating phải từ 1 đến 5" });
            }

            await _reviewRepository.UpdateAsync(review);
            return Ok(new { message = "Cập nhật đánh giá thành công", review });
        }

        // DELETE: api/Review/5 (Admin & Staff có quyền xóa đánh giá vi phạm / spam)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var exists = await _reviewRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá cần xóa" });
            }

            await _reviewRepository.DeleteAsync(id);
            return Ok(new { message = "Xóa đánh giá thành công", reviewId = id });
        }
    }
}

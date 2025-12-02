using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.common.interfaces;
using WhiteLagoon.Application.common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        // ============================================================
        // FINALIZE BOOKING (GET)
        // ============================================================
        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber!,
                Email = user.Email!,
                Name = user.Name,
            };

            booking.TotalCost = booking.Villa.Price * nights;

            return View(booking);
        }

        // ============================================================
        // FINALIZE BOOKING (POST) - CREATE CHECKOUT SESSION
        // ============================================================
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);

            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";

            // Stripe Checkout Session Options
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

            // Add line item (dynamic product)
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name
                    }
                },
                Quantity = 1
            });

            // Create session (correct namespace)
            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);

            _unitOfWork.Booking.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            // Redirect user to Stripe Checkout
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        // ============================================================
        // BOOKING CONFIRMATION - CHECK STRIPE PAYMENT STATUS
        // ============================================================
        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(
                u => u.Id == bookingId,
                includeProperties: "User,Villa"
            );

            if (bookingFromDb.Status == SD.StatusPending)
            {
                // Fetch Stripe session
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);
                string? paymentIntentId = session.PaymentIntentId
                              ?? session.PaymentIntent?.Id;

                // CORRECT STRIPE CHECK
                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentID(
                        bookingFromDb.Id, session.Id, session.PaymentIntentId
                    );
                    _unitOfWork.Save();
                }
            }

            return View(bookingId);
        }

        //regions API Call
        [HttpGet]
        [Authorize]
        //public IActionResult GetAll()
        //{
        //    IEnumerable<Booking> objBookings;

        //    if (User.IsInRole(SD.Role_Admin))
        //    {
        //        objBookings = _unitOfWork.Booking.GetAll(includeProperties:"User,Villa");
        //    }
        //    else
        //    {
        //        var claimsIdentity = (ClaimsIdentity)User.Identity;
        //        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        //        objBookings = _unitOfWork.Booking
        //            .GetAll(u => u.UserId == userId,includeProperties:"User,Villa");
        //    }
        //    objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
        //    return Json(new { data = objBookings});
        //}
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;

            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking
                    .GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                objBookings = _unitOfWork.Booking
                    .GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }
            //objBookings = _unitOfWork.Booking
            //       .GetAll(includeProperties: "User,Villa");
            if (!string.IsNullOrEmpty(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }
            return Json(new { data = objBookings });
        }

    }
}

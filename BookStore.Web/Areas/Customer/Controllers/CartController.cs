﻿using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModel;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookStore.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly ILogger<CartController> _logger;
		private readonly IUnitOfWork _unitOfWork;

		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public double OrderTotal { get; set; }

		public CartController(ILogger<CartController> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var calimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = calimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new()
			{
				CartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includePrperties: "Product"),
				OrderHeader = new(),

			};

			foreach (var cart in ShoppingCartVM.CartList)
			{
				cart.Price = PriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartVM);
		}

		public IActionResult Summary()
		{
			var calimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = calimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new()
			{
				CartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includePrperties: "Product"),
				OrderHeader = new(),

			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


			foreach (var cart in ShoppingCartVM.CartList)
			{
				cart.Price = PriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartVM);
		}


		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST(ShoppingCartVM ShoppingCartVM)
		{
			var calimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = calimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.CartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includePrperties: "Product");


			
			ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;




			foreach (var cart in ShoppingCartVM.CartList)
			{
				cart.Price = PriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

			if(applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
			}
			else
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusApproved;
			}

			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.CartList)
			{
				OrderDetail orderDetail = new OrderDetail()
				{

					ProductId = cart.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,


				};

				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}



			//Strip settings start here...

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{

				var domain = "https://localhost:44307/";

				var options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string>
				{
					"card",
				},
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"customer/cart/Index",
				};

				foreach (var item in ShoppingCartVM.CartList)
				{

					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							},

						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);

				}

				var service = new SessionService();
				Session session = service.Create(options);

				var intentId = session.PaymentIntentId;

				_unitOfWork.OrderHeader.UpdateStripId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();


				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
				//Strip settings end here...

			}
			else
			{
				return RedirectToAction("OrderConfirmation", "Cart", new { id= ShoppingCartVM.OrderHeader.Id });
			}

			//_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.CartList);
			//_unitOfWork.Save();

			//return RedirectToAction("Index", "Home");
		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);
			
			if(orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				//Stripe status check here...

				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}
			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u=> u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			HttpContext.Session.Clear();
			_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			_unitOfWork.Save();

			return View(id);
		}



		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
			_unitOfWork.ShoppingCart.IncrementCount(cart, 1);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{

			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
			if (cart.Count > 1)
			{
				_unitOfWork.ShoppingCart.DecrementCount(cart, 1);

                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);
                _unitOfWork.Save();
				return RedirectToAction(nameof(Index));
			}
			else
			{
				TempData["warning"] = " Minimum 1 item needs to be in cart...";
				return RedirectToAction(nameof(Index));
			}

		}

		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);
			_unitOfWork.Save();
			var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count ;
			HttpContext.Session.SetInt32(StaticDetails.SessionCart,count);
			return RedirectToAction(nameof(Index));
		}



		private double PriceBasedOnQuantity(double quantity, double price, double price50, double price100)
		{
			if (quantity <= 50)
			{
				return price;
			}
			else if (quantity <= 100)
			{
				return price50;
			}
			else
			{
				return price100;
			}

		}
	}
}

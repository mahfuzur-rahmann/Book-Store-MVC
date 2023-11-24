using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModel;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            foreach(var cart in ShoppingCartVM.CartList)
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


            ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;




            foreach (var cart in ShoppingCartVM.CartList)
            {
                cart.Price = PriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
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

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.CartList);
            _unitOfWork.Save();

			return RedirectToAction("Index","Home");
		}



		public IActionResult  Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(i=> i.Id ==  cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

		public IActionResult Minus(int cartId)
		{
            
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId);
            if(cart.Count > 1)
            {
				_unitOfWork.ShoppingCart.DecrementCount(cart, 1);
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
			return RedirectToAction(nameof(Index));
		}



		private double PriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if(quantity <= 50) 
            {
                return price;
            }
            else if(quantity <=100)
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

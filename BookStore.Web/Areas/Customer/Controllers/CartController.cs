using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModel;
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
                

            };

            foreach(var cart in ShoppingCartVM.CartList)
            {
                cart.Price = PriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.CartTotalPrice += (cart.Price * cart.Count);
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


            };

            foreach (var cart in ShoppingCartVM.CartList)
            {
                cart.Price = PriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.CartTotalPrice += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
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

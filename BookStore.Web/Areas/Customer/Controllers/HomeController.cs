using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookStore.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includePrperties: "Category");

            return View(productList);
        }


        public IActionResult Details(int productId)
        {

            ShoppingCart cartObj = new ShoppingCart()
            {
                Count= 1, 
                ProductId =  productId,
                Product  = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category")
            };
       
            return View(cartObj);
        }
        
        [HttpPost] 
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //Extract the user identity or userId..
            var calimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = calimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claims.Value;


            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(i=>i.ApplicationUserId == claims.Value &&  i.ProductId == shoppingCart.ProductId );
            if(cartFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);

            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
            }

            _unitOfWork.Save();

            TempData["success"] = "Product added to cart successfully....";


            ShoppingCart cartObj = new ShoppingCart()
            {
                Count= 1,
                
                // Product  = _unitOfWork.Product.Get(u => u.Id == productId, includePrperties: "Category")
            };

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

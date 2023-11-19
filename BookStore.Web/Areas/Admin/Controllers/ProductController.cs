using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            var objProductList = _unitOfWork.Product.GetAll(includePrperties: "Category").ToList();


            return View(objProductList);
        }

        //Upset(Update and insert) operation----------

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = CategoryList
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(x => x.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                //For image start here---
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    string fullFilePath = Path.Combine(productPath, fileName);

                    if (!string.IsNullOrEmpty(productVM.Product.ImgUrl))
                    {
                        var oldPath = Path.Combine(wwwRootPath, productVM.Product.ImgUrl.Trim('\\'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    //productVM.Product.ImgUrl = fullFilePath;
                    productVM.Product.ImgUrl = @"\images\product\" + fileName;
                }
                //For image end here----

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product created successfully....";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });


                return View(productVM);
            }



        }



        //Create operation----------

        //public IActionResult Create()
        //{
        //    //------This commented section for view bag
        //    //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
        //    //{
        //    //    Text = x.Name,
        //    //    Value = x.Id.ToString()
        //    //});
        //    //ViewBag.CategoryList = CategoryList;

        //    IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
        //    {
        //        Text = x.Name,
        //        Value = x.Id.ToString()
        //    });

        //    ProductVM productVM = new()
        //    {
        //        Product = new Product(),
        //        CategoryList = CategoryList
        //    };

        //    return View(productVM);
        //}

        // [HttpPost]
        //public IActionResult Create(ProductVM productVM)
        //{
        //    //var checkNameIsInt = int.TryParse(getObject.Name, out _);
        //    //if (checkNameIsInt)
        //    //{
        //    //    ModelState.AddModelError("name", "Name field cannot be a numeric value...");
        //    //}
        //    if (ModelState.IsValid )
        //    {
        //        _unitOfWork.Product.Add(productVM.Product);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product created successfully....";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    else
        //    {
        //        productVM.CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
        //        {
        //            Text = x.Name,
        //            Value = x.Id.ToString()
        //        });


        //        return View(productVM);
        //    }



        //}


        //Edit operation---
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product productFromDb = _unitOfWork.Product.Get(c => c.Id == id);
        //    return View(productFromDb);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product getObject)
        //{
        //    //var checkNameIsInt = int.TryParse(getObject.Name, out _);
        //    //if (checkNameIsInt)
        //    //{
        //    //    ModelState.AddModelError("name", "Name field cannot be a numeric value...");
        //    //}
        //    if (ModelState.IsValid )
        //    {
        //        _unitOfWork.Product.Update(getObject);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product updated successfully....";

        //        return RedirectToAction("Index", "Product");
        //    }

        //    return View();

        //}



        //Delete operation-----
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product productFromDb = _unitOfWork.Product.Get(c => c.Id == id);
        //    return View(productFromDb);
        //}


        //[HttpPost, ActionName("Delete")]

        //public IActionResult DeletePOST(int? id)
        //{
        //    Product productFromDb = _unitOfWork.Product.Get(c => c.Id == id);
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(productFromDb);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully....";

        //    return RedirectToAction("Index", "Product");
        //}

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var objProductList = _unitOfWork.Product.GetAll(includePrperties: "Category").ToList();
            return Json(new { data = objProductList });

        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var objProduct = _unitOfWork.Product.Get(u => u.Id == id);

            if (objProduct == null)
            {
                return Json(new { succes = false, message = "Error while deleting." });
            }

            var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, objProduct.ImgUrl.Trim('\\'));

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _unitOfWork.Product.Remove(objProduct);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfully." });

        }

        #endregion

    }
}

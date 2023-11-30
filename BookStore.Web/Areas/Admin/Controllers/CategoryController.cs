using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        //Create operation----------

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category getObject)
        {
            var checkNameIsInt = int.TryParse(getObject.Name, out _);
            if (checkNameIsInt)
            {
                ModelState.AddModelError("name", "Name field cannot be a numeric value...");
            }
            if (ModelState.IsValid && checkNameIsInt == false)
            {
                _unitOfWork.Category.Add(getObject);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully....";
                return RedirectToAction("Index", "Category");
            }

            return View();

        }


        //Edit operation---
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category categoryFromDb = _unitOfWork.Category.Get(c => c.Id == id);
            //Category categoryFromDb = _db.Categories.Where(c => c.Id == id).FirstOrDefault();
            //Category categoryIdFromDb = _db.Categories.Find(id);
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category getObject)
        {
            var checkNameIsInt = int.TryParse(getObject.Name, out _);
            if (checkNameIsInt)
            {
                ModelState.AddModelError("name", "Name field cannot be a numeric value...");
            }
            if (ModelState.IsValid && checkNameIsInt == false)
            {
                _unitOfWork.Category.Update(getObject);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully....";

                return RedirectToAction("Index", "Category");
            }

            return View();

        }



        //Delete operation-----
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category categoryFromDb = _unitOfWork.Category.Get(c => c.Id == id);
            return View(categoryFromDb);
        }


        [HttpPost, ActionName("Delete")]

        public IActionResult DeletePOST(int? id)
        {
            Category categoryFromDb = _unitOfWork.Category.Get(c => c.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(categoryFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully....";

            return RedirectToAction("Index", "Category");
        }


        //Alternative for delete a category--
        //[HttpPost]
        //public IActionResult Delete(Category getObjectForDelete)
        //{
        //    _db.Categories.Remove(getObjectForDelete);
        //    _db.SaveChanges();
        //    return RedirectToAction("Index", "Category");
        //}
    }
}

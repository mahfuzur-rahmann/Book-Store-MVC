using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModel;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }


        public IActionResult Index()
        {
            var objProductList = _unitOfWork.Company.GetAll().ToList();


            return View(objProductList);
        }

        //Upset(Update and insert) operation----------

        public IActionResult Upsert(int? id)
        {
            Company company = new();

            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _unitOfWork.Company.Get(x => x.Id == id);
                return View(company);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Company obj)
        {

            if (ModelState.IsValid)
            {
               

                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Company created successfully....";
                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                    TempData["success"] = "Product updated successfully....";
                }

                _unitOfWork.Save();
               
                return RedirectToAction("Index", "Company");
            }
            return View(obj);

        }



        

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });

        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var objCompany = _unitOfWork.Company.Get(u => u.Id == id);

            if (objCompany == null)
            {
                return Json(new { succes = false, message = "Error while deleting." });
            }

    

            _unitOfWork.Company.Remove(objCompany);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfully." });

        }

        #endregion

    }
}

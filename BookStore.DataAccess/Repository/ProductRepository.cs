using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {


        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Product productObject)
        {
            var objFromDb = _db.Products.FirstOrDefault(p => p.Id == productObject.Id);
            if (objFromDb! != null)
            {
                objFromDb.Title = productObject.Title;
                objFromDb.ISBN = productObject.ISBN;
                objFromDb.Author = productObject.Author;
                objFromDb.Description = productObject.Description;
                objFromDb.Price = productObject.Price;
                objFromDb.Price50 = productObject.Price50;
                objFromDb.Price100 = productObject.Price100;
                objFromDb.ListPrice = productObject.ListPrice;
                objFromDb.CategoryId = productObject.CategoryId;
                if (productObject.ImgUrl != null)
                {

                    objFromDb.ImgUrl = productObject.ImgUrl;
                }

            }
            //_db.Products.Update(productObject);

        }
    }
}

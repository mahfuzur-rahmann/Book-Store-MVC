using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Seeds
{
    public class DefaultCategory
    {
        public static List<Category> Categories
        {
            get
            {
                return new List<Category>()
                {
                    new Category {  Id = 1, Name= "Action", DisplayOrder= 1},
                    new Category {  Id = 2, Name= "Comedy", DisplayOrder= 2},
                    new Category {  Id = 3, Name= "Detective", DisplayOrder= 3},
                };
            }
        }
    }
}

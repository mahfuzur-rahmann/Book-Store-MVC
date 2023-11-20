using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        
        IApplicationUserRepository ApplicationUserRepository { get; }
        
        IShoppingCartRepository ShoppingCartRepository { get; }

        void Save();
    }
}

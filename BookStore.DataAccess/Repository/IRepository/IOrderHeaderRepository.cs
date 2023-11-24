using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader headerObject);

        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripId(int id, string sessionId, string paymentIntentId);
    }
}

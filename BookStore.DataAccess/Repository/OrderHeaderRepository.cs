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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {


        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);

        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var objFromDb = _db.OrderHeaders.FirstOrDefault(u=> u.Id == id);

            if(objFromDb != null)
            {
                objFromDb.OrderStatus = orderStatus;
                if(objFromDb.PaymentStatus != null)
                {
                    objFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

		public void UpdateStripId(int id, string sessionId, string paymentIntentSId)
		{
			var objFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

			objFromDb.PaymentIntentId = paymentIntentSId;
			objFromDb.SessionId = sessionId;

		}
	}
}

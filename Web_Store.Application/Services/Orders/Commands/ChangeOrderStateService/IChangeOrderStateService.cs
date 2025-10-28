using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;
using Web_Store.Domain.Entities.Orders;

namespace Web_Store.Application.Services.Orders.Commands.ChangeOrderStateService
{
    public interface IChangeOrderStateService
    {
        ResultDto Execute(RequestChangeOrderStateDto request);
    }

    public class ChangeOrderStateService : IChangeOrderStateService
    {
        private readonly IDataBaseContext _context;

        public ChangeOrderStateService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto Execute(RequestChangeOrderStateDto request)
        {
            var order = _context.Orders.Find(request.OrderId);

            if (order == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "سفارش یافت نشد"
                };
            }

            order.OrderState = request.NewOrderState;
            _context.SaveChanges();

            return new ResultDto
            {
                IsSuccess = true,
                Message = "وضعیت سفارش با موفقیت تغییر کرد"
            };
        }
    }

    public class RequestChangeOrderStateDto
    {
        public long OrderId { get; set; }
        public OrderState NewOrderState { get; set; }
    }
}

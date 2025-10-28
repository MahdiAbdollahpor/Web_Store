using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;
using Web_store.Common;
using Web_Store.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Web_Store.Application.Services.Orders.Queries.IGetOrderInvoiceServiceForAdmin
{
    public interface IGetOrderInvoiceService
    {
        ResultDto<OrderInvoiceDto> Execute(long orderId);
    }

    public class GetOrderInvoiceService : IGetOrderInvoiceService
    {
        private readonly IDataBaseContext _context;

        public GetOrderInvoiceService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<OrderInvoiceDto> Execute(long orderId)
        {
            var order = _context.Orders
                .Include(p => p.User)
                .Include(p => p.OrderDetails)
                .ThenInclude(p => p.Product)
                .FirstOrDefault(p => p.Id == orderId);

            if (order == null)
            {
                return new ResultDto<OrderInvoiceDto>
                {
                    IsSuccess = false,
                    Message = "سفارش یافت نشد"
                };
            }

            var orderItems = order.OrderDetails.Select(od => new OrderItemDto
            {
                ProductName = od.Product.Name,
                Price = od.Price,
                Count = od.Count
            }).ToList();

            var invoice = new OrderInvoiceDto
            {
                OrderId = order.Id,
                OrderDate = order.InsertTime,
                UserFullName = order.User.FullName,
                UserEmail = order.User.Email,
                TotalAmount = orderItems.Sum(oi => oi.TotalPrice),
                OrderItems = orderItems,
                OrderState = EnumHelpers<OrderState>.GetDisplayValue(order.OrderState)
            };

            return new ResultDto<OrderInvoiceDto>
            {
                IsSuccess = true,
                Data = invoice
            };
        }
    }

    public class OrderInvoiceDto
    {
        public long OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public int TotalAmount { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public string OrderState { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public int TotalPrice => Price * Count;
    }
}

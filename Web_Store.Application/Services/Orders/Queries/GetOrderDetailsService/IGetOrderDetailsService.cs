using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;
using Web_Store.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Web_Store.Application.Services.Orders.Queries.GetOrderDetailsService
{
    public interface IGetOrderDetailsService
    {
        ResultDto<OrderDetailsDto> Execute(long orderId, long userId);
    }

    public class GetOrderDetailsService : IGetOrderDetailsService
    {
        private readonly IDataBaseContext _context;

        public GetOrderDetailsService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<OrderDetailsDto> Execute(long orderId, long userId)
        {
            var order = _context.Orders
                .Include(p => p.OrderDetails)
                .ThenInclude(p => p.Product)
                .Include(p => p.RequestPay)
                .Where(p => p.Id == orderId && p.UserId == userId)
                .Select(p => new OrderDetailsDto
                {
                    OrderId = p.Id,
                    OrderState = p.OrderState,
                    RequestPayId = p.RequestPayId,
                    InsertTime = p.InsertTime,
                    OrderDetails = p.OrderDetails.Select(o => new OrderDetailItemDto
                    {
                        Count = o.Count,
                        OrderDetailId = o.Id,
                        Price = o.Price,
                        ProductId = o.ProductId,
                        ProductName = o.Product.Name,
                    }).ToList(),
                })
                .FirstOrDefault();

            if (order == null)
            {
                return new ResultDto<OrderDetailsDto>()
                {
                    IsSuccess = false,
                    Message = "سفارش یافت نشد"
                };
            }

            return new ResultDto<OrderDetailsDto>()
            {
                Data = order,
                IsSuccess = true,
            };
        }
    }

    public class OrderDetailsDto
    {
        public long OrderId { get; set; }
        public OrderState OrderState { get; set; }
        public long RequestPayId { get; set; }
        public DateTime InsertTime { get; set; }
        public List<OrderDetailItemDto> OrderDetails { get; set; }
        public int TotalAmount => OrderDetails?.Sum(od => od.Price * od.Count) ?? 0;
    }

    public class OrderDetailItemDto
    {
        public long OrderDetailId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public int TotalPrice => Price * Count;
    }
}


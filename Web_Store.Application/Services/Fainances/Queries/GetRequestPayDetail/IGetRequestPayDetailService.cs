using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Orders.Queries.GetUserOrders;
using Web_Store.Application.Services.Orders.Queries.IGetOrderInvoiceServiceForAdmin;
using Web_store.Common.Dto;
using Web_store.Common;
using Web_Store.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Web_Store.Application.Services.Fainances.Queries.GetRequestPayDetail
{
    public interface IGetRequestPayDetailService
    {
        ResultDto<RequestPayDetailDto> Execute(long requestPayId);
    }

    public class GetRequestPayDetailService : IGetRequestPayDetailService
    {
        private readonly IDataBaseContext _context;

        public GetRequestPayDetailService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<RequestPayDetailDto> Execute(long requestPayId)
        {
            var requestPay = _context.RequestPays
                .Include(p => p.User)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(p => p.Id == requestPayId);

            if (requestPay == null)
            {
                return new ResultDto<RequestPayDetailDto>
                {
                    IsSuccess = false,
                    Message = "فاکتور یافت نشد"
                };
            }

            var orders = requestPay.Orders.Select(o => new OrderDetailDto
            {
                OrderId = o.Id,
                OrderState = EnumHelpers<OrderState>.GetDisplayValue(o.OrderState),
                OrderDate = o.InsertTime,
                ProductsCount = o.OrderDetails.Count,
                OrderItems = o.OrderDetails.Select(od => new OrderItemDto
                {
                    ProductName = od.Product.Name,
                    Price = od.Price,
                    Count = od.Count
                }).ToList()
            }).ToList();

            var result = new RequestPayDetailDto
            {
                Id = requestPay.Id,
                Guid = requestPay.Guid,
                UserName = requestPay.User.FullName,
                UserEmail = requestPay.User.Email,
                UserId = requestPay.UserId,
                Amount = requestPay.Amount,
                IsPay = requestPay.IsPay,
                PayDate = requestPay.PayDate,
                Authority = requestPay.Authority,
                RefId = requestPay.RefId,
                InsertTime = requestPay.InsertTime,
                Orders = orders
            };

            return new ResultDto<RequestPayDetailDto>
            {
                IsSuccess = true,
                Data = result
            };
        }
    }


    // DTO برای جزئیات کامل فاکتور
    public class RequestPayDetailDto
    {
        public long Id { get; set; }
        public Guid Guid { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public long UserId { get; set; }
        public int Amount { get; set; }
        public bool IsPay { get; set; }
        public DateTime? PayDate { get; set; }
        public string Authority { get; set; }
        public long RefId { get; set; }
        public DateTime InsertTime { get; set; }
        public List<OrderDetailDto> Orders { get; set; }
    }

    public class OrderDetailDto
    {
        public long OrderId { get; set; }
        public string OrderState { get; set; }
        public DateTime OrderDate { get; set; }
        public int ProductsCount { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public int TotalPrice => Price * Count;
    }
}

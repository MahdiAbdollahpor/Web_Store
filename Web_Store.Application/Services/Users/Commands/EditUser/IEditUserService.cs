using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Logs;
using Web_store.Common.Dto;
using Web_Store.Application.Services.Logs.Commands;

namespace Web_Store.Application.Services.Users.Commands.EditUser
{
    public interface IEditUserService
    {
        ResultDto Execute(RequestEdituserDto request);
    }

    public class EditUserService : IEditUserService
    {
        private readonly IDataBaseContext _context;
        private readonly ILogService _logService;

        public EditUserService(IDataBaseContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public ResultDto Execute(RequestEdituserDto request)
        {
            try
            {
                var user = _context.Users.Find(request.UserId);
                if (user == null)
                {
                    _logService.LogWarning("Update", "User", request.UserId, "کاربر یافت نشد");

                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "کاربر یافت نشد"
                    };
                }

                // ذخیره مقادیر قدیمی
                var oldValues = new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    UpdateTime = user.UpdateTime
                };

                user.FullName = request.Fullname;
                user.UpdateTime = DateTime.Now;

                _context.SaveChanges();

                // ذخیره مقادیر جدید
                var newValues = new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    UpdateTime = user.UpdateTime
                };

                _logService.LogInformation(
                    "Update",
                    "User",
                    user.Id,
                    $"کاربر {user.Email} ویرایش شد",
                    JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true }),
                    JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true })
                );

                return new ResultDto()
                {
                    IsSuccess = true,
                    Message = "ویرایش کاربر انجام شد"
                };
            }
            catch (Exception ex)
            {
                _logService.LogError("Update", "User", request.UserId, $"خطا در ویرایش کاربر: {ex.Message}");

                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در ویرایش کاربر: " + ex.Message
                };
            }
        }
    }

    public class RequestEdituserDto
    {
        public long UserId { get; set; }
        public string Fullname { get; set; }
    }
}
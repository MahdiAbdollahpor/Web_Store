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

namespace Web_Store.Application.Services.Users.Commands.UserSatusChange
{
    public interface IUserSatusChangeService
    {
        ResultDto Execute(long UserId);
    }

    public class UserSatusChangeService : IUserSatusChangeService
    {
        private readonly IDataBaseContext _context;
        private readonly ILogService _logService;

        public UserSatusChangeService(IDataBaseContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public ResultDto Execute(long UserId)
        {
            try
            {
                var user = _context.Users.Find(UserId);
                if (user == null)
                {
                    _logService.LogWarning("StatusChange", "User", UserId, "کاربر یافت نشد");

                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "کاربر یافت نشد"
                    };
                }

                // ذخیره وضعیت قدیمی
                var oldStatus = user.IsActive;
                var oldValues = new
                {
                    IsActive = user.IsActive,
                    UpdateTime = user.UpdateTime
                };

                user.IsActive = !user.IsActive;
                user.UpdateTime = DateTime.Now;

                _context.SaveChanges();

                // ذخیره وضعیت جدید
                var newValues = new
                {
                    IsActive = user.IsActive,
                    UpdateTime = user.UpdateTime
                };

                var action = user.IsActive ? "فعال‌سازی" : "غیرفعال‌سازی";

                _logService.LogInformation(
                    "StatusChange",
                    "User",
                    user.Id,
                    $"کاربر {user.Email} {action} شد",
                    JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true }),
                    JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true })
                );

                return new ResultDto()
                {
                    IsSuccess = true,
                    Message = $"کاربر با موفقیت {action} شد"
                };
            }
            catch (Exception ex)
            {
                _logService.LogError("StatusChange", "User", UserId, $"خطا در تغییر وضعیت کاربر: {ex.Message}");

                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در تغییر وضعیت کاربر: " + ex.Message
                };
            }
        }
    }
}
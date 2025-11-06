using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;
using System.Text.Json;
using Web_Store.Application.Services.Logs.Commands;

namespace Web_Store.Application.Services.Users.Commands.RemoveUser
{
    public class RemoveUserService : IRemoveUserService
    {
        private readonly IDataBaseContext _context;
        private readonly ILogService _logService;

        public RemoveUserService(IDataBaseContext context, ILogService logService)
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
                    _logService.LogWarning("SoftDelete", "User", UserId, "کاربر یافت نشد");

                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "کاربر یافت نشد"
                    };
                }

                // ذخیره اطلاعات کاربر قبل از حذف
                var userData = new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = user.UserInRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
                };

                user.RemoveTime = DateTime.Now;
                user.IsRemoved = true;

                _context.SaveChanges();

                _logService.LogWarning(
                    "SoftDelete",
                    "User",
                    user.Id,
                    $"کاربر {user.Email} حذف شد",
                    JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true }),
                    ""
                );

                return new ResultDto()
                {
                    IsSuccess = true,
                    Message = "کاربر با موفقیت حذف شد"
                };
            }
            catch (Exception ex)
            {
                _logService.LogError("SoftDelete", "User", UserId, $"خطا در حذف کاربر: {ex.Message}");

                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در حذف کاربر: " + ex.Message
                };
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Web_store.Common;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Logs;
using Web_Store.Application.Services.Logs.Commands;
using Web_Store.Domain.Entities.Users;

namespace Web_Store.Application.Services.Users.Commands.RgegisterUser
{
    public interface IRegisterUserService
    {
        ResultDto<ResultRegisterUserDto> Execute(RequestRegisterUserDto request);
    }

    public class RegisterUserService : IRegisterUserService
    {
        private readonly IDataBaseContext _context;
        private readonly ILogService _logService;

        public RegisterUserService(IDataBaseContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public ResultDto<ResultRegisterUserDto> Execute(RequestRegisterUserDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return new ResultDto<ResultRegisterUserDto>()
                    {
                        Data = new ResultRegisterUserDto()
                        {
                            UserId = 0,
                        },
                        IsSuccess = false,
                        Message = "پست الکترونیک را وارد نمایید"
                    };
                }

                if (_context.Users.Any(u => u.Email == request.Email))
                {
                    _logService.LogWarning("Create", "User", 0, $"ایمیل {request.Email} قبلاً ثبت شده است");

                    return new ResultDto<ResultRegisterUserDto>
                    {
                        Data = new ResultRegisterUserDto { UserId = 0 },
                        IsSuccess = false,
                        Message = "ایمیل قبلاً ثبت شده است."
                    };
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return new ResultDto<ResultRegisterUserDto>()
                    {
                        Data = new ResultRegisterUserDto()
                        {
                            UserId = 0,
                        },
                        IsSuccess = false,
                        Message = "نام را وارد نمایید"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return new ResultDto<ResultRegisterUserDto>()
                    {
                        Data = new ResultRegisterUserDto()
                        {
                            UserId = 0,
                        },
                        IsSuccess = false,
                        Message = "رمز عبور را وارد نمایید"
                    };
                }

                if (request.Password != request.RePasword)
                {
                    return new ResultDto<ResultRegisterUserDto>()
                    {
                        Data = new ResultRegisterUserDto()
                        {
                            UserId = 0,
                        },
                        IsSuccess = false,
                        Message = "رمز عبور و تکرار آن برابر نیست"
                    };
                }

                string emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                var match = Regex.Match(request.Email, emailRegex, RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    return new ResultDto<ResultRegisterUserDto>()
                    {
                        Data = new ResultRegisterUserDto()
                        {
                            UserId = 0,
                        },
                        IsSuccess = false,
                        Message = "ایمیل خودرا به درستی وارد نمایید"
                    };
                }

                var passwordHasher = new PasswordHasher();
                var hashedPassword = passwordHasher.HashPassword(request.Password);

                User user = new User()
                {
                    Email = request.Email,
                    FullName = request.FullName,
                    Password = hashedPassword,
                    IsActive = true,
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                List<UserInRole> userInRoles = new List<UserInRole>();
                var roleNames = new List<string>();

                foreach (var item in request.roles)
                {
                    var roles = _context.Roles.Find(item.Id);
                    if (roles == null)
                    {
                        _logService.LogError("Create", "User", user.Id, $"نقش با شناسه {item.Id} یافت نشد");

                        return new ResultDto<ResultRegisterUserDto>
                        {
                            Data = new ResultRegisterUserDto { UserId = 0 },
                            IsSuccess = false,
                            Message = $"نقش با شناسه {item.Id} یافت نشد."
                        };
                    }

                    userInRoles.Add(new UserInRole
                    {
                        Role = roles,
                        RoleId = roles.Id,
                        User = user,
                        UserId = user.Id,
                    });

                    roleNames.Add(roles.Name);
                }

                user.UserInRoles = userInRoles;
                _context.SaveChanges();

                // ایجاد لاگ برای ثبت کاربر
                var userData = new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = roleNames,
                    InsertTime = user.InsertTime
                };

                _logService.LogInformation(
                    "Create",
                    "User",
                    user.Id,
                    $"کاربر جدید {request.FullName} ثبت شد",
                    null,
                    JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true })
                );

                return new ResultDto<ResultRegisterUserDto>()
                {
                    Data = new ResultRegisterUserDto()
                    {
                        UserId = user.Id,
                    },
                    IsSuccess = true,
                    Message = "ثبت نام کاربر انجام شد",
                };
            }
            catch (Exception ex)
            {
                _logService.LogError("Create", "User", 0, $"خطا در ثبت کاربر: {ex.Message}");

                return new ResultDto<ResultRegisterUserDto>()
                {
                    Data = new ResultRegisterUserDto()
                    {
                        UserId = 0,
                    },
                    IsSuccess = false,
                    Message = "ثبت نام انجام نشد !"
                };
            }
        }
    }

    public class RequestRegisterUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RePasword { get; set; }
        public List<RolesInRegisterUserDto> roles { get; set; }
    }

    public class RolesInRegisterUserDto
    {
        public long Id { get; set; }
    }

    public class ResultRegisterUserDto
    {
        public long UserId { get; set; }
    }
}
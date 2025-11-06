using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Domain.Entities.HomePages;

namespace Web_Store.Application.Services.Sliders
{
    public interface ISliderService
    {
        ResultDto AddSlider(RequestAddSliderDto request);
        ResultDto EditSlider(RequestEditSliderDto request);
        ResultDto DeleteSlider(long id);
        ResultDto<List<SliderDto>> GetAllSliders();
        ResultDto<SliderDto> GetSliderById(long id);
    }

    public class SliderService : ISliderService
    {
        [Obsolete]
        private readonly IHostingEnvironment _environment;
        private readonly IDataBaseContext _context;

        [Obsolete]
        public SliderService(IHostingEnvironment environment, IDataBaseContext context)
        {
            _environment = environment;
            _context = context;
        }

        [Obsolete]
        public ResultDto AddSlider(RequestAddSliderDto request)
        {
            var resultUpload = UploadFile(request.File!);

            if (resultUpload == null || !resultUpload.Status)
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "خطا در آپلود فایل"
                };
            }

            Slider slider = new Slider()
            {
                link = request.Link!,
                Src = resultUpload.FileNameAddress!,
            };
            _context.Sliders.Add(slider);
            _context.SaveChanges();

            return new ResultDto()
            {
                IsSuccess = true,
                Message = "اسلایدر با موفقیت اضافه شد"
            };
        }

        [Obsolete]
        public ResultDto EditSlider(RequestEditSliderDto request)
        {
            var slider = _context.Sliders.Find(request.Id);
            if (slider == null)
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "اسلایدر یافت نشد"
                };
            }

            if (request.File != null)
            {
                // حذف فایل قبلی
                if (!string.IsNullOrEmpty(slider.Src))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, slider.Src);
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // آپلود فایل جدید
                var resultUpload = UploadFile(request.File);
                if (resultUpload != null && resultUpload.Status)
                {
                    slider.Src = resultUpload.FileNameAddress!;
                }
            }

            slider.link = request.Link!;
            _context.SaveChanges();

            return new ResultDto()
            {
                IsSuccess = true,
                Message = "اسلایدر با موفقیت ویرایش شد"
            };
        }

        [Obsolete]
        public ResultDto DeleteSlider(long id)
        {
            var slider = _context.Sliders.Find(id);
            if (slider == null)
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "اسلایدر یافت نشد"
                };
            }

            // حذف فایل
            if (!string.IsNullOrEmpty(slider.Src))
            {
                var filePath = Path.Combine(_environment.WebRootPath, slider.Src);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _context.Sliders.Remove(slider);
            _context.SaveChanges();

            return new ResultDto()
            {
                IsSuccess = true,
                Message = "اسلایدر با موفقیت حذف شد"
            };
        }

        public ResultDto<List<SliderDto>> GetAllSliders()
        {
            var sliders = _context.Sliders
                .OrderByDescending(x => x.Id)
                .Select(s => new SliderDto
                {
                    Id = s.Id,
                    Src = s.Src,
                    Link = s.link,
                    InsertTime = s.InsertTime
                })
                .ToList();

            return new ResultDto<List<SliderDto>>()
            {
                Data = sliders,
                IsSuccess = true
            };
        }

        public ResultDto<SliderDto> GetSliderById(long id)
        {
            var slider = _context.Sliders
                .Where(s => s.Id == id)
                .Select(s => new SliderDto
                {
                    Id = s.Id,
                    Src = s.Src,
                    Link = s.link,
                    InsertTime = s.InsertTime
                })
                .FirstOrDefault();

            return new ResultDto<SliderDto>()
            {
                Data = slider,
                IsSuccess = slider != null
            };
        }

        [Obsolete]
        private UploadDto? UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string folder = @"images\HomePages\Slider\";
                var uploadsRootFolder = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(uploadsRootFolder))
                {
                    Directory.CreateDirectory(uploadsRootFolder);
                }

                string fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsRootFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                return new UploadDto()
                {
                    FileNameAddress = folder + fileName,
                    Status = true,
                };
            }
            return null;
        }
    }

    public class SliderDto
    {
        public long Id { get; set; }
        public string? Src { get; set; }
        public string? Link { get; set; }
        public DateTime InsertTime { get; set; }
    }

    public class RequestAddSliderDto
    {
        public IFormFile? File { get; set; }
        public string? Link { get; set; }
    }

    public class RequestEditSliderDto
    {
        public long Id { get; set; }
        public IFormFile? File { get; set; }
        public string? Link { get; set; }
    }
}

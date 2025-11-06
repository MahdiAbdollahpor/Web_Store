using Microsoft.AspNetCore.Http;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Domain.Entities.HomePages;
using Microsoft.AspNetCore.Hosting;

namespace Web_Store.Application.Services.HomePages.EditHomePageImages
{
    public interface IEditHomePageImages
    {
        ResultDto Execute(requestEditHomePageImagesDto request);
    }

    public class EditHomePageImages : IEditHomePageImages
    {
        private readonly IDataBaseContext _context;
        [Obsolete]
        private readonly IHostingEnvironment _environment;

        [Obsolete]
        public EditHomePageImages(IDataBaseContext context, IHostingEnvironment hosting)
        {
            _context = context;
            _environment = hosting;
        }

        [Obsolete]
        public ResultDto Execute(requestEditHomePageImagesDto request)
        {
            var image = _context.HomePageImages.Find(request.Id);
            if (image == null)
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "بنر یافت نشد"
                };
            }

            if (request.file != null)
            {
                // حذف فایل قبلی
                if (!string.IsNullOrEmpty(image.Src))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, image.Src);
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // آپلود فایل جدید
                var resultUpload = UploadFile(request.file);
                image.Src = resultUpload!.FileNameAddress;
            }

            image.link = request.Link!;
            image.ImageLocation = request.ImageLocation;

            _context.SaveChanges();
            return new ResultDto()
            {
                IsSuccess = true,
                Message = "بنر با موفقیت ویرایش شد"
            };
        }

        [Obsolete]
        private UploadDto? UploadFile(IFormFile file)
        {
            if (file != null)
            {
                string folder = $@"images\HomePages\Slider\";
                var uploadsRootFolder = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(uploadsRootFolder))
                {
                    Directory.CreateDirectory(uploadsRootFolder);
                }


                if (file == null || file.Length == 0)
                {
                    return new UploadDto()
                    {
                        Status = false,
                        FileNameAddress = "",
                    };
                }

                string fileName = DateTime.Now.Ticks.ToString() + file.FileName;
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




    public class requestEditHomePageImagesDto
    {
        public long Id { get; set; }
        public IFormFile? file { get; set; }
        public string? Link { get; set; }
        public ImageLocation ImageLocation { get; set; }
    }



}




using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Microsoft.AspNetCore.Hosting;

namespace Web_Store.Application.Services.HomePages.DeleteHomePageImages
{
    public interface IDeleteHomePageImages
    {
        ResultDto Execute(long id);
    }

    public class DeleteHomePageImages : IDeleteHomePageImages
    {
        private readonly IDataBaseContext _context;
        [Obsolete]
        private readonly IHostingEnvironment _environment;

        [Obsolete]
        public DeleteHomePageImages(IDataBaseContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Obsolete]
        public ResultDto Execute(long id)
        {
            var image = _context.HomePageImages.Find(id);
            if (image == null)
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "بنر یافت نشد"
                };
            }

            // حذف فایل
            if (!string.IsNullOrEmpty(image.Src))
            {
                var filePath = Path.Combine(_environment.WebRootPath, image.Src);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _context.HomePageImages.Remove(image);
            _context.SaveChanges();

            return new ResultDto()
            {
                IsSuccess = true,
                Message = "بنر با موفقیت حذف شد"
            };
        }


    }
}

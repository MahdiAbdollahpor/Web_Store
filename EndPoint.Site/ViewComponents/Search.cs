﻿using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.Common.Queries.GetCategory;

namespace EndPoint.Site.ViewComponents
{
    public class Search : ViewComponent
    {
        private readonly IGetCategoryService _getCategoryService;
        public Search(IGetCategoryService getCategoryService)
        {
            _getCategoryService = getCategoryService;
        }


        public IViewComponentResult Invoke()
        {
            return View(viewName: "Search", _getCategoryService.Execute().Data);
        }
    }
}

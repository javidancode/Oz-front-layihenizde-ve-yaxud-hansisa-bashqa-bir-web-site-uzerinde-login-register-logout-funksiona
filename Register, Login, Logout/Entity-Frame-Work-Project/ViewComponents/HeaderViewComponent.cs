﻿using Entity_Frame_Work_Project.Data;
using Entity_Frame_Work_Project.Services;
using Entity_Frame_Work_Project.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entity_Frame_Work_Project.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly LayoutService _layoutService;

        public HeaderViewComponent(LayoutService layoutService)
        {
            _layoutService = layoutService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (Request.Cookies["basket"] != null)
            {
                List<BasketVM> basket = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);

                //count = basket.Count();

                //foreach (var item in basket)
                //{
                //    count += item.Count;
                //}

                count = basket.Sum(m => m.Count);
            }
            else
            {
                count = 0;
            }

            Dictionary<string, string> datas = await _layoutService.GetDatasFromSetting();

            HeaderVM headerVM = new HeaderVM
            {
                Count = count,
                settings = datas
            };

            ViewBag.count = count;

            return await Task.FromResult(View(headerVM));
        }
    }
}

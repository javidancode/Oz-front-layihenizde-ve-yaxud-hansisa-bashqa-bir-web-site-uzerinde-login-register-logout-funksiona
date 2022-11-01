using Entity_Frame_Work_Project.Data;
using Entity_Frame_Work_Project.Helpers;
using Entity_Frame_Work_Project.Models;
using Entity_Frame_Work_Project.ViewModels.ProductViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entity_Frame_Work_Project.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1, int take = 3)
        {
            List<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Skip((page * take) - take)
                .Take(take)
                //.OrderByDescending(m => m.Id)
                .ToListAsync();

            List<ProductListVM> mapDatas = GetDatasMap(products);

            int count = await GetPageCount(take);

            Paginate<ProductListVM> result = new Paginate<ProductListVM>(mapDatas, page, count);

            return View(result);
        }

        private async Task<int> GetPageCount(int take)
        {
            int productCount = await _context.Products.Where(m => !m.IsDeleted).CountAsync();

            return (int)Math.Ceiling((decimal)productCount / take);
        }

        private List<ProductListVM> GetDatasMap(List<Product> products)
        {
            List<ProductListVM> productList = new List<ProductListVM>();

            foreach (var producs in products)
            {
                ProductListVM newProduct = new ProductListVM
                {
                    Id = producs.Id,
                    Title = producs.Title,
                    Desc = producs.Desc,
                    Price = producs.Price,
                    MainImage = producs.ProductImages.Where(m=> m.IsMain).FirstOrDefault()?.Image,
                    CategoryName = producs.Category.Name
                };

                productList.Add(newProduct);
            }

            return productList;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = await GetCategoriesAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM product)
        {
            ViewBag.categories = await GetCategoriesAsync();

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            foreach (var photo in product.Photos)
            {
                if (!photo.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Photo", "Please choose correct image type");
                    return View(product);
                }

                if (!photo.CheckFileSize(400))
                {
                    ModelState.AddModelError("Photo", "Please choose correct image size");
                    return View(product);
                }
            }

            List<ProductImage> images = new List<ProductImage>();

            foreach (var photo in product.Photos)
            {
                string fileName = Guid.NewGuid().ToString() + "_" + photo.FileName;

                string path = Helper.GetFilePath(_env.WebRootPath, "img", fileName);

                await Helper.SaveFile(path, photo);

                ProductImage image = new ProductImage
                {
                    Image = fileName,
                };

                images.Add(image);
            }

            images.FirstOrDefault().IsMain = true;

            decimal convertedPrice = decimal.Parse(product.Price.Replace(".", ","));

            Product newProduct = new Product
            {
                Title = product.Title,
                Desc = product.Desc,
                Price = convertedPrice,
                CreateDate = DateTime.Now,
                CategoryId = product.CategoryId,
                ProductImages = images
            };

            await _context.ProductImages.AddRangeAsync(images);
            await _context.Products.AddAsync(newProduct);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Product product = await _context.Products
                .Where(m => m.Id == id)
                .Include(m => m.ProductImages)
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();

            foreach (var item in product.ProductImages)
            {
                string path = Helper.GetFilePath(_env.WebRootPath, "img", item.Image);
                Helper.SliderImgDelete(path);
                item.IsDeleted = true;
            }

            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return NotFound();

            Product dbProduct = await _context.Products
                .Include(m => m.Category)
                .Include(m => m.ProductImages)
                .FirstOrDefaultAsync();

            ProductUpdateVM updateProduct = new ProductUpdateVM
            {
                Id = dbProduct.Id,
                Title = dbProduct.Title,
                Price = dbProduct.Price,
                Desc = dbProduct.Desc,
                CategoryId = dbProduct.CategoryId,
                Images = dbProduct.ProductImages,
            };

            return View(updateProduct);
        }

        private async Task<SelectList> GetCategoriesAsync()
        {
            IEnumerable<Category> categories = await _context.Categories.ToListAsync();

            return new SelectList(categories, "Id", "Name");
        }


    }
}

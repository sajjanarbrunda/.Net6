using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment   hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;

        }

        public IActionResult Index()
        {

            //IEnumerable<Product> objProductList = _unitOfWork.Product.GetAll();
            //return View(objProductList);
            return View();


        }
        //GET
        

        public IActionResult Upsert(int? id)
        {
            //     viewbag and ViewData
            //      Product product = new();

            //            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(
            //             u =>new SelectListItem { 
            //               Text = u.Name,
            //             Value = u.Id.ToString()


            // } );
            //            IEnumerable<SelectListItem> CoverTypeList = _unitOfWork.CoverType.GetAll().Select(
            //u => new SelectListItem
            //{
            // Text = u.Name,
            // Value = u.Id.ToString()
            //});
            ProductVM productVM = new()
            {

                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {

                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {

                    Text = i.Name,
                    Value = i.Id.ToString()
                }),

            };




            if (id == null || id == 0)
            {
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                ////create product
                return View(productVM);
            }
            else {

                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);

                //Update product
            }
           
        
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj , IFormFile file)
        {
            
            if (ModelState.IsValid)
            {

                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null) {

                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\Product");
                    var extensions = Path.GetExtension(file.FileName);



                    if (obj.Product.ImageUrl != null) {


                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    using (var filestreams = new FileStream(Path.Combine(uploads, fileName + extensions), FileMode.Create)) {

                        file.CopyTo(filestreams);
                    }
                    obj.Product.ImageUrl = @"\Images\Product\" + fileName + extensions;



                }
                if (obj.Product.Id == 0)
                {


                    _unitOfWork.Product.Add(obj.Product);
                }
                else {
                    _unitOfWork.Product.Update(obj.Product);

                }
             
                _unitOfWork.Save();
                TempData["sucess"] = "CoverType edited succesfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        
      
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }
        //post
        [HttpDelete]
        
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);


            if (obj == null)
            {
                return Json(new
                {
                    sucess = false,
                    message = "Error while deleting"
                });

            }
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath)) { 
            
            System.IO.File.Delete(oldImagePath);
            }
          
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            return Json(new
            {
                sucess = true,
                message = "Delete Succesfully"
            });

         
        }

        #endregion


    }




}


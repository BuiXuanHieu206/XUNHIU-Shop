﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020375.BusinessLayers;
using SV20T1020375.DomainModels;
using SV20T1020375.Web.AppCodes;
using SV20T1020375.Web.Models;

namespace SV20T1020375.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}, {WebUserRoles.Employee}")]
    public class SupplierController : Controller
    {
        const int PAGE_SIZE = 20;
        const string CREATE_TITLE = "Bổ sung nhà cung cấp";
        const string UPDATE_TITLE = "Cập nhật thông tin nhà cung cấp";
        const string SUPPlIER_SEARCH = "supplier_search"; //Tên biến session dùng để lưu lại điều kiện tìm kiếm
        public IActionResult Index()
        {
            //Kiểm tra xem trong session có lưu điều kiện tìm kiếm không
            //Nếu có thì sử dụng lại điều kiện tìm kiếm, ngược lại thì tìm kiếm theo điều kiện mặc định
            Models.PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPlIER_SEARCH);
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;
            var data = CommonDataService.ListOfSuppliers(out rowCount, input.Page, input.PageSize, input.SearchValue ?? "");
            var model = new SupplierSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            //Lưu lại điều kiện tìm kiếm
            ApplicationContext.SetSessionData(SUPPlIER_SEARCH, input);

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = CREATE_TITLE;
            var model = new Supplier()
            {
                SupplierID = 0,
                BirthDate = new DateTime(1990, 1, 1)
            };
            return View("Edit", model);
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = UPDATE_TITLE;
            var model = CommonDataService.GetSupplier(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        [HttpPost] //Attribute => chỉ nhận dữ liệu gửi lên dưới dạng POST
        public IActionResult Save(Supplier model, string birthDateInput = "")
        {
            DateTime? d = birthDateInput.ToDateTime();
            if (d.HasValue)
                model.BirthDate = d.Value;
            if (string.IsNullOrWhiteSpace(model.SupplierName))
                ModelState.AddModelError("SupplierName", "Tên không được để trống"); //tên lỗi + thông báo lỗi
            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError("ContactName", "Tên giao dịch không được để trống");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email không được để trống");
            if (string.IsNullOrWhiteSpace(model.Province))
                ModelState.AddModelError("Province", "Vui lòng chọn tỉnh/thành");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.SupplierID == 0 ? CREATE_TITLE : UPDATE_TITLE;
                return View("Edit", model);
            }
            if (model.SupplierID == 0)
            {
                int id = CommonDataService.AddSupplier(model);
                if (id <= 0)
                {
                    ModelState.AddModelError("Email", "Email bị trùng");
                    ViewBag.Title = CREATE_TITLE;
                    return View("Edit", model);
                }
            }
            else
            {
                bool result = CommonDataService.UpdateSupplier(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được nhà cung cấp. Có thể email bị trùng");
                    ViewBag.Title = UPDATE_TITLE;
                    return View("Edit", model);
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                bool result = CommonDataService.DeleteSupplier(id);
                return RedirectToAction("Index");
            }

            var model = CommonDataService.GetSupplier(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
    }
}

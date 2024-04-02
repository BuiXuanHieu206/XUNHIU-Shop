using Microsoft.AspNetCore.Mvc;
using SV20T1020375.Web.Models;
using System.Globalization;

namespace SV20T1020375.Web.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung test";
            var model = new Models.Person()
            {
                Name = "Bùi Xuân Hiếu",
                Birthday = DateTime.Now,
                Salary = 500.25m
            };
            return View(model);
        }
        public IActionResult Save(Models.Person model, string birthdayInput ="")
        {
            //Chuyển chuỗi birthdayInput thành giá trị ngày, nếu hợp lệ mới dùng giá trị do người dùng nhập
            DateTime? d = null;
            try
            {
                d = DateTime.ParseExact(birthdayInput, "d/M/yyyy", CultureInfo.InvariantCulture);
            }
            catch { }
            if (d.HasValue)
                model.Birthday = d.Value;
            return Json(model);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020375.BusinessLayers;
using SV20T1020375.DomainModels;
using SV20T1020375.Web.AppCodes;
using SV20T1020375.Web.Models;

namespace SV20T1020375.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}, {WebUserRoles.Employee}")]
    public class ProductController : Controller
    {
        const int PAGE_SIZE = 20;
        const string CREATE_TITLE = "Bổ sung mặt hàng";
        const string UPDATE_TITLE = "Cập nhật thông tin mặt hàng";
        const string PRODUCT_SEARCH = "product_search"; //Tên biến session dùng để lưu lại điều kiện tìm kiếm
        public IActionResult Index()
        {
            //Kiểm tra xem trong session có lưu điều kiện tìm kiếm không
            //Nếu có thì sử dụng lại điều kiện tìm kiếm, ngược lại thì tìm kiếm theo điều kiện mặc định
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);

            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                };
            }

            return View(input);
        }

        public IActionResult Search(ProductSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ListProducts(out rowCount, input.Page, input.PageSize, input.SearchValue ?? "", input.CategoryID,
                                            input.SupplierID, input.MinPrice, input.MaxPrice);

            var model = new ProductSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            // Lưu lại vào session điều kiện tìm kiếm
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(model);
        }
        public IActionResult Create()
        {
            ViewBag.Title = CREATE_TITLE;
            ViewBag.IsEdit = false;
            var model = new Product()
            {
                ProductID = 0,
                Photo = "nophoto.jpg",
            };

            return View("Edit", model);
        }
        public IActionResult Edit(int id = 0) 
        {
            ViewBag.Title = UPDATE_TITLE;
            ViewBag.IsEdit = true;

            var model = ProductDataService.GetProduct(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult Save(Product model, IFormFile? uploadPhoto = null)
        {
            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError(nameof(model.ProductName), "Tên mặt hàng không được để trống");
            if (string.IsNullOrWhiteSpace(model.ProductDescription))
                ModelState.AddModelError(nameof(model.ProductDescription), "Mô tả không được để trống");
            if (model.CategoryID.ToString() == "0")
                ModelState.AddModelError(nameof(model.CategoryID), "Vui lòng chọn Loại hàng");
            if (model.SupplierID.ToString() == "0")
                ModelState.AddModelError(nameof(model.SupplierID), "Vui lòng chọn Nhà cung cấp");
            if (string.IsNullOrWhiteSpace(model.Unit))
                ModelState.AddModelError(nameof(model.Unit), "Đơn vị tính không được để trống");
            if (!decimal.TryParse(model.Price.ToString(), out decimal parsedPrice) || parsedPrice <= 0)
                ModelState.AddModelError("Price", "Giá hàng không hợp lệ.");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                ViewBag.IsEdit = model.ProductID == 0 ? false : true;
                return View("Edit", model);
            }

            // Xử lý ảnh upload: Nếu có ảnh được upload thì lưu ảnh lên server, gán tên file ảnh đã lưu cho model.Photo
            if (uploadPhoto != null)
            {
                // Tên file sẽ lưu trên server 
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                // Đường dẫn vật lý đến file sẽ lưu trên server (vd: D:\MyWeb\wwwroot\images\employees\photo.png)
                string filePath = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, @"images\products", fileName);

                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                // Gán tên file ảnh cho model.Photo
                model.Photo = fileName;
            }

            if (model.ProductID == 0)
            {
                int id = ProductDataService.AddProduct(model);
                if (id <= 0)
                {
                    ModelState.AddModelError("ProductName", "Tên mặt hàng bị trùng");
                    ViewBag.Title = "Bổ sung mặt hàng";
                    return View("Edit", model);
                }
            }
            else
            {
                bool result = ProductDataService.UpdateProduct(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được mặt hàng. Có thể tên mặt hàng bị trùng");
                    ViewBag.Title = "Cập nhật thông tin mặt hàng";
                    return View("Edit", model);
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                bool result = ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }

            var model = ProductDataService.GetProduct(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }
        public IActionResult Photo(int id = 0, string method = "add", int photoId = 0)
        {
            if (id < 0)
            {
                return RedirectToAction("Index");
            }

            ProductPhoto data = null;
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";

                    data = new ProductPhoto()
                    {
                        PhotoID = 0,
                        ProductID = id,
                    };

                    return View(data);
                case "edit":
                    ViewBag.Title = "Cập nhật ảnh của mặt hàng";

                    if (photoId < 0)
                    {
                        return RedirectToAction("Index");
                    }

                    data = ProductDataService.GetPhoto(photoId);
                    if (data == null)
                    {
                        return RedirectToAction("Index");
                    }

                    return View(data);
                case "delete":
                    //TODO: Xóa ảnh có mã photoId (xóa trực tiếp, không cần xác nhận)
                    ProductDataService.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }
        public IActionResult AddPhoto(ProductPhoto model, IFormFile? uploadPhoto)
        {
            if (uploadPhoto == null)
                ModelState.AddModelError(nameof(model.Photo), "Vui lòng chọn ảnh");
            if (string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError(nameof(model.Description), "Mô tả không được để trống");
            if (!int.TryParse(model.DisplayOrder.ToString(), out int parsedDisplayOrder) || parsedDisplayOrder <= 0)
                ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị ảnh mặt hàng phải > 0");

            // Kiểm tra thứ tự hiển thị
            List<ProductPhoto> lstPhotos = ProductDataService.ListPhotos(model.ProductID);
            bool isUsed = false;

            foreach (ProductPhoto item in lstPhotos)
            {
                if (item.DisplayOrder == model.DisplayOrder && model.PhotoID != item.PhotoID)
                {
                    isUsed = true;
                    break;
                }
            }

            if (isUsed)
            {
                ModelState.AddModelError("DisplayOrder",
                        $"Thứ tự hiển thị {model.DisplayOrder} của ảnh đã được sử dụng");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.PhotoID == 0 ? "Bổ sung ảnh cho mặt hàng" : "Cập nhật ảnh của mặt hàng";
                return View("Photo", model);
            }

            // Xử lý ảnh upload: Nếu có ảnh được upload thì lưu ảnh lên server, gán tên file ảnh đã lưu cho model.Photo
            if (uploadPhoto != null)
            {
                // Tên file sẽ lưu trên server 
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                // Đường dẫn vật lý đến file sẽ lưu trên server (vd: D:\MyWeb\wwwroot\images\employees\photo.png)
                string filePath = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, @"images\products", fileName);

                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                // Gán tên file ảnh cho model.Photo
                model.Photo = fileName;
            }

            // Thêm hoặc Cập nhật
            if (model.PhotoID == 0)
            {
                long ptId = ProductDataService.AddPhoto(model);
                if (ptId <= 0)
                {
                    ModelState.AddModelError("Error", "Không thêm được ảnh cho mặt hàng");
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    return View("Photo", model);
                }
            }
            else
            {
                bool result = ProductDataService.UpdatePhoto(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được ảnh cho mặt hàng");
                    ViewBag.Title = "Cập nhật ảnh của mặt hàng";
                    return View("Photo", model);
                }
            }

            return RedirectToAction("Edit", new { id = model.ProductID });
        }
        public IActionResult Attribute(int id = 0, string method = "add", int attributeId = 0)
        {
            if (id < 0)
            {
                return RedirectToAction("Index");
            }

            ProductAttribute data = null;
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";

                    data = new ProductAttribute()
                    {
                        AttributeID = 0,
                        ProductID = id,
                    };

                    return View(data);
                case "edit":
                    ViewBag.Title = "Cập nhật thuộc tính của mặt hàng";
                    if (attributeId < 0)
                    {
                        return RedirectToAction("Index");
                    }

                    data = ProductDataService.GetAttribute(attributeId);
                    if (data == null)
                    {
                        return RedirectToAction("Index");
                    }

                    return View(data);
                case "delete":
                    ProductDataService.DelAttribute(attributeId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }
        public IActionResult AddAttribute(ProductAttribute model)
        {
            if (string.IsNullOrWhiteSpace(model.AttributeName))
                ModelState.AddModelError(nameof(model.AttributeName), "Tên thuộc tính không được rỗng");
            if (string.IsNullOrWhiteSpace(model.AttributeValue))
                ModelState.AddModelError(nameof(model.AttributeValue), "Giá trị thuộc tính không được rỗng");
            if (!int.TryParse(model.DisplayOrder.ToString(), out int parsedDisplayOrder) || parsedDisplayOrder <= 0)
                ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị thuộc tính phải > 0");

            // Kiểm tra thứ tự hiển thị
            List<ProductAttribute> lstAttributes = ProductDataService.ListAttributes(model.ProductID);
            bool isUsed = false;

            foreach (ProductAttribute item in lstAttributes)
            {
                if (item.DisplayOrder == model.DisplayOrder && model.AttributeID != item.AttributeID)
                {
                    isUsed = true;
                    break;
                }
            }

            if (isUsed)
            {
                ModelState.AddModelError("DisplayOrder",
                        $"Thứ tự hiển thị {model.DisplayOrder} của thuộc tính đã được sử dụng");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.AttributeID == 0 ? "Bổ sung thuộc tính cho mặt hàng" : "Cập nhật thuộc tính của mặt hàng";
                return View("Attribute", model);
            }

            // Thêm hoặc Cập nhật
            if (model.AttributeID == 0)
            {
                long atId = ProductDataService.AddAttribute(model);
                if (atId <= 0)
                {
                    ModelState.AddModelError(nameof(model.AttributeName), "Tên thuộc tính bị trùng");
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    return View("Attribute", model);
                }
            }
            else
            {
                bool result = ProductDataService.UpdateAttribute(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được thuộc tính. Có thể tên thuộc tính bị trùng");
                    ViewBag.Title = "Cập nhật thuộc tính của mặt hàng";
                    return View("Attribute", model);
                }
            }

            return RedirectToAction("Edit", new { id = model.ProductID });
        }
    }
}

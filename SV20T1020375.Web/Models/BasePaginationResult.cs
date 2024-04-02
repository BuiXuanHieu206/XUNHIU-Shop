using SV20T1020375.DomainModels;

namespace SV20T1020375.Web.Models
{
    /// <summary>
    /// Lớp cơ sở cho các lớp biểu diễn dữ liệu là kết quả của thao tác
    /// tìm kiếm, phân trang
    /// </summary>
    public abstract class BasePaginationResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
        public int RowCount { get; set; }
        public int PageCount 
        { 
            get
            {
                if (PageSize == 0)
                    return 1;
                int c = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    c += 1;
                return c;
            }
        }
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách khách hàng
    /// </summary>
    public class CustomerSearchResult : BasePaginationResult
    {
        public List<Customer> Data { get; set; }
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách loại hàng
    /// </summary>
    public class CategorySearchResult : BasePaginationResult
    {
        public List<Category> Data { get; set; }
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách người giao hàng
    /// </summary>
    public class ShipperSearchResult : BasePaginationResult
    {
        public List<Shipper> Data { get; set; }
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách nhà cung cấp
    /// </summary>
    public class SupplierSearchResult : BasePaginationResult
    {
        public List<Supplier> Data { get; set; }
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách nhân viên
    /// </summary>
    public class EmployeeSearchResult : BasePaginationResult
    {
        public List<Employee> Data { get; set; }
    }
    public class ProductSearchResult : BasePaginationResult 
    { 
        public List<Product> Data { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
    }
}

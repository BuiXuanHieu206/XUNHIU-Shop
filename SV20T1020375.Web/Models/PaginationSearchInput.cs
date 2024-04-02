namespace SV20T1020375.Web.Models
{
    /// <summary>
    /// Đầu vào tím kiếm dữ liệu để nhận dữ liệu dưới dạng phân trang
    /// </summary>
    public class PaginationSearchInput
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public string SearchValue { get; set; } = "";
    }
    /// <summary>
    /// Đầu vào tìm kiếm dùng cho mặt hàng
    /// </summary>
    public class ProductSearchInput : PaginationSearchInput
    {
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}

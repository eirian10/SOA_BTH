# SOA_BTH
# Bài Thực Hành Kiến Trúc Hướng Dịch Vụ (SOA)

Repository chứa mã nguồn của 2 bài thực hành được tổ chức thành 2 nhánh riêng biệt:

## Cấu trúc các nhánh (Branches)

* **`thuc-hanh-4`**: Xây dựng hệ thống quản lý đơn hàng gồm 3 dịch vụ độc lập:
  * `AuthService` (`http://localhost:5000`): Xác thực người dùng và cấp phát JWT Token.
  * `ProductService` (`http://localhost:5001`): Quản lý thông tin và tồn kho sản phẩm
  * `OrderService` (`http://localhost:5002`): Quản lý đơn hàng và tích hợp giao diện kiểm thử

* **`thuc-hanh-5`**: Phát triển mở rộng từ Bài thực hành 4, bổ sung thêm dịch vụ thứ 4:
  * `ReportService` (`http://localhost:5003`): Thống kê số lượng bán, doanh thu, chi phí và lợi nhuận theo đơn hàng và sản phẩm với cơ sở dữ liệu độc lập (`ReportServiceDB`)

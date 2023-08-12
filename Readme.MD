## Controller
   - Là một lớp kế thừ từ lớp Controller: Microsoft.AspNetCore.Mvc.Controller
   - Trong Controller có các properties giống trong Page Model:
      this.HttpContext
      this.Request
      this.Response
      this.User
      this.ModelState
      this.ViewData
      this.ViewBag
      this.Url
      this.TempData
   - Có thể Inject các dịch vụ qua phương thức khởi tạo:
      VD: logger, dbcontext, ...

## Action
   -  Là phương thức được khai báo public, nó có thể trả về bất kì kiểu dữ liệu gì,
      nội dung trả về của nó được Convert thành chuỗi và trả về nội dung cho Client
   -  Đối với IActionResult có rất nhiều kiểu trả về:
      Kiểu trả về                 | Phương thức          | Mô tả
      -------------------------------------------------------------------------------------------------------
      ContentResult               | Content()            | Thiết lập nội dung trả về trực tiếp của Response VD: return Content(noidung, "contentType(text/plaint, application/json, ...)");
      EmptyResult                 | new EmptyResult()    | Tương đương phương thức có kiểu trả về là void
      FileResult                  | File()               | Trả về nội dung của 1 file: ảnh, text, ... . Đọc nội dung file thành 1 bảng byte rồi trả về VD: return File(bytesarray, "contentType(image/png,jpg)")
      ForbidResult                | Forbid()             |
      JsonResult                  | Json()               | Trả về Json, tham số là 1 đối tượng VD: return Json(obj);
      LocalRedirectResult         | LocalRedirect()      | Truy cập vào trang chỉ ra trong tham số, tham số là 1 local redirect VD: /Home/Privacy
      RedirectResult              | Redirect()           | Truy cập vào trang chỉ ra trong tham số, tham số là 1 địa chỉ url VD: https://google.com.vn
      RedirectToActionResult      | RedirectToAction()   |
      RedirectToPageResult        | RedirectToRoute()    |
      RedirectToRouteResult       | RedirectToPage()     |
      PartialViewResult           | PartialView()        |
      ViewComponentResult         | ViewComponent()      |
      StatusCodeResult            | StatusCode()         |
      ViewResult                  | View()               |
   - ViewResult: Trả về chế độ xem từ một Action trong Controller, thường là 1 trang html
      - Các trường hợp trả về:
         View();           : Tìm file razor tương ứng với /View/Controller/Action.cshtml
         View(obj);        : Giống TH trên và truyền dữ liệu sang View. Chú ý với truyền dữ liệu sang là string thì phải ép sang object VD: return View(object(string...));
         View(path);       : Tìm file với đường dẫn là path, path là đường dẫn tuyệt đối VD: Views/ABC/Xyz.cshtml
         View(path, obj)   : Giống TH trên và có truyền dữ liệu sang View

## View
   - Là file cshtml

## Area:
   - Thiết lập cho Controller thuộc 1 vùng nào đó
   - Là tên dùng để Routing
   - Là cấu trúc thư mục chưa MVC
   - Thiết lập Area cho Controller bằng [Area("AreaName")]
   - Tạo cấu trúc thư mục:
      dotnet aspnet-codegenerator area AreaName

## logger:
   - Có 6 cấp độ của logger:
      - 1: Debug - Viết ra trong quá trình phát triển ứng dụng
      - 2: Information - Thông tin thông thường trong quá trình hoạt động ứng dụng
      - 3: Warning - Cảnh báo
      - 4: Error - Lỗi
      - 5: Critical - Có thể crash chương trình
   - Sử dụng logger theo cấp độ:
      - Cú pháp: _logger.Log(LogLevel.Warning, "Nội dung");
         hoặc    _logger.LogWaring/Error/Information("Nội dung");

## Truyền dữ liệu từ Controller sang View
   - Model
   - TempData
   - Ngoài cách truyền dữ liệu từ Controller sang View qua Model, có thể sử dụng ViewData và ViewBag.
   - Cú pháp:
      ViewData["key"] = value - giống kiểu dữ liệu Dictionary
      ViewBag.key = value;

## Truyền dữ liệu từ trang này sang trang khác
   - Sử dụng TempData, nó sử dụng session của hệ thống để lưu dữ liệu và trang khác có thể lưu dữ liệu này
   - Lưu ý, khi ở trang khác mà đọc dữ liệu từ TempData thì nó lập tức bị xóa luôn
   - Cú pháp: TempData["Key"] = value


## Đường dẫn của ứng dụng
   - Có thể lưu đường dẫn vào 1 biến static để sử dụng khi cần:
      - Khai báo 1 biến static trong class Startup
      - Inject dịch vụ IWebHostEnvironment, gán biến đó bằng thuộc tính ContentRootPath của dịch vụ
         public static string RootPath {set;get;}

         public Startup(IConfiguration configuration, IWebHostEnvironment env)
         {
               Configuration = configuration;
               RootPath = env.ContentRootPath;
         }
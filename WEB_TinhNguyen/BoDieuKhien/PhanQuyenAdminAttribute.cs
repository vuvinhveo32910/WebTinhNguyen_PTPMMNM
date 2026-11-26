using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.BoDieuKhien
{
    /// <summary>
    /// Thuộc tính kiểm soát truy cập và phân quyền chi tiết theo từng phân hệ quản trị (/admin).
    /// Kế thừa từ <see cref="AuthorizeAttribute"/> để tương thích 100% với pipeline xác thực chuẩn ASP.NET Core,
    /// đồng thời triển khai <see cref="IAuthorizationFilter"/> và <see cref="IAsyncActionFilter"/>
    /// để kiểm tra quyền hạn phân hệ trên từng request GET/POST.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PhanQuyenAdminAttribute : Attribute, IAuthorizationFilter, IAsyncActionFilter
    {
        /// <summary>
        /// Mã phân hệ chức năng yêu cầu để truy cập Controller hoặc Action này (ví dụ: "TrangChu", "TinTuc", "QuanLyTaiKhoan").
        /// </summary>
        public string MaPhanHe { get; }

        public PhanQuyenAdminAttribute(string maPhanHe = "")
        {
            MaPhanHe = QuyenAdmin.ChuanHoaMaPhanHe(maPhanHe);
        }

        /// <summary>
        /// Thực thi kiểm tra phân quyền trong pha Authorization Filter của ASP.NET Core MVC.
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null) return;

            // Bỏ qua nếu Action có gắn [AllowAnonymous]
            foreach (var filter in context.ActionDescriptor.EndpointMetadata)
            {
                if (filter is IAllowAnonymous || filter is AllowAnonymousAttribute)
                {
                    return;
                }
            }

            var user = context.HttpContext?.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                // Chưa đăng nhập -> chuyển hướng về trang đăng nhập /admin/login
                var returnUrl = context.HttpContext?.Request?.Path.Value ?? "/Admin";
                context.Result = new RedirectResult($"/admin/login?ReturnUrl={WebUtility.UrlEncode(returnUrl)}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(MaPhanHe) && !user.CoQuyen(MaPhanHe))
            {
                // Đã đăng nhập nhưng tài khoản không được cấp quyền cho phân hệ này -> Chặn 403 Forbidden
                if (context.HttpContext != null)
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }

        /// <summary>
        /// Thực thi kiểm tra phân quyền trong pha Action Filter (bảo vệ kép cho cả GET và POST CRUD).
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null)
            {
                if (next != null) await next();
                return;
            }

            foreach (var filter in context.ActionDescriptor.EndpointMetadata)
            {
                if (filter is IAllowAnonymous || filter is AllowAnonymousAttribute)
                {
                    await next();
                    return;
                }
            }

            var user = context.HttpContext?.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                var returnUrl = context.HttpContext?.Request?.Path.Value ?? "/Admin";
                context.Result = new RedirectResult($"/admin/login?ReturnUrl={WebUtility.UrlEncode(returnUrl)}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(MaPhanHe) && !user.CoQuyen(MaPhanHe))
            {
                if (context.HttpContext != null)
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            await next();
        }
    }
}


using FresherMisa2026.Entities;
using System.Net;
using System.Text.Json;
using MySqlConnector;
namespace FresherMisa2026.WebAPI.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                Console.WriteLine("Before run middleware");
                // Pass the request to the next middleware/component
                await _next(context);
                Console.WriteLine("After run middleware");
            }
            catch (Exception ex)
            {
                // Handle the exception globally
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Set status code and content type
            context.Response.ContentType = "application/json";
            var mysqlEx = exception as MySqlException
                          ?? exception.InnerException as MySqlException;
             if (mysqlEx != null)
            {
                // Trường hợp 1: Unique Index bị vi phạm (từ DB level - Error 1062)
                if (mysqlEx.Number == 1062)
                {
                    string userMessage;

                    if (mysqlEx.Message.Contains("UQ_EmployeeCode"))
                        userMessage = "Mã nhân viên đã tồn tại";
                    else
                        userMessage = "Dữ liệu đã tồn tại";  // fallback nếu unique index khác bị vi phạm

                    context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    var response = new ServiceResponse
                    {
                        IsSuccess = false,
                        Code = (int)HttpStatusCode.Conflict,
                        UserMessage = userMessage,
                        DevMessage = "Duplicate entry: " + mysqlEx.Message
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }

                // Trường hợp 2: Stored Procedure SIGNAL error (SQLSTATE '45000')
                if (mysqlEx.Message.Contains("EmployeeCode đã tồn tại"))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    var response = new ServiceResponse
                    {
                        IsSuccess = false,
                        Code = (int)HttpStatusCode.Conflict,
                        UserMessage = "Mã nhân viên đã tồn tại",
                        DevMessage = mysqlEx.Message
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }
            }

            // Create response payload
            var errorResponse = new ServiceResponse
            {
                IsSuccess = false,
                Code = context.Response.StatusCode,
                UserMessage = "Có lỗi xảy ra vui lòng liên hệ Misa!",
                DevMessage = exception.Message // Optional: include for dev
            };

            // Serialize the response to JSON
            var jsonResponse = JsonSerializer.Serialize(errorResponse);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.IO;
using System.Threading.RateLimiting;
// using Serilog.Enrichers.Environment;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Middleware;
using Secure_Academic_Management.Health;

// ============================================================================
// 1. CẤU HÌNH SERILOG (Logging) - Checklist M03: Mục 15, 20, 23
// ============================================================================
// Cấu hình Serilog NGAY LẬP TỨC để có thể bắt được cả các lỗi khởi động ứng dụng (startup errors)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    //.Enrich.WithMachineName()
    //.Enrich.WithThreadId()
    // QUAN TRỌNG: Không bao giờ ghi mật khẩu, connection string hay token vào log
    .Filter.ByExcluding(logEvent =>
        logEvent.Properties.ContainsKey("Password") ||
        logEvent.Properties.ContainsKey("ConnectionString"))
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("Starting up the M03 Student Management Application");

    var builder = WebApplication.CreateBuilder(args);

    // Thay thế ILogger mặc định của .NET bằng Serilog
    builder.Host.UseSerilog();

    // ============================================================================
    // 2. CẤU HÌNH DỊCH VỤ (Services)
    // ============================================================================

    // A. Kết nối SQL Server - Checklist M03: Mục 13 (Bảo mật Connection String)
    // Lưu ý: Chỉ lấy TÊN chuỗi kết nối, giá trị thực tế sẽ được lấy từ User Secrets hoặc Environment Variables
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(); // Tự động thử lại nếu kết nối tạm thời bị lỗi
        }));

    var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
    Directory.CreateDirectory(dataProtectionPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("SecureAcademicManagement");

    // B. Cấu hình ASP.NET Core Identity - Checklist M03: Mục 9, 17
    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        // Bắt buộc xác nhận email (tùy chọn nghiệp vụ)
        options.SignIn.RequireConfirmedAccount = false;

        // Bảo mật: Khóa tài khoản sau 5 lần nhập sai mật khẩu (Chống brute-force)
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Bảo mật: Yêu cầu độ phức tạp mật khẩu
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>() // Hỗ trợ phân quyền theo Role (Admin, Staff, Student)
    .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("sql-server");
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("login", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    // Cấu hình Cookie bảo mật (Checklist M03: Mục 12 - CSRF)
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Chỉ cho phép qua HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict;

        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

    var app = builder.Build();

    // ============================================================================
    // 3. CẤU HÌNH HTTP REQUEST PIPELINE
    // ============================================================================

    // Bảo mật: Xử lý lỗi an toàn - Checklist M03: Mục 15, 23
    if (!app.Environment.IsDevelopment())
    {
        // Môi trường Production: Ẩn chi tiết lỗi kỹ thuật, chuyển hướng đến trang lỗi thân thiện
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts(); // Bắt buộc sử dụng HTTPS
    }
    else
    {
        // Môi trường Development: Có thể dùng trang lỗi chi tiết để debug (nhưng vẫn nên hạn chế)
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles(); // Phục vụ file CSS, JS, hình ảnh

    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        await next();
    });

    app.UseRouting();
    app.UseRateLimiter();

    // Thứ tự BẮT BUỘC: Authentication -> Authorization -> Endpoints
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<AuditAndMetricsMiddleware>();

    app.MapHealthChecks("/health");
    app.MapGet("/metrics", () =>
        Results.Text(AuditAndMetricsMiddleware.RenderMetrics(), "text/plain"));

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    Log.Information("Application started successfully");

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbInitializer.Initialize(services);
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // Đảm bảo flush toàn bộ log ra file trước khi ứng dụng tắt
    Log.CloseAndFlush();
}
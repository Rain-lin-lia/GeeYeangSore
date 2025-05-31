using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Data;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Hubs;
using GeeYeangSore.Settings; 

var builder = WebApplication.CreateBuilder(args);

var backendName = Environment.GetEnvironmentVariable("BACKEND_NAME");
var port = Environment.GetEnvironmentVariable("CUSTOM_PORT") ?? "7022";
var vueOrigin = Environment.GetEnvironmentVariable("VUE_ORIGIN") ?? "http://localhost:5173";

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 添加 GeeYeangSoreContext 服務
builder.Services.AddDbContext<GeeYeangSoreContext>(options =>
    options.UseSqlServer(connectionString));

// 添加 IHttpContextAccessor 服務
builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "GeeYeangSore", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) =>
        apiDesc.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor &&
        descriptor.ControllerTypeInfo.Namespace != null &&
        descriptor.ControllerTypeInfo.Namespace.StartsWith("GeeYeangSore.APIControllers"));
});
builder.Services.AddRazorPages();
// 新增 CORS 政策
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueDevServer", policy =>
    {
        policy.WithOrigins(vueOrigin, "http://localhost:5178", "http://localhost:5176", "http://localhost:5175", "http://localhost:5174")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
    });
});
// 添加 Session 服務
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
// 添加 SignalR
builder.Services.AddSignalR();

//添加SMTP
builder.Services.Configure<SmtpSettings>(
builder.Configuration.GetSection("SmtpSettings"));

var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// 啟用靜態檔案服務
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
// 啟用 CORS 中介軟體
app.UseCors("AllowVueDevServer");
// 啟用 Session 中間件
app.UseSession();

app.UseAuthorization();

// 修改路由配置，添加一個新的默認路由指向 Admin 區域的 Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

// 原有的 areas 路由
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 原有的普通路由（當 URL 沒有指定 area 時使用）
app.MapControllerRoute(
    name: "nonarea",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
// 加上 SignalR Hub 路由
app.MapHub<ChatHub>("/hub");
app.MapControllers();
app.MapRazorPages();

app.Run();
using System.Globalization;
using MapApi;
using MapApi.Services;
using NewLife;
using NewLife.Cube;
using NewLife.Log;
using XCode;

// 日志输出到控制台，并拦截全局异常
XTrace.UseConsole();

var builder = WebApplication.CreateBuilder(args);

// 运行时语言：固定界面与异常语言，或单独固定日志语言；留空时跟随操作系统
var culture = builder.Configuration["Localization:Culture"];
if (!culture.IsNullOrEmpty()) Localization.Culture = CultureInfo.GetCultureInfo(culture);
var logCulture = builder.Configuration["Localization:LogCulture"];
if (!logCulture.IsNullOrEmpty()) Localization.LogCulture = CultureInfo.GetCultureInfo(logCulture);

// 请求语言随 Accept-Language 变化时，日志默认保持宿主语言，避免日志语言抖动
if (Localization.Culture == null) Localization.LogCulture ??= CultureInfo.CurrentUICulture;

var services = builder.Services;

// 配置星尘。借助StarAgent，或者读取配置文件 config/star.config 中的服务器地址、应用标识、密钥
var star = services.AddStardust(null);

// 初始配置
var set = NewLife.Setting.Current;
if (set.IsNew)
{
    set.DataPath = "../Data";
    set.Save();
}

_ = EntityFactory.InitAllAsync();

services.AddSingleton<MapService>();

// Add services to the container.
builder.Services.AddRazorPages();

// 引入魔方
services.AddCube();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 按 Accept-Language 切换本次请求的界面语言；配置 Localization:Culture 后固定语言，不再随请求变化
app.Use(async (context, next) =>
{
    using var scope = RequestCultureScope.Create(context.Request.Headers.AcceptLanguage);
    await next();
});

app.UseAuthorization();

app.UseCube(app.Environment);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CubeHome}/{action=Index}/{id?}");

// 启用星尘注册中心，向注册中心注册服务，服务消费者将自动更新服务端地址列表
app.RegisterService("NewLife.Map", null, app.Environment.EnvironmentName);

app.Run();

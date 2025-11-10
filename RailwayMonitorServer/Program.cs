using System.Text;
using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Services;
using RailwayAlarmBackend.Handlers;
using RailwayAlarmBackend.Hubs;
using RailwayAlarmBackend.Interfaces;
using RailwayAlarmBackend.Models.Configs; // 引入全局异常处理命名空间
using Serilog;
using Microsoft.Extensions.FileProviders;



var builder = WebApplication.CreateBuilder(args);

// 配置Serilog日志
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 启用对 DefaultValue 特性的支持
    options.SupportNonNullableReferenceTypes();

    // 启用 XML 注释（可选，但推荐）
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // options.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers();

// 配置Entity Framework - 之前的问题：缺少DbContext注册，导致服务无法获取数据库连接
// 修复前：DataContext没有在DI容器中注册，DeviceTypeService无法获取有效的DbContext实例
var offline = builder.Configuration.GetValue<bool>("Offline", false);

if (offline)
{
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection")!, //现场是 DefaultConnection
            new MySqlServerVersion(new Version(5, 7, 29)), // 修正为实际的MySQL版本
            mysqlOptions => mysqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(3),
                    errorNumbersToAdd: null
                )
        ));
}
else
{
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DevConnection")!, //现场是 DefaultConnection
            new MySqlServerVersion(new Version(8, 0, 35)), // 修正为实际的MySQL版本
            mysqlOptions => mysqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(3),
                    errorNumbersToAdd: null
                )
        ));
}




builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IAlarmTraceService, AlarmTraceService>();

// 注册SignalR服务
builder.Services.AddSignalR();

// 配置CORS - 允许所有跨域请求
builder.Services.AddCors(options =>
{
    // 策略1：允许任何来源（不支持凭证）
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()     // 允许任何来源
            .AllowAnyMethod()     // 允许任何HTTP方法
            .AllowAnyHeader();    // 允许任何请求头
        // 注意：AllowAnyOrigin() 和 AllowCredentials() 不能同时使用
    });

    // 策略2：允许特定来源（支持凭证）
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173",
                         "http://localhost:8081", "https://localhost:8081") // 明确指定前端地址
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();  // 允许凭证
    });
});

var config = builder.Configuration.GetSection("SuperBrain");
var alarmImageFolder = config.GetSection("ImageFolder").Value  ?? "image";
if (!Directory.Exists(alarmImageFolder))
    Directory.CreateDirectory(alarmImageFolder);


builder.Services.Configure<SuperBrainConfig>(config);
builder.Services.AddHostedService<SuperBrainHostService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.AddProblemDetails();
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();

// 配置静态文件服务 - 允许前端访问告警追踪图片
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), alarmImageFolder)),
    RequestPath = $"/{alarmImageFolder}"
});

// 启用CORS中间件 - 必须在路由之后，控制器映射之前
app.UseCors("AllowSpecificOrigins");


app.UseExceptionHandler();

// 映射控制器路由 - 之前的问题：缺少路由映射，导致API端点无法访问
// 修复前：虽然控制器存在，但没有调用app.MapControllers()，API端点无法注册
app.MapControllers();

// 映射SignalR Hub路由
app.MapHub<AlarmTraceHub>("/alarmHub");

// 仅在非IIS环境下显式指定端口
if (app.Environment.IsDevelopment())
{
    app.Run("http://localhost:8081");
}
else
{
    app.Run();
}
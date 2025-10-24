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
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));



// 注册自定义服务 - 之前的问题：缺少服务类注册，导致控制器注入失败
// 修复前：DeviceTypeService等服务没有在DI容器中注册，控制器构造函数注入时得到null
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

// 配置SuperBrain
builder.Services.Configure<SuperBrainConfig>(config);
// 注册托管服务 
// builder.Services.AddHostedService<SuperBrainHostService>();
// ===============================================
// 全局异常处理配置
// ===============================================
// 1. 注册全局异常处理程序
//    - 将GlobalExceptionHandler注册到DI容器中
//    - 当应用程序发生未处理异常时，会自动调用此处理程序
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 2. 配置ProblemDetails服务
//    - 提供标准化的错误响应格式
//    - 与异常处理程序配合使用，确保错误响应格式统一
builder.Services.AddProblemDetails();
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

// ===============================================
// 启用全局异常处理中间件
// ===============================================
// 3. 使用异常处理中间件
//    - 在HTTP请求管道中启用异常处理
//    - 捕获所有未处理的异常并转发给注册的异常处理程序
//    - 位置很重要：应该在路由中间件之后，控制器映射之前
app.UseExceptionHandler();

// 映射控制器路由 - 之前的问题：缺少路由映射，导致API端点无法访问
// 修复前：虽然控制器存在，但没有调用app.MapControllers()，API端点无法注册
app.MapControllers();

// 映射SignalR Hub路由
app.MapHub<AlarmTraceHub>("/alarmHub");
app.Run();
using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Services;
using RailwayAlarmBackend.Handlers; // 引入全局异常处理命名空间

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<DeviceService>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

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

app.Run();
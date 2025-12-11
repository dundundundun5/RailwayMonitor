# 部署
1. 创建了依赖注入配置
   (DependencyInjection.cs)

-
使用Microsoft.Extensions.Hosting创建Host
- 配置了appsettings.json加载
- 注册了DbContext（使用MySQL连接）
- 注册了服务：IDeviceService和IAlarmTrace
  Service
- 注册了所有窗口类

2. 修改了App.xaml.cs

- 在构造函数中创建Host
- 在OnStartup中启动Host并获取MainWindow
- 在OnExit中正确关闭Host

3. 修改了窗口类

- MainWindow.xaml.cs: 通过构造函数接收IDe
  viceService和IConfiguration
- DeviceManagement.xaml.cs:
  通过构造函数接收IDeviceService
- DeviceEditWindow.xaml.cs:
  通过构造函数接收IDeviceService

4. 更新了窗口创建代码

- 修改了DeviceManagement中创建DeviceEditW
  indow的代码，传递_deviceService

关键特点：

1. 与后端风格一致：使用Microsoft.Extensio
   ns.DependencyInjection，与你的ASP.NET
   Core后端保持一致
2. 配置集中管理：所有配置在DependencyInje
   ction.cs中统一管理
3. 自动解析：窗口的依赖项会自动通过构造函
   数注入
4. 生命周期管理：使用Host管理应用生命周期

使用方法：

现在你可以在任何窗口的构造函数中声明需要
的服务，例如：
public MyWindow(IDeviceService
deviceService, IAlarmTraceService
alarmTraceService)
{
// 服务会自动注入
}

注意事项：

1. 确保已安装必要的NuGet包：
   -
   Microsoft.Extensions.DependencyInjection
   - Microsoft.Extensions.Hosting
2. 如果还有其他服务需要注册，只需在Depend
   encyInjection.cs的ConfigureServices方法中
   添加
3. 窗口的注册方式：
   - AddSingleton: 单例，整个应用只有一个
   实例（如MainWindow）
   - AddTransient:
   每次请求都创建新实例（如对话框窗口）

现在你的WPF项目就有了完整的依赖注入支持，
与后端ASP.NET
Core项目保持一致的架构风格。

是的，需要修改App.xaml！我看到第6行有Star
tupUri="MainWindow.xaml"，这表示WPF会直接
启动MainWindow。但是我们现在使用依赖注入
，需要在App.xaml.cs中手动创建和显示MainWi
ndow。


## 文件配置
1. 后端 Offline -> true， 表示本地
2. 后端 数据库连接字符串测试一下

## 环境配置
1. MYSQL 5.7.29 
   1. port=`3306`
   2. database=`ocean`
   2. username=`root` ;password=`Aotie3199@`
   3. username=`aotie`; password=`Aotie3199@`
   4. 在`aotie`用户下运行 `device.sql` 和 `alarm_trace.sql`，那是后端要用的两个表
1. ~~（可选）本地开发环境，本地包和破解版Rider~~ 
   2. 本地开发环境参见 `nuget.config` ，将`packages.zip` 解压到 `用户名/.nuget`
2. .net 8 runtime 
3. __mysql安装包__
4. 发布到本地文件夹
5. 运行 `dotnet-hosting-8.0.22-win.exe` 这是IIS支持 + net8 控制台运行时
6. 运行 `windowsdesktop-runtime-8.0.22-win-x64.exe` 这是 net8 桌面运行时

## 开启IIS
1. 开始菜单-搜索-启用或关闭Windows功能
2. 勾选 Internet Information Service
3. 打开Internet Information Services (IIS)管理器

## 配置并开启IIS服务

1. 新建应用程序池<随便写个英文名>， 默认代码托管
   1. 高级设置-启用32位应用程序-false
2. 添加网站-端口8081-物理文件夹选择.net webapi目录
3. 记得修改配置文件，删除数据库

## 配置取流
- `RailwayMonitorServer`
    - 删除藏在某处的laoda.jpg，孩子们，想我了吗？
    - 离线运行 `"Offline": false`
    - 数据库连接字符串 `"ConnectionStrings": {
      "DefaultConnection": "server=192.168.100.33;database=ocean;user=aotie;password=Aotie3199@;Connection Lifetime=31536000;Min Pool Size=5;Max Pool Size=20;Pooling=true;",
      "DevConnection": "server=192.168.100.33;database=ocean;user=aotie;password=Aotie3199@;Connection Lifetime=31536000;Min Pool Size=5;Max Pool Size=20;Pooling=true;"
    }` 一定要把DevConnection内容删掉，那是我自己的阿里云服务器！那是我自己的阿里云服务器！那是我自己的阿里云服务器！
    - 超脑配置 `"SuperBrain": {
      "Ip": "192.168.100.10",
      "Port": "8000",
      "Username": "admin",
      "Password": "11111111a",
      "ImageFolder": "alarmTraceImage",
      "AlarmInterval": 60,
      "RecognizeHat": false
    }`

- `RailwayMonitorClient`
  - 最多显示监控画面数量 `"VideoNumber": 11`
  - 后端无效时默认本地实时监控取流 `"VideoIpAddressList": [
          "192.168.100.10:8000:2",
          "192.168.100.10:8000:5",
          "192.168.100.10:8000:3",
          "192.168.100.10:8000:8",
          "192.168.100.10:8000:7",
          "192.168.100.10:8000:4",
          "192.168.100.10:8000:1",
          "192.168.100.10:8000:6",
          "192.168.100.20:9601:4",
          "192.168.100.20:9601:2",
          "192.168.100.20:9601:1"
      ]`
  - 超脑通道对应点位`"SuperBrainChannelName": [
      7, 
      1, 
      3, 
      6, 
      2, 
      8, 
      5, 
      4
      ]`
  - 录像机取流地址  `"RecorderIpAddress" : "192.168.100.20:9601"`


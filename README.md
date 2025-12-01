# 部署

## 文件配置
1. 后端 Offline -> true， 表示本地
2. 后端 数据库连接字符串测试一下

## 环境配置
1. MYSQL 5.7.29 aotie Aotie3199@ 3306， root密码保持一致
1. （可选）本地开发环境，本地包和破解版Rider
2. .net 8 runtime 
3. （可选）mysql安装包
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

## 数据库

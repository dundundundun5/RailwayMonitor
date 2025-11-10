# 部署

## 环境配置
1. （可选）本地开发环境，本地包和破解版Rider
2. .net 8 runtime 
3. （可选）mysql安装包
4. 后端和前端发布到本地文件夹

## 开启IIS
1. 开始菜单-搜索-启用或关闭Windows功能
2. 勾选 Internet Information Service
3. 打开Internet Information Services (IIS)管理器

## 配置并开启IIS服务
1. 新建应用程序池<随便写个英文名>， 默认代码托管
   1. 高级设置-启用32位应用程序-false
2. 添加网站-端口8081-物理文件夹选择.net webapi目录

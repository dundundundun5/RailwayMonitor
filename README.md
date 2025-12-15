# 部署


## 文件配置
1. 后端 Offline -> true， 表示本地
2. 后端 数据库连接字符串测试一下

## 环境配置
1. MYSQL 5.7.29 
   1. port=`33206`
   2. database=`ocean`
   2. username=`root` ;password=`Aotie3199@`
   3. username=`aotie`; password=`Aotie3199@`
   4. 在`aotie`用户下运行 `device.sql` 和 `alarm_trace.sql`，那是后端要用的两个表
1. ~~（可选）本地开发环境，本地包和破解版Rider~~ 
   2. 本地开发环境参见 `nuget.config` ，将`packages.zip` 解压到 `用户名/.nuget`
2. .net 8 runtime 
3. __mysql安装包__
4. 发布到本地文件夹
6. 运行 `windowsdesktop-runtime-8.0.22-win-x64.exe` 这是 net8 桌面运行时


## 配置取流
- `RailwayMonitorServer`
    - 删除藏在某处的laoda.jpg，孩子们，想我了吗？
    - 离线运行 `"Offline": false`
    - 数据库连接字符串 `"ConnectionStrings": {
      "DefaultConnection": "server=localhost;port=33206;database=ocean;user=aotie;password=Aotie3199@;Connection Lifetime=31536000;Min Pool Size=5;Max Pool Size=20;Pooling=true;",
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


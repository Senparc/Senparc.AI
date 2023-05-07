## 命令示例行使用说明

### 1. 打开解决方案

打开解决方案 `Senparc.AI.sln`，设置 appsettings.json 中 ApiKey 等平台参数，启动项目 `Senparc.AI.Samples.Consoles`：

<img width="826" alt="image" src="https://user-images.githubusercontent.com/2281927/233587658-d57e30de-dc97-42c7-901f-70327f4eee00.png">

### 2. 操作

#### 2.1 对话

输入 `1`，进入对话操作：

<img width="674" alt="image" src="https://user-images.githubusercontent.com/2281927/233588902-8786e582-7384-4a59-895f-1e8eaaa805b4.png">

#### 2.2 Embedding

在上一步输入 `2` 即可进入 Embedding 操作，Embedding 分为常规信息和引用信息两类，将在下一步中做选择：

<img width="598" alt="image" src="https://user-images.githubusercontent.com/2281927/233589177-b9ab0863-f397-4cba-9d0b-6039a5e2baeb.png">

#### 2.2.1 常规 Embedding（Information）

选择 `1`，进入到常规 Embedding 测试，输入信息由 3 个英文冒号分割，录入完成后输入 `n` 开始对话测试：

<img width="1175" alt="image" src="https://user-images.githubusercontent.com/2281927/233590261-9bb70435-e513-49c9-bda2-a9c0e7f883c4.png">

#### 2.2.2 引用 Embedding（Reference）
2.2.2 上一步选择 `2`，进入到引用 Embedding 测试，输入信息由 3 个英文冒号分割，录入完成后输入 `n` 开始对话测试：

<img width="1176" alt="image" src="https://user-images.githubusercontent.com/2281927/233590721-c9414ffb-27db-4923-a9f9-0580dc10d275.png">

#### 2.3 DallE 绘图操作

初始界面中输入 `3`，进入 DallE 接口的绘图操作：

<img width="1175" alt="image" src="https://user-images.githubusercontent.com/2281927/233681813-ad49e8dc-c69e-4798-b023-903857cd4351.png">

结果将以 URL 的形式返回，此时出入 `s` ，可保存图片到本地：
<img width="503" alt="image" src="https://user-images.githubusercontent.com/2281927/233681967-61b7e4cc-8962-4c36-8593-13a45595330c.png">

> 注意：接口返回的 URL 是一个暂存地址，不可用于持久化的展示，需要及时保存，

#### 2.4 Planner

在第一步中根据已有的 Skill 进行提供，如 SummarizeSkill，WriterSkill 等等。

第二步中直接提供 Plan 的目标。 

## Embedding 测试素材

### Embedding 普通信息测试
```
1:::My name is Jeffrey
2:::My country is China
3:::I'm working for Senparc
4:::Senparc is a company based in Suzhou
5:::Senparc is founded at 2010
6:::Senparc startup at 2010, in Jiangsu Province, Suzhou City, Gusu Distinct
7:::Senparc.Weixin SDK is made by Senparc, main author is Jeffrey Su who is also the founder of Senparc.
8:::Jeffrey Su's Chinese name is Zhenwei Su.
9:::Zhenwei Su wrote two books.
```

### Embedding 引用信息测试
```
https://github.com/NeuCharFramework/NcfDocs/blob/main/start/home/index.md:::README: NCF 简介，源码地址，QQ 技术交流群"
https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/get-docs.md:::获取文档，在线阅读官方文档，在 NCF 站点中进入官方文档，下载源码后使用 npm 本地运行，下载文档源码，运行 npm 命令
https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/run-ncf.md:::使用 Visual Studio 运行 NCF
```

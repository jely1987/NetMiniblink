# NetMiniblink
#### 介绍
免费版miniblink的C#封装，miniblink官网：http://miniblink.net
- 面向对象，符合.NET使用习惯。
- 支持AnyCPU生成exe。
- 支持b/s开发模式，给习惯web的人用，关键是使用ajax不会卡界面，并且分离界面和逻辑。
- 本封装是面向UI的，不是浏览器！

![view](https://images.gitee.com/uploads/images/2020/0304/133534_fcad9dea_307669.png "view")

发布日志和下载：(https://gitee.com/aochulai/NetMiniblink/releases)

### 2019-12-18
- Cookie现在支持新增了，之前只能读取。
- 提供一个缓存框架，但并没有默认实现，有兴趣的自己扩展。
- 修复BindNetFunc会被GC回收的问题。
- 基于Miniblink的下载回调新增一个默认的下载实现。
- 优化和补充细节。
- 某些结构变动。


### 2019-12-03
- 新增一个资源加载类：ZipLoader，用于从ZIP文件中加载资源，支持密码，支持内嵌。


### 2019-11-30
- 换了个更好看的演示Demo。
- NavigateBefore事件中新增2个类型：BlankLink和WindowOpen
- 修复[NetFunc]方法如果参数是object类型会报错的问题。


### 2019-10-07
- 截图实现方式换成了滚动截屏，为了稳定建议自己控制下网页内容，不过打印下报表什么的还是可以的。


### 2019-09-15
- 消息处理不再基于拦截winform的处理过程，改为winform本身提供的方式。
- 重新实现并增强alert,confirm,prompt，通过对应的事件有完全控制能力。
- 资源加载默认提供本地资源和嵌入式资源两个handler。
- 新增一个Cookies属性，读取方便些。
- 提供window.open事件，默认会自动加载open的地址。
- 文件拖拽控制与事件：window.addEventListener("dropFile", function(e){  });


### 2019-08-18
- 新增打印功能
- 新增禁用cookie
- 获取或设置请求的Header
- 获取Post的请求体
- 一些其他的细节


### 2019-07-15
- 改了好几个显示错误的大BUG。
- 可以使用AnyCPU生成了exe了。
- 完善了一点事件细节。
- 更换了仓库地址。


### 2019-07-14
- 修复js类型判断的错误。
- 修复因为兼容x64导致的中文乱码。


### 2019-06-26
- 主要是新增了截图功能，方便大家打印报表啥的。
- 其次是重构了2个基础事件触发方式，打算尽量减少使用WinApi。
- 最后是修了几个比较明显的BUG。


### 2018-10-13
- 很多人不看这个文件，所以在页面里加一个打开源码地址的按钮，哈哈。


### 2018-9-4
- 删除了看着碍眼的MiniblinkReady事件，现在实例化之后就直接可以用了。
- 新增窗体阴影。


### 2018-9-1
- 新增一个控件形式的Miniblink（MiniblinkBrowser）。
- MiniblinkForm改成MiniblinkBrowser的包装类，但是额外加了一些和窗体有关的功能。
- MiniblinkForm新增无边框模式下调整窗体大小。


### 2018-8-23
- 目标框架改为.NET4.0
- 新增NetFuncAttribute，绑定Net方法到Js更简单啦。
- 新增TempNetFunc，专门针对一次性使用的委托。


### 2018-8-21
- 改了一下，现在打开窗体设计界面不会报错了。


### 2018-8-19
- 完成基本封装，很多暂时用不上的事件或者属性没写。
- 主要是加了NetApiEngine，使得可以把逻辑代码放在窗体之外。
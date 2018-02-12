# WiX Toolset制作完全自定义界面的Windows安装程序
借助WiX提供的Bootstrapper和Burn技术，编写WPF MVVM图形界面类库，来实现自定义用户界面的捆绑安装。
- ### [目录](#目录)
    - [实现的功能](#实现的功能)
    - [项目流程](#项目流程)
    - [如何使用Demo](#如何使用demo)
        - [下载安装WiX](#下载安装wix)
        - [下载源码到本地](#下载源码到本地)
        - [添加要打包的项目引用](#添加要打包的项目引用)
        - [修改生成的安装包名称](#修改生成的安装包名称)
        - [编辑Product](#编辑product)
            - [修改名称等文字信息](#修改名称等文字信息)
            - [修改要打包安装的文件](#修改要打包安装的文件)
            - [批量添加多个文件](#批量添加多个文件)
            	- [将路径下所有文件引入](#将路径下所有文件引入)
                - [修改生成的wxs文件](#修改生成的wxs文件)
            - [添加自定义变量](#添加自定义变量)
            - [如何创建桌面与开始桌面快捷方式](#如何创建桌面与开始桌面快捷方式)
        - [编辑Bundle](#编辑bundle)
            - [修改文字信息](#修改文字信息)
            - [引用wpf类库](#引用wpf类库)
            - [定义变量](#定义变量)
            - [安装多个msi或者exe文件](#安装多个msi或者exe文件)
            - [获取用户输入信息并传递给Product](#获取用户输入信息并传递给product)
		- [WPF类库与MVVM模式](#wpf类库与mvvm模式)
    - [如何从零开发](#如何从零开发)
        - [下载并安装WiX](#下载并安装wix)
        - [创建解决方案](#创建解决方案)
        - [向自定义C#类库添加必要的引用](#向自定义类库添加必要的引用)
        - [扩展BootstrapperApplication类](#扩展bootstrapperapplication类)
        - [定义Model](#定义model)
        - [定义ViewModel](#定义viewmodel)
        - [创建View](#创建view)
        - [将C#类库引入Bootstrapper](#将类库引入bootstrapper)
        - [使用Setup Project打包你的程序](#使用setup-project打包你的程序)
        - [使用Bootstrapper打包msi文件](#使用bootstrapper打包msi文件)
        - [Product文件中元素与属性的简单说明](#product文件中元素与属性的简单说明)
            - [Product元素](#product元素)
            - [Package元素](#package元素)
            - [MajorUpgrade元素](#majorupgrade元素)
            - [MediaTemplate元素](#mediatemplate元素)
            - [Directory元素](#directory元素)
            - [Feature元素](#feature元素)
            - [Component元素](#component元素)
            - [File元素](#file元素)
            - [创建快捷方式](#创建快捷方式)
            - [添加到注册表](#添加到注册表)
            - [条件判断](#条件判断)
        - [Bundle文件中元素与属性的简单说明](#bundle文件中元素与属性的简单说明)
            - [Chain元素](#chain元素)
            - [判断当前操作系统版本是否满足](#判断当前操作系统版本是否满足)
            - [判断.net framework版本并下载安装](#判断framework版本并下载安装)
    - [参考内容](#参考内容)
        - [WiX下载地址](#wix下载地址)
        - [参考教程](#参考教程)

## 实现的功能
* 将使用Visual Studio 开发的windows软件打包为安装软件（.exe）
* 具有安装/卸载/修复/的功能，可判断是否已安装旧版本
* 判断是否已安装所依赖的其他软件，如.net framework，支持package从网址自动下载安装
* 获取用户输入信息作为安装包内的某些属性值，如用户姓名，是否创建快捷方式，安装路径等

## 项目流程
* 拥有要打包的WINDOWS项目或要打包的文件
* 建立WiX Setup Project将上述文件打包为 .msi安装文件，本项目可以创建用户界面但默认是没有，我们也不需要
* 建立Bootstrapper Project对生成的 .msi安装文件进行包装，并对依赖的外部环境或者软件进行判断安装，提供安装界面
* 建立c# wpf类库，自定义用户安装行为与界面，实现与ootstrapper Project的通信  

![](/imag/VisualStudio2015打包方式.png)

* * *

## 如何使用Demo
### 下载安装WiX
从官网下载WiX安装包后进行安装即可，这是免费的。也可以使用Visual Studio的NuGet进行搜索安装。
### 下载源码到本地
下载仓库源码到本地后，使用Visual Studio（开发时使用的2015）打开解决方案Installer.sln。
### 添加要打包的项目引用
将打开后未能成功加载的WpfAppToPackage项目移除并添加要打包的已存在的项目，并且移除DGSetup项目中References对WpfAppToPackage的引用，添加自己要打包的项目的引用。这里这么做是为了向DGSetup项目添加要打包的文件，当然这一步也可以不做，直接删除未成功加载的项目及引用即可，这样就需要通过相对路径添加要打包的文件，[看这里](#修改要打包安装的文件)。
### 修改生成的安装包名称
右键Bootstrapper项目，选择属性，在Installer选项卡的output name属性中修改为要生成的安装包文件名，如 xxx_setup。当然也可以修改Setup Project的生成文件名，我的Demo中他的名称为DGSetup1.msi，若要修改方式与Bootstrapper项目一样，右键属性，修改output name，但要注意的是，一旦修改这一个名称，你就必须要修改如下两个个地方：
* Bootstrapper/Bundle.wxs line 40:  
`<MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">`
* CustomBA/ViewModels/InstallViewModel.CS line 18:  
`private static string MyInstellerName = "DGSetup1.msi";`
- - -
### 编辑Product
在DGSetup项目中Product.wxs文件定义如和将你的文件打包，它是一种xml格式的文件，根节点必须是`<Wix>`，在`<Product>`元素中来定义打包行为。
这里只介绍当你要打包自己的文件时要做哪些更改，具体每个元素及属性的含义请看[文档末尾](#product文件中元素与属性的简单说明),或者查看教程[WiX 3.6](https://www.safaribooksonline.com/library/view/wix-36-a/9781782160427/ch01s02.html)
#### 修改名称等文字信息
要说明的是，所有文字我都放在了zh-cn/zh-cn.wxl文件中，比如产品名称与在注册表中的名称，公司名称，电话，帮助/关于等网址，这样当需要修改这些文字时只需要修改zh-cn.wxl文件即可，在Product.wxs中引用他们可以用这种方式：`!(loc.IdValue)`，如：  
zh-cn.wxl:  

```
<WixLocalization Culture="zh-cn" Codepage="936" Language="4" xmlns="http://schemas.microsoft.com/wix/2006/localization">
   <String Id="ProductName">深瞳人眼摄像机客户端</String>
   ... ...

```
Product.wxs:  
```
	<Product Id="*" Name="!(loc.ProductName)" Language="4" 
           Version="1.1.0.0" 
           Manufacturer="!(loc.Manufacturer)" UpgradeCode="B9F8908C-A947-4D44-8D46-A9804C877629">
           ... ...
```

#### 修改要打包安装的文件
在34~38行定义了需要打包的文件，如果有外部依赖库需要添加也可以放在这里：
```
		<ComponentGroup Id="ProductComponents">
      <Component Id="ProductComponent" Directory="INSTALLFOLDER" Guid="84D3BAE6-2758-4FBD-9714-1E052A066830">
        <File Source="$(var.WpfAppToPackage.TargetPath)" Id="myapplication.exe" KeyPath="yes"></File>
      </Component>
		</ComponentGroup>
```
这里采用的方式引用要打包的WpfAppToPackage项目的输出目录，也可以采用相对路径的形式`Source="..\..\LibraFClient\LibraFClient\bin\Release\LibraFClient.exe"` 或者绝对路径也可以，但需要注意，采用相对路径时需要手动设置Guid的值。  
当你要打包多个文件时，要注意每一个`<Component>`最好只含有一个`<File>`，而且`<File>`的`KeyPath`的值最好为`yes`，因为`KeyPath`文件可以在丢失后使用“修复”功能重新得到，而一个`Component`只能有一个`KeyPath`文件。
#### 批量添加多个文件
##### 将路径下所有文件引入
当需要打包几十几百个文件时，如果自己手动添加`<File>`节点就太麻烦了，所以WiX提供了`heat.exe`工具来批量添加指定文件夹下的所有文件。最简单的使用`heat.exe`的方式是，在我们的`msi`即`Setup Project`项目名称上右键->属性，找到`Build Events`选项卡，在里面的`Pre-build Event Command Line`中添加如下一行命令：
```
yourwixtoolsetpath\heat.exe dir "yourdirpath" -dr INSTALLFOLDER -cg yourComponentId -gg -scom -sreg -sfrag -out "youroutputpath\UtilityHeat.wxs"
```
* `yourwixtoolsetpath` ：wix toolset的安装位置，路径中不要包含空格
* `dir`：要添加打包的文件夹路径，后面是它的值，用双引号括起来
* `-dr`：安装时要把这些文件放置的路径，是`Product.wxs`中某一`<Directory>`的`Id`的值，不需要双引号
* `-cg`：在`Product.wxs`中某一`<ComponentRef>`的`Id`的值
* `-out`:heat.exe会生成一个`wxs`文件，`-out`便是指明这个文件放在什么地方，当然文件名字可以随自己设置

这样在编译项目的时候应该会创建一个`wxs`，然后我们把它引入到我们的`msi`即`Setup Project`项目中就可以了，因为WiX支持跨文件读取节点，所以这个`wxs`文件内定义的所有内容和`Product.wxs`中定义的内容都会相互引用。
还采取另外一种方式，那就是找到`heat.exe`然后在命令行中调用它并指定参数：
我的`heat.exe`路径是`C:\Program Files (x86)\WiX Toolset v3.10\bin\heat.exe`，所以打开命令行工具，定位到`C:\Program Files (x86)\WiX Toolset v3.10\bin`，然后调用`heat.exe`并给他相同的参数：
```
C:\Program Files (x86)\WiX Toolset v3.10\bin>heat.exe dir "yourdirpath" -dr INSTALLFOLDER -cg yourComponentId -gg -scom -sreg -sfrag -out "youroutputpath\UtilityHeat.wxs"
```
控制台会提示你生成成功，然后再把生成的文件引入到项目当中即可。
##### 修改生成的wxs文件
生成成功之后打开wxs文件，会看到他里面是包含了所有要打包的文件引用，但是路径都好像不太对，可以使用批量查找修改的方式将所有路径更改到正确的地方。一般WiX会按照当时要打包的文件夹名称创建一个新的文件夹，然后把所有文件放到里面，如果需要的话可以对文档上方的` <DirectoryRef>`进行修改。
#### 添加自定义变量
当需要获取除安装路径/是否创建桌面快捷方式这两种用户安装时输入的信息，如用户名时，可以使用`<Property>`元素，它的`Id`属性唯一标识它，而`Value`属性则是它的值，他可以定义在Product.wxs中`<Product>`与`</Product>`之间任意的位置，通过`[IdName]`可以引用它的值。当然要获取用户输入的信息，还需要其他的设置：
* 在product.wxs中定义`Property`:`  <Property Id="USERNAME" Value=""></Property>`
* 在Bundle.wxs中引用msi包时，
```
<MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
        <MsiProperty Name="USERNAME" Value="[Username]"/>
      </MsiPackage>
```
* 在viewmodel中，调用`(BootstrapperApplication的实例).Engine.StringVariables[USERNAME] = Username;`

#### 如何创建桌面与开始桌面快捷方式
[看文档后面的内容](#创建快捷方式)
- - -
### 编辑Bundle
在Bootstrapper项目中，Bundle.wxs用于定义要安装哪些依赖程序，以及如何验证这些依赖程序已经安装，并且定义安装界面的样式。这里只说明你打包自己的文件时需要修改的地方，元素与属性的详细说明请查看[文档末尾](#)或者[教程](https://www.safaribooksonline.com/library/view/wix-36-a/9781782160427/ch15.html)。
#### 修改文字信息
和Product一样，我将所有文字信息都放在了zh-cn/zh-cn.wxl文件中，如软件名称，公司名，联系电话等等。引用方式和Product相同：`!(loc.IdName)`。
#### 引用wpf类库
 在`<BootstrapperApplicationRef Id="ManagedBootstrapperApplicationHost" >`与`</BootstrapperApplicationRef>`之间通过`<Payload>`来添加程序集的引用，这里引用了自定义的wpf界面类库CustomBA.dll，以及其他依赖的dll:
 ```
  <BootstrapperApplicationRef Id="ManagedBootstrapperApplicationHost" >
      <Payload SourceFile="$(var.CustomBA.TargetDir)CustomBA.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)BootstrapperCore.config" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Composition.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Interactivity.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Mvvm.Desktop.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Mvvm.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.PubSubEvents.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.SharedInterfaces.dll" />
    </BootstrapperApplicationRef>
 ```
#### 定义变量
`<?define VariableName=Value ?>`，在Bundle中定义一个名为VariableName值为Value的变量，可通过`$(var.VariableName)`进行引用。
#### 安装多个msi或者exe文件
在`<Chain>`当中，通过`<ExePackage>`来指定要安装.exe文件，`<MsiPackage>`来指定要安装.msi文件，`SourceFile`属性则指定文件源路径，`DisplayInternalUI="no"`表示不显示该文件执行时显示的界面来实现静默安装，`Compressed="no"`表示不把这个.exe或者.msi压缩到最终生成的安装包中以减小最终文件的大小，`DownloadUrl="$(var.DoNetDownloadUrl)" `指定当安装时若从 `SourceFile`指定路径找不到该文件则从该网址中下载。不过在你build项目时需要该文件存在于你的`SourceFile`路径中。
```
<Chain  DisableRollback="yes">
      <MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup.msi" DisplayInternalUI="no">
      </MsiPackage>
      <MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
      </MsiPackage>
    </Chain>
```
#### 获取用户输入信息并传递给Product
如[这里](#添加自定义变量)所说，要把变量从viewmdoel传给Product需要进行依次的传值。InstallPageViewModel是安装界面的viewmodel，它定义了一个属性名为“CreateShortCut”的公有bool变量，绑定到用户是否勾选了“创建桌面快捷方式”，并在其`setter`语句块中调用:
```
set{
	... ...
	this.SetBurnVariable("CreateShortCut", bol);
}
```
将值传入到burn中，在Bundle.wxs中`<Chain>`中的`<MsiPackage>`与`</MsiPackage>`中间获取该值:
```
<MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
        <MsiProperty Name="CreateShortcutDeskTop" Value="[CreateShortCut]"/>
      </MsiPackage>
```
在Product.wxs中可直接通过`[CREATESHORTCUTDESKTOP]`(将变量名改为大写)获取Bundle传递的值，而后在创建桌面快捷方式的`<Component>`中加一个验证条件，当`CREATESHORTCUTDESKTOP`属性的值为`True`时创建桌面快捷方式，否则不创建：
```
 <Component Id="ApplicationShortcutDeskTop">
+          <Condition>
+            <![CDATA[CREATESHORTCUTDESKTOP="True"]]>
+          </Condition>
          <Shortcut Id="ApplicationDeskTopShortcut"
                 Name="!(loc.ProductName)"
                 Description="!(loc.Title)"
                 Target="[#myapplication.exe]" Icon="icon"
                 WorkingDirectory="APPLICATIONROOTDIRECTORY"/>
          <RemoveFolder Id="DesktopFolder" On="uninstall"/>
          <RegistryValue
              Root="HKCU"
              Key="Software\Microsoft\!(loc.CompanyName)\!(loc.RegistryName)"
              Name="installed"
              Type="integer"
              Value="1"
              KeyPath="yes"/>
        </Component>
```
- - -
### WPF类库与MVVM模式

![](/imag/BootstrapperWPF.png)

* * *




## 如何从零开发
### 下载并安装WiX
从官网下载WiX安装包后进行安装即可，这是免费的。也可以使用Visual Studio的NuGet进行搜索安装。
### 创建解决方案
1. 打开Visual Studio（我使用的是VS2015），新建项目(New Project)—>其他项目类型(Other Project Types)—>**Visual Studio解决方案(Visual Studio Solutions)**，修改项目名称与存储位置（我这里命名为WiXPackage）。
2. 引用要打包的项目，在资源管理器中右击已经建好的空白解决方案，选择添加——>已存在的项目，在打开的对话框中找到要打包的项目添加进来（选择.csproj文件）。
3. 创建 **WiX Setup Project** ，在当前解决方案上右键添加——>新项目，如果已经正确安装WiX那么会在打开的项目模板列表左边看到 **Windows Installer XML** ,点击之后在右边选中 **Setup Project** ，按自己需要修改名称并确定。此时会看到解决方案资源管理器中看到一个有 **Product.wxs** 文件和 **References** 引用文件夹。在References右键—>添加引用—>项目，添加刚才引用的要打包的项目。
4. 创建 **Bootstrapper** 项目，在当前解决方案上右键添加——>新项目，同样找到 **Windows Installer XML** ,在右边选中 **Bootstrapper Project** ，按自己需要修改名称并确定。此时会看到解决方案资源管理器中看到一个有 **Bundle.wxs** 文件和 **References** 引用文件夹。
5. 创建C#类库，在当前解决方案上右键添加——>新项目，以此找到Visaul C#—>**Class Library**，命名为CustomBA。向References需要添加Bootstrapper程序集的引用，在WiX的安装路径（C:\Program Files (x86)\WiX Toolset v3.10\SDK）中找到**BootstrapperCore.dll**并添加引用。在SDK目录下还能看见名为 **BootstrapperCore.config** 的文件，将它复制粘贴到这个类库项目文件中，打开它并且将他的 **host** 元素的assemblyName属性值改为本类库名（CustomBA）。

### 向自定义类库添加必要的引用
1. 为了要使用WPF来自定义界面，还需要向C#类库添加如下引用：
	* PresentationCore
	* PresentationFramework
	* System.Xaml
	* WindowsBase
1. 为了要使用MVVM模式，可以使用NuGet来向C#类库安装Prism，在VS菜单栏上依次点击工具—>NuGet Package—>管理NuGet解决方案，在浏览中搜索Prism并安装到CustomBA解决方案中。
1. 在C#类库中新建一个public类，名称为`CustomBootstrapperApplication`，暂时先不需要添加任何内容，而后在类库的`Properties\AssemblyInfo.cs`文件中添加如下代码，它指定Burn调用`CustomBootstrapperApplication`类的`Run`方法作为程序入口：
```
using CustomBA;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
[assembly: BootstrapperApplication(
   typeof(CustomBootstrapperApplication))]
```
4. 创建Views，ViewModels，Models文件夹，在C#类库项目中新建以上三个文件夹，依次存放MVVM模式中的Views，ViewModels和Models。

### 扩展BootstrapperApplication类
刚才在类库中新建了一个名为`CustomBootstrapperApplication`的类，现在我们对他进行修改。修改后的完整代码如下：
```
using CustomBA.Models;
using CustomBA.ViewModels;
using CustomBA.Views;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Windows.Threading;

namespace CustomBA
{
    public class CustomBootstrapperApplication :BootstrapperApplication
    {
        public static Dispatcher Dispatcher { get; set; }
        protected override void Run()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;

            var model = BootstrapperApplicationModel.GetBootstrapperAppModel(this);
            var view = new InstallView();

            model.SetWindowHandle(view);

            this.Engine.Detect();

            view.Show();
            Dispatcher.Run();
            this.Engine.Quit(model.FinalResult);
        }
    }
}
```
* 让`CustomBootstrapperApplication`类继承`BootstrapperApplication`类并重写他的`Run`方法，这便是安装程序的入口。  
* 在`Run`方法中，定义一个`Dispatcher`对象来惊醒UI线程与其他线程的通信，定义`model`和`view`来设置显示界面
* `SetWindowHandle`方法是待会要在`model`类中要写的方法，它给Burn传递要显示的`view`
* `Engine.Detect()`用来检测这个bundle是否已经被安装
* `Detect`需要在`view`,`viewmodel`和`model`实例化之后再被调用，因为它需要的一些配置信息需要被设置。
* `Show()`方法则显示图形界面，`Dispatcher.Run()`来启动线程，当安装完成或者取消时调用`Dispatcher.InvokeShutdown（）`可以结束线程。
* `Engine.Quit()`无论安装结果如何都会关闭安装任务。

### 定义Model
在Models文件夹中新建`BootstrapperApplicationModel`类并修改为如下内容：
```
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Windows;
using System.Windows.Interop;

namespace CustomBA.Models
{
    public class BootstrapperApplicationModel
    {
        private IntPtr hwnd;
        private static BootstrapperApplicationModel bootstrapperAppModel;
        public static BootstrapperApplicationModel GetBootstrapperAppModel(BootstrapperApplication bootstrapperApplication)
        {
            if (bootstrapperAppModel == null)
                bootstrapperAppModel = new BootstrapperApplicationModel(bootstrapperApplication);
            return bootstrapperAppModel;
        }
        public static BootstrapperApplicationModel GetBootstrapperAppModel()
        {
             return bootstrapperAppModel;
        }
        private BootstrapperApplicationModel(BootstrapperApplication bootstrapperApplication)
        {
            this.BootstrapperApplication =
              bootstrapperApplication;
            this.hwnd = IntPtr.Zero;
            string[] strs = GetCommandLine();
        }

        public BootstrapperApplication BootstrapperApplication { get; private set; }

        public int FinalResult { get; set; }

        public void SetWindowHandle(Window view)
        {
            this.hwnd = new WindowInteropHelper(view).Handle;
        }

        public void PlanAction(LaunchAction action)
        {
            this.BootstrapperApplication.Engine.Plan(action);
        }

        public void ApplyAction()
        {
            this.BootstrapperApplication.Engine.Apply(this.hwnd);
        }

        public void LogMessage(string message)
        {
            this.BootstrapperApplication.Engine.Log(
              LogLevel.Standard,
              message);
        }
        public void SetBurnVariable(string variableName, string value)
        {
            this.BootstrapperApplication.Engine
               .StringVariables[variableName] = value;
        }
        public string[] GetCommandLine()
        {
            return this.BootstrapperApplication.Command
               .GetCommandLineArgs();
        }
        public bool HelpRequested()
        {
            return this.BootstrapperApplication.Command.Action ==
               LaunchAction.Help;
        }
    }
    public enum InstallState
    {
        Initializing,
        Present,
        NotPresent,
        Applying,
        Cancelled,
        Applied,
        Failed,
    }
}
```
该类为WPF程序的Model类，它封装了继承自BootstrapperApplication（来自于BootstrapperCore.dll）的CustomBootstrapperApplication的方法，比如设置界面实例`SetWindowHandle()`，配置安装信息`PlanAction()`，执行动作包括安装/卸载/修复`ApplyAction()`，向Burn传递参数`SetBurnVariable()`等。
这个类定义为一个单例，方便在每一个viewmodel中获取拥有当前安装程序信息，因为它的构造函数需要一个`BootstrapperApplication`的参数，在程序入口处实例化Model并传递`this`作为参数，那么之后的所有viewmodel都可以访问该单例Model而无需再获取一个BootstrapperApplication来实例化Model。

### 定义ViewModel
#### InstallView窗口的ViewModel： InstallViewModel
在ViewModels文件夹中新建名为InstallViewModel的类，并修改其内容如下：
```
using CustomBA.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CustomBA.ViewModels
{
    public class InstallViewModel : BaseViewModel
    {
        private string bit3264 = @"SOFTWARE\WOW6432Node\Microsoft\DeepGlint";
        private string bit32 = @"SOFTWARE\Microsoft\DeepGlint";
        private string productName = "LibraFClinet";
        private static string MyInstellerName = "DGSetup1.msi";
        private InstallState state;
        private static InstallViewModel viewmodel;
        /// <summary>
        /// 构造函数，使用单例模式
        /// </summary>
        private InstallViewModel()
        {
            this.State = InstallState.Initializing;
            this.WireUpEventHandlers();
            ///检查安装文件是否为空
            this.model.BootstrapperApplication.ResolveSource +=
               (sender, args) =>
               {
                   if (!string.IsNullOrEmpty(args.DownloadSource))
                   {
                       // Downloadable package found 
                       args.Result = Result.Download;
                   }
                   else
                   {
                       // Not downloadable 
                       args.Result = Result.Ok;
                   }
               };
        }
        public static InstallViewModel GetViewModel()
        {
            if (viewmodel == null)
                viewmodel = new InstallViewModel();
            return viewmodel;
        }
        private BootstrapperApplicationModel model
        {
            get
            {
                return BootstrapperApplicationModel.GetBootstrapperAppModel();
            }
        }
        /// <summary>
        /// 当前产品状态
        /// </summary>
        
        public InstallState State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (this.state != value)
                {
                    this.state = value;
                    if (state == InstallState.NotPresent)
                        if (ChekExistFromRegistry())
                        {
                            state = InstallState.Cancelled;
                        }
                    OnPropertyChanged("State");
                    OnPropertyChanged("CancelEnabled");
                    OnPropertyChanged("InstallEnabled");
                    OnPropertyChanged("UninstallEnabled");
                    OnPropertyChanged("ProgressEnabled");
                    OnPropertyChanged("FinishEnabled");
                }
                
            }
        }
        /// <summary>
        /// 与 InstallEnabled，UninstallEnabled,ProgressEnabled,FinishEnabled
        /// 根据State的值，判断该显示哪个界面
        /// </summary>
        public bool CancelEnabled
        {
            get
            {
                return State == InstallState.Cancelled;
            }
        }
        public bool InstallEnabled
        {
            get {
                return State == InstallState.NotPresent;
            }
        }

        public bool UninstallEnabled
        {
            get
            {
                return State == InstallState.Present;
            }
        }
        public bool ProgressEnabled
        {
            get
            {
                return State == InstallState.Applying;
            }
        }
        public bool FinishEnabled
        {
            get
            {
                return State == InstallState.Applied;
            }
        }


        protected void DetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId.Equals(MyInstellerName, StringComparison.Ordinal))
            {
                this.State = e.State == PackageState.Present ?
                  InstallState.Present : InstallState.NotPresent;
            }

        }

        private void WireUpEventHandlers()
        {
            //重新定义`BootstrapperApplication`的安装包验证方式，判断是当前msi安装包是否已经安装过
            this.model.BootstrapperApplication.DetectPackageComplete += this.DetectPackageComplete;
        }
        /// <summary>
        /// 查找注册表看是否已经安装本产品的其他版本
        /// </summary>
        /// <returns></returns>
        protected bool ChekExistFromRegistry()
        {
            try
            {
                //64位
                using (RegistryKey pathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(bit3264))
                {
                    var strs = pathKey.GetSubKeyNames();
                    foreach (string str in strs)
                        if (str.Equals(productName))
                        {
                            return true;
                        }
                }
                //32位
                using (RegistryKey pathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(bit32))
                {
                    var strs = pathKey.GetSubKeyNames();
                    foreach (string str in strs)
                        if (str.Equals(productName))
                        {
                            return true;
                        }
                }
            }
            catch { }
            return false;
        }
    }
}
```

#### 安装界面InstallPage的viewmodel：InstallPageViewModel
在ViewModels文件夹中新建`InstallPageViewModel`类并修改以下内容：
```
uusing CustomBA.HelpClass;
using CustomBA.Models;
using CustomBA.Views;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CustomBA.ViewModels
{
    public class InstallPageViewModel : BaseViewModel
    {
        private static string SoftWareName = "wpfapptopackage";
        private bool createShortCut;
        private string installFolder;
        private Visibility selectFileVisibility;
        private DelegateCommand BrowseCommand;
        private DelegateCommand InstallCommand;
        private DelegateCommand CloseCommand;
        private DelegateCommand ShowSelecFileCommand;

        /// <summary>
        /// InstallViewModel单例的引用，用来修改InstallViewModel.State
        /// 以实时根据当前状态更改显示的界面内容
        /// </summary>
        private InstallViewModel installViewModel
        {
            get { return InstallViewModel.GetViewModel(); }
        }
        /// <summary>
        /// Model单例的引用，用来定义事件触发执行什么操作
        /// 同时通过该引用的某些方法来执行安装功能
        /// </summary>
        private BootstrapperApplicationModel BootstrapperModel
        {
            get
            {
                return BootstrapperApplicationModel.GetBootstrapperAppModel();
            }
        }
        public InstallPageViewModel()
        {
            InitialCommand();
            SeleFileVisibility = Visibility.Collapsed;
            CreateShortCut = true;
            InstallFolder = @"C:\Program Files (x86)\DeepGlin\" + SoftWareName;
            WireUpEventHandlers();
        }

        /// <summary>
        /// 背景图片资源
        /// </summary>
        public BitmapSource BackImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.page1;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }
        public BitmapSource LogoImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.logo;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }
        /// <summary>
        /// 自定义安装路径的控件组是否显示
        /// </summary>
        public Visibility SeleFileVisibility
        {
            get { return selectFileVisibility; }
            set
            {
                selectFileVisibility = value;
                OnPropertyChanged("SeleFileVisibility");
            }
        }
       /// <summary>
       /// 是否创建桌面快捷方式
       /// </summary>
        public bool CreateShortCut
        {
            get { return createShortCut; }
            set
            {
                createShortCut = value;
                OnPropertyChanged("CreateShortCut");
                this.SetBurnVariable("CreateShortCut", createShortCut.ToString());
            }
        }
       /// <summary>
       /// 自定义安装路径
       /// 在用户选择的路径后再创建一个当前软件名称的文件夹，
       /// 使安装不混乱在根目录中
       /// </summary>
        public string InstallFolder
        {
            get { return installFolder; }
            set
            {
                try {
                    if (value != installFolder && ValidDir(value))
                    {
                        string[] para = value.Split('\\');
                        bool hassoftwarename = false;
                        foreach (string pa in para)
                        {
                            if (pa == SoftWareName)
                                hassoftwarename = true;
                        }
                        if (hassoftwarename)
                            installFolder = value;
                        else
                            installFolder = value + "\\" + SoftWareName;
                        OnPropertyChanged("InstallFolder");
                        this.SetBurnVariable("InstallFolder", installFolder);
                    }
                }
                catch {
                    installFolder = value;
                }
            }
        }

        /// <summary>
        /// 界面四个按钮的单击事件
        /// </summary>

        public ICommand btn_browse
        {
            get { return BrowseCommand; }
        }
        public ICommand btn_install
        {
            get { return InstallCommand; }
        }
        public ICommand btn_cancel
        {
            get { return CloseCommand; }
        }
        public ICommand btn_show
        {
            get { return ShowSelecFileCommand; }
        }
        /// <summary>
        /// 初始化按钮点击命令要调用的函数
        /// </summary>
        private void InitialCommand()
        {
            BrowseCommand = new DelegateCommand(Browse, IsValid);
            InstallCommand = new DelegateCommand(Install, IsValid);
            CloseCommand = new DelegateCommand(Close, IsValid);
            ShowSelecFileCommand = new DelegateCommand(Show, IsValid);
        }
        /// <summary>
        /// 打开选择安装路径窗口并获取用户选择的路径
        /// </summary>
        public void Browse()
        {
            var folderBrowserDialog = new FolderBrowserDialog { SelectedPath = InstallFolder };

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InstallFolder = folderBrowserDialog.SelectedPath;
            }
        }
        /// <summary>
        /// 开始安装
        /// 调用PlanAction()方法而不是ApplyAction()
        /// 此时只是开始执行配置安装信息，并不会执行安装进程
        /// </summary>
        public void Install()
        {
            this.BootstrapperModel.PlanAction(LaunchAction.Install);
        }
        /// <summary>
        /// 取消安装，关闭安装进程
        /// </summary>
        public void Close()
        {
             installViewModel.State = InstallState.Cancelled;
             CustomBootstrapperApplication.Dispatcher.InvokeShutdown();
        }
        /// <summary>
        /// 显示/关闭自定义安装界面
        /// </summary>
        public void Show()
        {
            if (SeleFileVisibility == Visibility.Collapsed)
                SeleFileVisibility = Visibility.Visible;
            else
                SeleFileVisibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 当安装进程开始时触发事件
        /// 将当前状态更改位Applying
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            this.installViewModel.State = InstallState.Applying;
        }
        /// <summary>
        /// 开始安装时调用的方法位PlanAction()
        /// 该方法执行完成之后触发本事件
        /// 事件中调用ApplyAction()方法开始执行安装进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (installViewModel.State == InstallState.Cancelled)
            {
                CustomBootstrapperApplication.Dispatcher
                  .InvokeShutdown();
                return;
            }
            this.BootstrapperModel.ApplyAction();
        }

        /// <summary>
        /// 注册BootstrapperApplication的两个事件
        /// </summary>
        private void WireUpEventHandlers()
        {
            this.BootstrapperModel.BootstrapperApplication.PlanComplete += this.PlanComplete;
            this.BootstrapperModel.BootstrapperApplication.ApplyBegin += this.ApplyBegin;
        }

        /// <summary>
        /// pathj是否是正确的文件夹路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ValidDir(string path)
        {
            try
            {
                string p = new DirectoryInfo(path).FullName;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 向Burn传递用户参数
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public void SetBurnVariable(string variableName, string value)
        {
            this.BootstrapperModel.SetBurnVariable(variableName, value);
        }
        public bool IsValid()
        {
            return true;
        }
    }
}

```
其他ViewModel的用法与这两个类似，不在赘述，请按照提供的源码自行设置
### 创建View
关于界面元素的定义，样式，文字，binding的数据，都是基础的WPF操作，请通过源码自行查看。需要说明的是，当你打开`InstallView`时xaml中会提示“对象未被设置到实例”，这是因为我们的单例Model只能再程序入口实例化，而这个入口不会在CustomBA项目编译时执行，只会通过Bootstrapper安装进程调用。
### 将类库引入Bootstrapper
当前面的工作做完之后编译CustomeBA项目，注意使用Release方式而不是Debug哦。如果一切顺利的话你会得到一个CustomBA.dll库文件。
* 首先，在Bootstrapper项目的引用文件夹中添加对CustomBA.dll的引用。
* 而后在Bundle.wxs文件`<Bundle>`与`</Bundle>`之间添加如下代码即可将我们自定义的类库和WPF程序需要的类库引入Bootstrapper项目：
```
  <BootstrapperApplicationRef Id="ManagedBootstrapperApplicationHost" >
      <Payload SourceFile="$(var.CustomBA.TargetDir)CustomBA.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)BootstrapperCore.config" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Composition.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Interactivity.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Mvvm.Desktop.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.Mvvm.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.PubSubEvents.dll" />
      <Payload SourceFile="$(var.CustomBA.TargetDir)Microsoft.Practices.Prism.SharedInterfaces.dll" />
    </BootstrapperApplicationRef>
```
### 使用Setup Project打包你的程序
在创建的Setup Project项目中打开Product.wxs文件，将
```
<Directory Id="TARGETDIR"
           Name="SourceDir">
```
修改为
```
<Directory Id="TARGETDIR"
           Name="SourceDir">
  <Directory Id="ProgramFilesFolder">
    <Directory Id="MyProgramDir"
               Name="Install Practice">

      <Component Id="CMP_InstallMeTXT"
                 Guid="E8A58B7B-F031-4548-9BDD-7A6796C8460D">

        <File Id="FILE_MyProgramDir_InstallMeTXT"
              Source="$(var.MyAppname.TargetPath)"
              KeyPath="yes" />
      </Component>
    </Directory>
  </Directory>
</Directory>
```
其中`MyAppname`为你已经引用的要打包的项目名称，当然你也可以用相对路径或则绝对路径指定任意的文件，这段代码表示要把这里所有的文件打包为msi安装文件，安装时会把文件安装在`C:\ProgrameFiles(86)\Install Practice`中。
此时若要Build该项目，要先保证`<Product>`的`Manufacturer`已经被赋值。生成之后你会在`bin/Release`目录下看到一个msi文件。
### 使用Bootstrapper打包msi文件
我们已经在Bootstrapper项目中引入了自定义的用户界面，接下来向它添加已经打包好的msi文件就可以生成拥有自定义界面的安装程序了。
在Bundle.wxs文件中，将`<Chain>`结点由原来的：
```
<Chain>
      <!--TODO: Define the list of chained packages.-->
   </Chain>
```
改为：
```
<Chain  DisableRollback="yes">
      <MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
      </MsiPackage>
    </Chain>
```
这里的SourceFile属性要改为你自己的msi文件路径，同时也不要忘了`<Bundle>`的`Manufacturer`属性要赋值。而后就可以Build这个项目了，你会在`bin/Release`文件夹中看到一个.exe安装文件了，运行一下看看是不是你的wpf界面。
关于其他常见的设置，我在前文或者后文中都写到了，比如创建快捷方式，验证.net framework版本，从界面向安装包传值等等，请按[目录](#目录)查看，或者查看末尾提供的参考教程链接。

- - -
### Product文件中元素与属性的简单说明
#### Product元素
* `Id`: 该属性为GUID，可以使用Visual Studio->Tools->Create GUID的第4/5/6类来生成，也可以直接赋值为“\*”，该属性每一次build都需要不同的值
* `Language`：语言序号，1033为en-us，4为zh-cn
* `Version`：软件版本号，用于产品升级
* `Manufacturer`：制造商，公司名或这开发者名称，必填！
* `UpgradeCode`：GUID，产品识别与更新的标识

#### Package元素
```
<Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" />
```
* `InstallScope`：值为"perMachine"表明将软件安装到当前机器，值为"perUser"则将软件安装到当前用户。

#### MajorUpgrade元素
当产品更新时检验版本，若已安装高级版本则显示提示信息
#### MediaTemplate元素
用于将打包的文件声明一个存储的磁盘文件区域
#### Directory元素
```
<Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="INSTALLFOLDER"/>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="AppStartMenuFolder" Name="!(loc.ProductName)"/>
      </Directory>
      <Directory Id="DesktopFolder"></Directory>
    </Directory>
```
* `INSTALLFOLDER`:安装软件时选中的安装目录，`ProgramFilesFolder`为系统C盘的`ProgrameFiels`文件夹，因为要用户自定义安装位置，因此要把自动生成的`<Directory Id="ProgramFilesFolder">`这一层删除。
* Name:文件夹名称
* 桌面与开始菜单快捷方式的位置为：
```
  <Directory Id="ProgramMenuFolder">
     <Directory Id="AppStartMenuFolder" Name="!(loc.ProductName)"/>
  </Directory>
     <Directory Id="DesktopFolder"></Directory>
```

#### Feature元素
`<Feature>`指定要执行打包的具体内容，内部包含若干`<Component>`，每一个`<Component>`定义一个行为或者文件。
#### Component元素
`<Component>` ,`<ComponentGroup>` ,`<ComponentRef>`和`<ComponentGroupRef>`,Group当中可以包含多个`Component`，Ref通过`Id`与`Component`或者`ComponentGroup`关联，Ref元素声明要添加一个拥有某`Id`值的`Component`或者`ComponentGroup`，而其具体信息可以在另外的地方进行定义，如;
```
<Feature Id="ProductFeature" Title="!(loc.Title)" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
...
<ComponentGroup Id="ProductComponents">
      <Component Id="ProductComponent" Directory="INSTALLFOLDER" Guid="84D3BAE6-2758-4FBD-9714-1E052A066830">
           <File Source="$(var.WpfAppToPackage.TargetPath)" Id="myapplication.exe" KeyPath="yes"></File>
      </Component>
		</ComponentGroup>
```
一个`Component`之中最好只放一个文件，每一个文件最好都设置`KeyPath`为“yes”，这是因为`KeyPath`文件可以在丢失后使用“修复”功能重新得到，而一个`Component`只能有一个`KeyPath`文件。 
#### File元素
一个`<File>`表示要在msi中打包并由客户安装到计算机的文件
#### 创建快捷方式
##### 桌面快捷方式
在Product.wxs的`<Directory Id="TARGETDIR" Name="SourceDir">`中间添加如下内容:
```
<Directory Id="DesktopFolder"></Directory>
```
在Product.wxs的任意位置添加如下内容：
```
 <DirectoryRef Id="DesktopFolder">
        <Component Id="ApplicationShortcutDeskTop">
          <Shortcut Id="ApplicationDeskTopShortcut"
                 Name="!(loc.ProductName)"
                 Description="!(loc.Title)"
                 Target="[#myapplication.exe]" Icon="icon"
                 WorkingDirectory="APPLICATIONROOTDIRECTORY"/>
          <RemoveFolder Id="DesktopFolder" On="uninstall"/>
          <RegistryValue
              Root="HKCU"
              Key="Software\Microsoft\!(loc.CompanyName)\!(loc.RegistryName)"
              Name="installed"
              Type="integer"
              Value="1"
              KeyPath="yes"/>
        </Component>
    </DirectoryRef>
```
`(loc.Name)`是引用同跟目录下`.wxl`文件中定义的字符串。
在`<Feature>`中添加如下内容：
```
<ComponentRef Id="ApplicationShortcutDeskTop" />
```
##### StartMenu快捷方式
在Product.wxs的`<Directory Id="TARGETDIR" Name="SourceDir">`中间添加如下内容:
```
<Directory Id="ProgramMenuFolder">
        <Directory Id="AppStartMenuFolder" Name="!(loc.ProductName)"/>
      </Directory>
```
在Product.wxs的任意位置添加如下内容：
```
<DirectoryRef Id="AppStartMenuFolder">
      <Component Id="ApplicationShortcutMenu" Guid="5AE52100-3B6E-4BA7-8A87-ED50F790B316">
        <Shortcut Id="ApplicationStartMenuShortcut"
                  Name="!(loc.ProductName)"
                  Description="!(loc.Title)"
                  Target="[#myapplication.exe]" Icon="icon"
                  WorkingDirectory="APPLICATIONROOTDIRECTORY"/>
        <RemoveFolder Id="CleanUpShortCut" Directory="AppStartMenuFolder" On="uninstall"/>
        <RegistryValue Root="HKCU" Key="Software\Microsoft\!(loc.CompanyName)\!(loc.RegistryName)" 
                       Name="installed" Type="integer" Value="1" KeyPath="yes"/>
      </Component>
    </DirectoryRef>
```
在`<Feature>`中添加如下内容：
```
<ComponentRef Id="ApplicationShortcutMenu" />
```
#### 添加到注册表
在Product.wxs的`<Directory Id="TARGETDIR" Name="SourceDir">`中间添加如下内容:
```
<ComponentRef Id="WriteToRegistry" />
```
在Product.wxs的任意位置添加如下内容：
```
<Component Id="WriteToRegistry"
                 Guid="1ECA3238-1B7B-4818-9A02-FAD3D6773613">

        <RegistryValue Id="RegistryValue"
                       KeyPath="yes"
                       Action="write"
                       Root="HKLM"
                       Key="Software\Microsoft\!(loc.CompanyName)\!(loc.RegistryName)"
                       Name="!(loc.ProductName)"
                       Value="!(loc.ProductName)"
                       Type="string" />
      </Component>
```
#### 条件判断
当我们从用户获得传过来的值，或者自己在Product中定义了一个`<Property>`时，我们想根据该属性值的不同判断某些操作是否进行，则在该`<Component>`中添加如下内容：
```
<Condition>
            <![CDATA[PROPERTYNAME=VALUE]]>
          </Condition>
```
当你的`PROPERTYNAME`属性值为`VALUE`时该操作才会执行。
- - -

### Bundle文件中元素与属性的简单说明
#### Chain元素
`<Chain>`中间可以放置任意多个安装文件，如果你想依次执行多个msi或者exe安装文件，只需要像这样:
```
 <Chain  DisableRollback="yes">
      <ExePackage ISourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup.exe" DisplayInternalUI="no">
      </ExePackage>
      <MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
      </MsiPackage>
    </Chain>
```
添加就是了，其中`<MsiPackage>`用来引用msi安装文件，`<ExePackage>`用来引用exe安装文件。
#### 判断当前操作系统版本是否满足
在Bootstrapper项目中添加引用WixBalExtension程序集并在`<Bundel>`结点添加命名空间的引用：
```
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi" 
     xmlns:bal="http://schemas.microsoft.com/wix/BalExtension" >
```
在`<Bundle>`元素任意位置添加如下内容：
```
<bal:Condition Message="!(loc.windowsversionmes)">
      <![CDATA[VersionNT >= v6.1]]>
    </bal:Condition>
```
6.1表示windows7。
#### 判断framework版本并下载安装
在Bootstrapper项目中添加引用WixUtilExtension程序集并在`<Bundel>`结点添加命名空间的引用：
```
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi" 
      xmlns:util="http://schemas.microsoft.com/wix/UtilExtension">
```
在`<Bundle>`元素任意位置添加如下内容：
```
 <util:RegistrySearch Root="HKLM" Key="SOFTWARE\Microsoft\Net Framework Setup\NDP\v4\Full" Value="Version" Variable="Netfx4FullVersion" />
    <util:RegistrySearch Root="HKLM" Key="SOFTWARE\Microsoft\Net Framework Setup\NDP\v4\Full" Value="Version" Variable="Netfx4x64FullVersion" Win64="yes" />
```
在`<Chain>`添加：
```
<ExePackage Id="Netfx4Full" Cache="yes" Compressed="no"  PerMachine="yes" 
            Permanent="yes" Vital="yes" SourceFile="..\dotnetfx40_full_x86_x64.exe"
            InstallCommand="/q /norestart "  DownloadUrl="http://go.microsoft.com/fwlink/?LinkId=164193" 
            DetectCondition="Netfx4FullVersion AND (NOT VersionNT64 OR Netfx4x64FullVersion)">
      </ExePackage>
```
在你Build项目时需要在`SourceFile`指定的位置存在dotnetfx40_full_x86_x64.exe文件，但`Compressed="no" `使得该文件不会被打包进你的安装文件，当用户只拿到安装包来安装时若系统中不存在.net 4.0时安装文件就会自动从`DownloadUrl`下载文件进行安装。
## 参考内容
### WiX下载地址
http://wixtoolset.org/releases/ ，虽然已经提供v3.11版本，但当我已经安装.net framework 4.0后，v3.11依旧提示需要安装.net 3.0，所以我使用的是wix v3.10.3 。
### 参考教程
[WiX 3.6: A Developer's Guide to Windows Installer XML](https://www.safaribooksonline.com/library/view/wix-36-a/9781782160427/pr01.html)
[WiX Toolset Manual Table of Contents](http://wixtoolset.org/documentation/manual/v3/)
可能会有错误或者偏差的地方，欢迎交流指正！

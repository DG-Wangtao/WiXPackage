# WiX Toolset制作完全自定义界面的Windows安装程序
借助WiX提供的Bootstrapper和Burn技术，编写WPF MVVM图形界面类库，来实现自定义用户界面的捆绑安装。
## 目录
* [实现的功能](#实现的功能)
* [项目流程](#项目流程)
* [如何使用Demo](#如何使用demo)
    * [1 下载并安装WiX](#下载安装wix)
    * [2 下载源码到本地](#下载源码到本地)
    * [3 添加要打包的项目引用](#添加要打包的项目引用)
    * [4 修改生成的安装包名称](#修改生成的安装包名称)
    * [5 编辑Product.wxs](#编辑product)
    * [6 编辑Bundel.wxs](#编辑bundle)
* [如何从零开发](#如何从零开发)
    * [1 下载并安装WiX](#下载并安装wix)
    * [2 创建解决方案](#创建解决方案)
    * [3 向C#类库添加必要的引用](#向类库添加必要的引用)
    * [4 扩展BootstrapperApplication类](#扩展bootstrapperapplication类)
    * [5 定义Model](#定义model)
* [WiX下载地址](#wix下载地址)
* [参考教程](#参考教程)
        
## 实现的功能;
* 将使用Visual Studio 开发的windows软件打包为安装软件（.exe）
* 具有安装/卸载/修复/的功能，可判断是否已安装旧版本
* 判断是否已安装所依赖的其他软件，如.net framework，支持package从网址自动下载安装
* 获取用户输入信息作为安装包内的某些属性值，如用户姓名，是否创建快捷方式，安装路径等
# 项目流程
* 拥有要打包的WINDOWS项目或要打包的文件
* 建立WiX Setup Project将上述文件打包为 .msi安装文件，本项目可以创建用户界面但默认是没有，我们也不需要
* 建立Bootstrapper Project对生成的 .msi安装文件进行包装，并对依赖的外部环境或者软件进行判断安装，提供安装界面
* 建立c# wpf类库，自定义用户安装行为与界面，实现与ootstrapper Project的通信  
  ![](https://github.com/DG-Wangtao/WiXPackage/blob/master/imag/Visual%20Studio%202015%E6%89%93%E5%8C%85%E6%96%B9%E5%BC%8F.png)  
# 如何使用Demo
## 下载安装WiX
从官网下载WiX安装包后进行安装即可，这是免费的。也可以使用Visual Studio的NuGet进行搜索安装。
## 下载源码到本地
下载仓库源码到本地后，使用Visual Studio（开发时使用的2015）打开解决方案Installer.sln。
## 添加要打包的项目引用
将打开后未能成功加载的WpfAppToPackage项目移除并添加要打包的已存在的项目，并且移除DGSetup项目中References对WpfAppToPackage的引用，添加自己要打包的项目的引用。这里这么做是为了向DGSetup项目添加要打包的文件，当然这一步也可以不做，直接删除未成功加载的项目及引用即可，这样就需要通过相对路径添加要打包的文件，[看这里](#修改要打包安装的文件)。
## 修改生成的安装包名称
右键Bootstrapper项目，选择属性，在Installer选项卡的output name属性中修改为要生成的安装包文件名，如 xxx_setup。当然也可以修改Setup Project的生成文件名，我的Demo中他的名称为DGSetup1.msi，若要修改方式与Bootstrapper项目一样，右键属性，修改output name，但要注意的是，一旦修改这一个名称，你就必须要修改如下两个个地方：
* Bootstrapper/Bundle.wxs line 40:  
`<MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">`
* CustomBA/ViewModels/InstallViewModel.CS line 18:  
`private static string MyInstellerName = "DGSetup1.msi";`
## 编辑Product
在DGSetup项目中Product.wxs文件定义如和将你的文件打包，它是一种xml格式的文件，根节点必须是`<Wix>`，在`<Product>`元素中来定义打包行为。
这里只介绍当你要打包自己的文件时要做哪些更改，具体每个元素及属性的含义请看[文档末尾](#product文件中元素与属性的简单说明),或者查看教程[WiX 3.6](https://www.safaribooksonline.com/library/view/wix-36-a/9781782160427/ch01s02.html)。
### 修改名称等文字信息
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

### 修改要打包安装的文件
在34~38行定义了需要打包的文件，如果有外部依赖库需要添加也可以放在这里：
```
		<ComponentGroup Id="ProductComponents">
      <Component Id="ProductComponent" Directory="INSTALLFOLDER" Guid="84D3BAE6-2758-4FBD-9714-1E052A066830">
        <File Source="$(var.WpfAppToPackage.TargetPath)" Id="myapplication.exe" KeyPath="yes"></File>
      </Component>
		</ComponentGroup>
```
这里采用的方式引用要打包的WpfAppToPackage项目的输出目录，也可以采用相对路径的形式`Source="..\..\LibraFClient\LibraFClient\bin\Release\LibraFClient.exe"` 或者绝对路径也可以，但需要注意，采用相对路径时需要手动设置Guid的值。  
当你要打包多个文件时，要注意每一个`<Component>`最好只含有一个`<File>`，而且`<File>`的`KeyPath`的值最好为'yes'，因为`KeyPath`文件可以在丢失后使用“修复”功能重新得到，而一个`Component`只能有一个`KeyPath`文件。
### 添加自定义变量
当需要获取除安装路径/是否创建桌面快捷方式这两种用户安装时输入的信息，如用户名时，可以使用`<Property>`元素，它的`Id`属性唯一标识它，而`Value`属性则是它的值，他可以定义在Product.wxs中`<Product>`与`</Product>`之间任意的位置，通过`[IdName]`可以引用它的值。当然要获取用户输入的信息，还需要其他的设置：
* 在product.wxs中定义`Property`:`  <Property Id="USERNAME" Value=""></Property>`
* 在Bundle.wxs中引用msi包时，
```
<MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
        <MsiProperty Name="USERNAME" Value="[Username]"/>
      </MsiPackage>
```
* 在viewmodel中，调用`(BootstrapperApplication的实例).Engine.StringVariables[USERNAME] = Username;`
### 如何创建桌面与开始桌面快捷方式
[看文档后面的内容](#创建快捷方式)
## 编辑Bundle
在Bootstrapper项目中，Bundle.wxs用于定义要安装哪些依赖程序，以及如何验证这些依赖程序已经安装，并且定义安装界面的样式。这里只说明你打包自己的文件时需要修改的地方，元素与属性的详细说明请查看[文档末尾](#)或者[教程](https://www.safaribooksonline.com/library/view/wix-36-a/9781782160427/ch15.html)。
### 修改文字信息
和Product一样，我将所有文字信息都放在了zh-cn/zh-cn.wxl文件中，如软件名称，公司名，联系电话等等。引用方式和Product相同：`!(loc.IdName)`。
### 引用wpf类库
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
### 定义变量
`<?define VariableName=Value ?>`，在Bundle中定义一个名为VariableName值为Value的变量，可通过`$(var.VariableName)`进行引用。
### 安装多个msi或者exe文件
在`<Chain>`当中，通过`<ExePackage>`来指定要安装.exe文件，`<MsiPackage>`来指定要安装.msi文件，`SourceFile`属性则指定文件源路径，`DisplayInternalUI="no"`表示不显示该文件执行时显示的界面来实现静默安装，`Compressed="no"`表示不把这个.exe或者.msi压缩到最终生成的安装包中以减小最终文件的大小，`DownloadUrl="$(var.DoNetDownloadUrl)" `指定当安装时若从 `SourceFile`指定路径找不到该文件则从该网址中下载。不过在你build项目时需要该文件存在于你的`SourceFile`路径中。
```
<Chain  DisableRollback="yes">
      <MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup.msi" DisplayInternalUI="no">
      </MsiPackage>
      <MsiPackage SourceFile="..\SetupProject\bin\Release\zh-cn\DGSetup1.msi" DisplayInternalUI="no">
      </MsiPackage>
    </Chain>
```
### 获取用户输入信息并传递给Product
如[这里](#添加自定义变量)所说，要把变量从viewmdoel传给Product需要进行依次的传值。









# 如何从零开发
## 下载并安装WiX
从官网下载WiX安装包后进行安装即可，这是免费的。也可以使用Visual Studio的NuGet进行搜索安装。
## 创建解决方案
1. 打开Visual Studio（我使用的是VS2015），新建项目(New Project)—>其他项目类型(Other Project Types)—>Visual Studio解决方案(Visual Studio Solutions)，修改项目名称与存储位置（我这里命名为WiXPackage）。
2. 引用要打包的项目，在资源管理器中右击已经建好的空白解决方案，选择添加——>已存在的项目，在打开的对话框中找到要打包的项目添加进来（选择.csproj文件）。
3. 创建 **WiX Setup Project** ，在当前解决方案上右键添加——>新项目，如果已经正确安装WiX那么会在打开的项目模板列表左边看到 **Windows Installer XML** ,点击之后在右边选中 **Setup Project** ，按自己需要修改名称并确定。此时会看到解决方案资源管理器中看到一个有 **Product.wxs** 文件和 **References** 引用文件夹。在References右键—>添加引用—>项目，添加刚才引用的要打包的项目。
4. 创建 **Bootstrapper** 项目，在当前解决方案上右键添加——>新项目，同样找到 **Windows Installer XML** ,在右边选中 **Bootstrapper Project** ，按自己需要修改名称并确定。此时会看到解决方案资源管理器中看到一个有 **Bundle.wxs** 文件和 **References** 引用文件夹。
5. 创建C#类库，在当前解决方案上右键添加——>新项目，以此找到Visaul C#—>Class Library，命名为CustomBA。向References需要添加Bootstrapper程序集的引用，在WiX的安装路径（C:\Program Files (x86)\WiX Toolset v3.10\SDK）中找到BootstrapperCore.dll并添加引用。在SDK目录下还能看见名为 **BootstrapperCore.config** 的文件，将它复制粘贴到这个类库项目文件中，打开它并且将他的 **host** 元素的assemblyName属性值改为本类库名（CustomBA）。
## 向类库添加必要的引用
1. 为了要使用WPF来自定义界面，还需要向C#类库添加如下引用：
* PresentationCore
* PresentationFramework
* System.Xaml
* WindowsBase
2. 为了要使用MVVM模式，可以使用NuGet来向C#类库安装Prism，在VS菜单栏上依次点击工具—>NuGet Package—>管理NuGet解决方案，在浏览中搜索Prism并安装到CustomBA解决方案中。
3. 在C#类库中新建一个public类，名称为CustomBootstrapperApplication，暂时先不需要添加任何内容，而后在类库的Properties\AssemblyInfo.cs文件中添加如下代码，它指定Burn调用CustomBootstrapperApplication类的Run方法作为程序入口：
 ```
using CustomBA;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

[assembly: BootstrapperApplication(
   typeof(CustomBootstrapperApplication))]
```
4. 创建Views/ViewModels/Models文件夹，在C#类库项目中新建以上三个文件夹，依次存放MVVM模式中的Views/ViewModels/Models。
## 扩展BootstrapperApplication类
刚才在类库中新建了一个名为CustomBootstrapperApplication的类，现在我们对他进行修改。修改后的完整代码如下：
```
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace CustomBA
{
    public class CustomBootstrapperApplication : BootstrapperApplication
    {
        public static Dispatcher Dispatcher { get; set; }

        protected override void Run()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;

            var model = new BootstrapperApplicationModel(this);
            var viewModel = new InstallViewModel(model);
            var view = new InstallView(viewModel);

            model.SetWindowHandle(view);

            this.Engine.Detect();

            view.Show();
            Dispatcher.Run();
            this.Engine.Quit(model.FinalResult);
        }
    }
}
```
让CustomBootstrapperApplication类继承BootstrapperApplication类并重写他的Run方法，这便是程序的入口。 <br/>
在Run方法中，定义一个Dispatcher对象来惊醒UI线程与其他线程的通信，定义model/viewmodel和view来设置显示界面，SetWindowHandle方法是待会要在model类中要写的方法，它给Burn传递要显示的view，Engine.Detect()则用来检测这个bundle是否已经被安装，Detect需要在view/viewmodel和model实例化之后再被调用，因为它需要的一些配置信息需要被设置。Show()方法则显示图形界面，Dispatcher.Run()来启动线程，当安装完成或者取消时调用Dispatcher.InvokeShutdown（）可以结束线程。Engine.Quit（）无论安装结果如何都会关闭安装任务。
## 定义Model

## Product文件中元素与属性的简单说明
### Product元素
* Id: 该属性为GUID，可以使用Visual Studio->Tools->Create GUID的第4/5/6类来生成，也可以直接赋值为“\*”，该属性每一次build都需要不同的值
* Language：语言序号，1033为en-us，4为zh-cn
* Version：软件版本号，用于产品升级
* Manufacturer：制造商，公司名或这开发者名称，必填！
* UpgradeCode：GUID，产品识别与更新的标识
### Package元素
```
<Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" />
```
* InstallScope：值为"perMachine"表明将软件安装到当前机器，值为"perUser"则将软件安装到当前用户。
### MajorUpgrade元素
当产品更新时检验版本，若已安装高级版本则显示提示信息
### MediaTemplate元素
用于将打包的文件声明一个存储的磁盘文件区域
### Directory元素
```
<Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="INSTALLFOLDER"/>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="AppStartMenuFolder" Name="!(loc.ProductName)"/>
      </Directory>
      <Directory Id="DesktopFolder"></Directory>
    </Directory>
```
* INSTALLFOLDER:安装软件时选中的安装目录，ProgramFilesFolder为系统C盘的ProgrameFiels文件夹，因为要用户自定义安装位置，因此要把自动生成的`<Directory Id="ProgramFilesFolder">`这一层删除。
* Name:文件夹名称
* 桌面与开始菜单快捷方式的位置为：
```
  <Directory Id="ProgramMenuFolder">
     <Directory Id="AppStartMenuFolder" Name="!(loc.ProductName)"/>
  </Directory>
     <Directory Id="DesktopFolder"></Directory>
```
### Feature元素
`<Feature>`指定要执行打包的具体内容，内部包含若干`<Component>`，每一个`<Component>`定义一个行为或者文件。
### Component元素
`<Component>` ,`<ComponentGroup>` ,`<ComponentRef>`和`<ComponentGroupRef>`,Group当中可以包含多个Component，Ref通过Id与Component或者ComponentGroup关联，Ref元素声明要添加一个拥有某Id值的Component或者ComponentGroup，而其具体信息可以在另外的地方进行定义，如;
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
### File元素
### 创建快捷方式


## Bundle文件中元素与属性的简单说明

### WiX下载地址
http://wixtoolset.org/releases/ ，虽然已经提供v3.11版本，但当我已经安装.net framework 4.0后，v3.11依旧提示需要安装.net 3.0，所以我使用的是wix v3.10.3 。
### 参考教程
[WiX 3.6: A Developer's Guide to Windows Installer XML](https://www.safaribooksonline.com/library/view/wix-36-a/9781782160427/pr01.html)<br/>
[WiX Toolset Manual Table of Contents](http://wixtoolset.org/documentation/manual/v3/)
### 可能会有错误或者偏差的地方，欢迎交流指正！

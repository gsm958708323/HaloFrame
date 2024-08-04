# UI框架
## 功能测试
测试全屏界面和弹窗界面的遮挡关系：
- 界面打开顺序 FullView1 - TipsView1 - FullView2
- 依次关闭界面，查看界面生命周期

测试不同层级下的UI遮挡
- 界面打开顺序 FullView1 - TipsView1 - FullView2_Top
- 依次关闭界面

测试子界面
- 打开FullView1 - 打开子界面 ChildView1 - 打开子界面ChildView2 - 打开FullView2
- 依次关闭界面，查看生命周期

子界面独立打开：
- 界面配置关系
- 打开界面FullView1， 单独打开界面ChildView2

## UI层级管理
界面根据不同层级，划分到不同的UILayer中管理

UILayer使用CustomStack来管理全屏与弹窗界面

新界面如果是弹窗则不做处理，如果是全屏界面，则要关闭上一个界面


## UI生命周期
LoadAsset - Awake - Start - Enable - Disable - Destroy - ReleaseUI
不继承于MonoBehaviour，独立生命周期管理

UIView
 - UIGameView 游戏界面
 - UISubView 子界面

## 子界面
1、子界面理论上和普通界面一样，可以单独打开使用

2、子界面也可以继续创建子界面

3、界面之间通过ChildList建立父子关系

## 框架设计思路
Q：为什么需要区分弹窗与全屏？
A：为了让被全屏覆盖的UI可以隐藏，减少drawcall和运行时的消耗。

Q：每个界面为什么要拆分单独的Canvas
A：为了方便管理，每个界面都单独管理自己的Canvas，方便管理UI的层级关系，某个界面元素变更时不会影响其他界面元素。

Q：为什么不直接使用SetActive来显示隐藏界面
A：减少网格重建带来的消耗，每个canvas下的UI元素变动只会影响当前界面

Q：为什么同一个界面打开多次，也会创建多个预制体
A：界面打开顺序：1(全屏) 2 3(全屏) 4，界面回退时，为了解决这种情况下界面可以按栈中顺序返回


## 待优化
- 对栈中相同界面的预制体做堆叠限制
- 同一帧连续压入多个弹窗，如何保证顺序，先打开的先显示？

## 后续计划
- UI Item生命周期管理
- UI代码自动生成
- UI组件自动绑定
- UI动效管理
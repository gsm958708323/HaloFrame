# UI框架
## UI层级管理
界面根据不同层级，划分到不同的UILayer中管理

UILayer使用CustomStack来管理全屏与弹窗界面

新界面如果是弹窗则不做处理，如果是全屏界面，则要关闭上一个界面


## UI生命周期
LoadAsset - Awake - Start - Enable - Disable - Destroy - ReleaseUI

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

---
# 事件系统

## 测试用例：
- 重复监听
- 事件A触发，回调方法中移除了事件A的监听
- 事件A触发，回调方法中移除了事件B的监听
- 连续重复触发多个事件id，能否保证回调的顺序正确性，即先触发的事件先回调。
- 值类型装箱
- 事件触发时，监听对象被销毁了

## 在遍历回调方法列表时，执行回调方法，可能导致回调方法列表被修改？
在移除监听时，判断当前事件id是否正在执行中，如果是则把EventInfo中IsRelease设置为true，等遍历完毕之后再统一回收并移除

## 关注内容
- 实现事件系统的时候如果用泛型委托的要注意了，委托在类型转化的时候会产生GC
- 不能清楚知道事件名对应几个变量，需要自己定义的时候留意一下

![输入图片说明](Assets\HaloFrame\Samples\Event\EventTips1.png)
![输入图片说明](Assets\HaloFrame\Samples\Event\EventTips2.png)

# 红点系统
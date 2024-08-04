/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/20 22:26:39
** dsec:  UISubView 
UI子界面基类继承UIGameView的原因：
1、子界面理论上和普通界面一样，可以单独打开使用
2、子界面也可以继续创建子界面
3、界面之间通过ChildList建立父子关系

子界面资源可以动态加载，也可以通过查找节点作为界面资源
*******************************************************/
namespace HaloFrame
{
    public class UISubView : UIGameView
    {
        public UIGameView Parent;
        public ResType ResType = ResType.Dynamic;

        protected override void CloseSelf()
        {
            if (Parent != null)
            {
                Parent.CloseChild(GetType());
            }
            else
            {
                GameManager.UI.Close(GetType());
            }
        }
    }
}

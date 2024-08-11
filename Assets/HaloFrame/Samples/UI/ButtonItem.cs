using HaloFrame;
using UnityEngine.UI;

public class ButtonItem : UIItem
{
    protected override void OnStart(object[] args)
    {
        base.OnStart(args);

        int type = (int)args[0];
        gameObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (type == 0)
            {
                Parent.OpenOneChildUI<ChildView1>();
            }
            else if (type == 1)
            {
                Parent.OpenOneChildUI<ChildView2>();
            }
            else if (type == 2)
            {
                Parent.OpenOneChildUI<ChildView3>();
            }
            else if (type == 3)
            {
                Parent.OpenOneChildUI<ChildView4>();
            }
        });

        gameObject.GetComponentInChildren<Text>().text =  $"子界面{type + 1}";
    }
}

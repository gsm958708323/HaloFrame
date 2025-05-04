using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class RedDotNode
    {
        string fullPath;
        public string Name
        {
            get;
            private set;
        }

        public string FullPath
        {
            get
            {
                // test
                if (string.IsNullOrEmpty(fullPath))
                {
                    if (Parent == null || Parent == GameManager.RedDot.Root)
                    {
                        fullPath = Name;
                    }
                    else
                    {
                        fullPath = Parent.FullPath + GameManager.RedDot.SplitChar + Name;
                    }
                }
                return fullPath;
            }
        }

        public override string ToString()
        {
            return FullPath;
        }

        /// <summary>
        /// 红点数量
        /// </summary>
        public int Value { get; private set; }
        /// <summary>
        /// 父节点
        /// </summary>
        public RedDotNode Parent { get; private set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public Dictionary<RangeString, RedDotNode> Childrens;
        public int ChildrenCount
        {
            get
            {
                if (Childrens == null)
                {
                    return 0;
                }

                int sum = Childrens.Count;
                foreach (var item in Childrens.Values)
                {
                    sum += item.ChildrenCount;
                }
                return sum;
            }
        }

        public RedDotNode(string name)
        {
            Name = name;
            Value = 0;
            changeCB = null;
        }

        public RedDotNode(string name, RedDotNode parent) : this(name)
        {
            Parent = parent;
        }

        Action<int> changeCB;
        public void AddListener(Action<int> cb)
        {
            changeCB += cb;
        }

        public void RemoveListener(Action<int> cb)
        {
            changeCB -= cb;
        }

        public void RemoveAllListeners()
        {
            changeCB = null;
        }

        #region 红点逻辑

        /// <summary>
        /// 改变自身红点数量
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(int value)
        {
            if (Childrens != null && Childrens.Count != 0)
            {
                throw new Exception("不允许直接改变非叶子节点的值：" + FullPath);
            }

            InternalChangeValue(value);
        }

        public void AddValue(int newValue)
        {
            SetValue(Value + newValue);
        }

        /// <summary>
        /// 根据子节点的值计算新值，将下层所有子节点的值汇总
        /// </summary>
        public void UpdateValue()
        {
            // todo 对比ChildrenCount
            int sum = 0;
            if (Childrens != null && Childrens.Count != 0)
            {
                foreach (var item in Childrens)
                {
                    sum += item.Value.Value;
                }
            }

            InternalChangeValue(sum);
        }

        private void InternalChangeValue(int value)
        {
            if (value == Value)
            {
                return;
            }
            value = Math.Max(0, value); // 值不允许小于0

            Value = value;
            changeCB?.Invoke(value);
            GameManager.RedDot.NodeValueChangeCallback?.Invoke(this, Value);
            //标记父节点为脏节点
            GameManager.RedDot.MarkDirtyNode(Parent);
        }

        public RedDotNode AddChild(RangeString key)
        {
            if (Childrens == null)
            {
                Childrens = new Dictionary<RangeString, RedDotNode>();
            }
            if (Childrens.ContainsKey(key))
            {
                throw new Exception("节点已存在：" + key.ToString());
            }

            RedDotNode child = new RedDotNode(key.ToString(), this);
            Childrens.Add(key, child);
            GameManager.RedDot.NodeNumChangeCallback?.Invoke();
            return child;
        }

        public bool RemoveChild(RangeString key)
        {
            if (Childrens == null || Childrens.Count == 0)
            {
                return false;
            }

            RedDotNode child = GetChild(key);
            if (child != null)
            {
                // todo 先移除再刷新？
                GameManager.RedDot.MarkDirtyNode(this);
                Childrens.Remove(key);

                GameManager.RedDot.NodeNumChangeCallback?.Invoke();
                return true;

            }
            return false;
        }

        public void RemoveAllChild()
        {
            if (Childrens == null || Childrens.Count == 0)
            {
                return;
            }

            Childrens.Clear();

            GameManager.RedDot.MarkDirtyNode(this);
            GameManager.RedDot.NodeNumChangeCallback?.Invoke();
        }

        private RedDotNode GetChild(RangeString key)
        {
            if (Childrens == null)
                return null;

            Childrens.TryGetValue(key, out RedDotNode child);
            return child;
        }

        public RedDotNode GetOrAddChild(RangeString key)
        {
            var child = GetChild(key);
            if (child == null)
            {
                child = AddChild(key);
            }
            return child;
        }

        #endregion
    }
}

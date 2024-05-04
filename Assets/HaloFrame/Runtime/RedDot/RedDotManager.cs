using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HaloFrame
{
    public class RedDotManager : IManager
    {
        public RedDotNode Root { get; internal set; }
        public char SplitChar { get; internal set; }
        public Action<RedDotNode, int> NodeValueChangeCallback;
        public Action NodeNumChangeCallback;

        private Dictionary<string, RedDotNode> nodeDict;
        List<RedDotNode> dirtyList;
        List<RedDotNode> tempDirtyList;

        public StringBuilder CacheString;

        public RedDotManager()
        {
            SplitChar = '.';
            nodeDict = new Dictionary<string, RedDotNode>();
            Root = new RedDotNode("Root");
            dirtyList = new List<RedDotNode>();
            tempDirtyList = new List<RedDotNode>();
            CacheString = new StringBuilder();
        }

        public RedDotNode AddListener(string path, Action<int> cb)
        {
            if (cb == null)
                return null;

            RedDotNode node = GetRedNode(path);
            node.AddListener(cb);
            return node;
        }
        public void RemoveListener(string path, Action<int> cb)
        {
            if (cb == null)
                return;

            RedDotNode node = GetRedNode(path);
            node.RemoveListener(cb);
        }

        public void RemoveAllListeners(string path)
        {
            RedDotNode node = GetRedNode(path);
            node.RemoveAllListeners();
        }

        public void SetValue(string path, int value)
        {
            RedDotNode node = GetRedNode(path);
            node.SetValue(value);
        }

        public void AddValue(string path, int add)
        {
            RedDotNode node = GetRedNode(path);
            node.AddValue(add);
        }

        public int GetRedValue(string path)
        {
            RedDotNode node = GetRedNode(path);
            if (node == null)
            {
                return 0;
            }

            return node.Value;
        }

        private RedDotNode GetRedNode(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("路径不合法，不能为空");

            // 已经找到的放到字典中
            if (nodeDict.TryGetValue(path, out RedDotNode node))
            {
                return node;
            }

            RedDotNode cur = Root;
            int length = path.Length;
            int startIndex = 0;
            for (int i = 0; i < length; i++)
            {
                if (path[i] != SplitChar)
                    continue;

                // 遇到分隔符，准备划分父子节点
                if (i == length - 1)
                {
                    throw new Exception("路径不合法，不能以路径分隔符结尾：" + path);
                }
                int endIndex = i - 1;
                if (endIndex < startIndex)
                {
                    throw new Exception("路径不合法，不能存在连续的路径分隔符或以路径分隔符开头：" + path);
                }

                var child = cur.GetOrAddChild(new RangeString(path, startIndex, endIndex));
                cur = child;
                startIndex = i + 1; // 更新起始索引，继续查找下一个子节点
            }


            /*
            1.2.3 node 最后一个节点，存放完整路径
            1.2 node
            1 node
            */
            var lastNode = cur.GetOrAddChild(new RangeString(path, startIndex, length - 1)); // 创建最后一个子节点，并返回
            nodeDict.Add(path, lastNode); // 添加到字典中，方便下次查找
            return lastNode;
        }

        public bool RemoveRedNode(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("路径不合法，不能为空");

            if (!nodeDict.ContainsKey(path))
            {
                return false;
            }

            var node = GetRedNode(path);
            nodeDict.Remove(path);
            return node.Parent.RemoveChild(new RangeString(node.Name, 0, node.Name.Length - 1));
        }

        public void RemoveAllRedNode()
        {
            Root.RemoveAllChild();
            nodeDict.Clear();
        }

        /// <summary>
        /// 子节点发生改变，需要通知父节点刷新
        /// </summary>
        /// <param name="parent"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void MarkDirtyNode(RedDotNode node)
        {
            //根结点不能被标记为脏节点
            if (node == null || node.Name == Root.Name)
            {
                return;
            }

            dirtyList.Add(node);
        }

        public override void Tick(float deltaTime)
        {
            if (dirtyList.Count == 0)
                return;

            //复制一份，避免在遍历过程中修改集合
            tempDirtyList.Clear();
            foreach (var item in dirtyList)
            {
                tempDirtyList.Add(item);
            }
            dirtyList.Clear();

            foreach (var item in tempDirtyList)
            {
                item.UpdateValue();
            }
        }
    }
}

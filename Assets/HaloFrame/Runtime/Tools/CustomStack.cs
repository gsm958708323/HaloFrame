using System.Collections.Generic;

namespace HaloFrame
{
    /// <summary>
    /// 自定义栈结构，支持从栈中移除数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomStack<T>
    {
        private readonly List<T> m_List = new List<T>();

        public int Count
        {
            get { return m_List.Count; }
        }

        public T Peek()
        {
            return m_List.Count > 0 ? m_List[m_List.Count - 1] : default(T);
        }

        public void Push(T value)
        {
            m_List.Add(value);
        }

        public T Pop()
        {
            T pop = default(T);
            if (m_List.Count > 0)
            {
                pop = m_List[m_List.Count - 1];
                m_List.RemoveAt(m_List.Count - 1);
            }
            return pop;
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public List<T> GetList()
        {
            return m_List;
        }

        public bool Contains(T value)
        {
            return m_List.Contains(value);
        }

        public void Remove(T value)
        {
            for (int i = m_List.Count - 1; i >= 0; i--)
            {
                T temp = m_List[i];
                if (temp.Equals(value))
                    m_List.RemoveAt(i);
            }
        }

        public void RemoveOne(T value)
        {
            for (int i = m_List.Count - 1; i >= 0; i--)
            {
                T temp = m_List[i];
                if (temp.Equals(value))
                {
                    m_List.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

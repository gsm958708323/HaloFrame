using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public abstract class ABundleAsync : ABundle
    {
        /// <summary>
        /// 检查是否加载完成
        /// </summary>
        /// <returns></returns>
        public abstract bool Tick();
    }
}


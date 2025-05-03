using System;

namespace HaloFrame
{
    public abstract class AResourceAsync : AResource
    {
        /// <summary>
        /// 资源是否加载完成
        /// </summary>
        /// <returns></returns>
        public abstract bool Tick();

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public abstract void LoadAssetAsync();
    }
}

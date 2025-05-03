using System;

namespace HaloFrame
{
    public class Resource : AResource
    {
        public override bool keepWaiting => throw new NotImplementedException();

        public override T GetAsset<T>()
        {
            throw new NotImplementedException();
        }

        public override void Load()
        {
            throw new NotImplementedException();
        }

        public override void LoadAsset()
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }
    }
}

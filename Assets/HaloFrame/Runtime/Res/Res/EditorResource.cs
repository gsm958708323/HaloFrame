using System;

namespace HaloFrame
{
    public class EditorResource : AResource
    {
        public override bool keepWaiting => !done;

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

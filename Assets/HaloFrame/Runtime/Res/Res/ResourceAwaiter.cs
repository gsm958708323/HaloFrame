using System;

namespace HaloFrame
{
    public class ResourceAwaiter : IAwaiter<IResource>, IAwaitable<ResourceAwaiter, IResource>
    {
        public bool IsCompleted => throw new NotImplementedException();

        public ResourceAwaiter GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public IResource GetResult()
        {
            throw new NotImplementedException();
        }

        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        internal void SetResult(IResource resource)
        {
            throw new NotImplementedException();
        }
    }
}

using System;

namespace HaloFrame
{
    public class ResourceAwaiter : IAwaiter<IResource>, IAwaitable<ResourceAwaiter, IResource>
    {
        public bool IsCompleted { get; private set; }
        public IResource result { get; private set; }
        private Action finishCB;

        public ResourceAwaiter GetAwaiter()
        {
            return this;
        }

        public IResource GetResult()
        {
            return result;
        }

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                finishCB?.Invoke();
            }
            else
            {
                finishCB += continuation;
            }
        }

        internal void SetResult(IResource resource)
        {
            IsCompleted = true;
            result = resource;
            finishCB?.Invoke();
            finishCB = null;
        }
    }
}

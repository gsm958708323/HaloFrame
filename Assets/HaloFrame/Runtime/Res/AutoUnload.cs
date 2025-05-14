using System;
using UnityEngine;

namespace HaloFrame
{
    public class AutoUnload : MonoBehaviour
    {
        internal AResource resource;

        void OnDestroy()
        {
            if (resource is null)
                return;

            GameManager.Resource.Unload(resource);
            resource = null;
        }
    }
}

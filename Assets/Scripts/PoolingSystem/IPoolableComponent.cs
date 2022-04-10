using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoolingSystem
{
    public interface IPoolableComponent
    {
        void Spawned();
        void Despawned();
    }
}

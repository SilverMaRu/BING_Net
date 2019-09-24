using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolWater
{
    ObjectPool ownerPool { get; set; }
    void Work();
    void Rest();
    void GoBack();
}

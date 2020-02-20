using UnityEngine.Events;

namespace CustomEvents {

    [System.Serializable]
    public class CUnityEventBool : UnityEvent<bool> { }

    [System.Serializable]
    public class CUnityEventDecimal : UnityEvent<decimal> { }

    [System.Serializable]
    public class CUnityEventFloat : UnityEvent<float> { }

    [System.Serializable]
    public class CUnityEventDouble : UnityEvent<double> { }

    [System.Serializable]
    public class CUnityEventInt : UnityEvent<int> { }

    [System.Serializable]
    public class CUnityEventTileColoringMode : UnityEvent<TileColoringMode> { }
}

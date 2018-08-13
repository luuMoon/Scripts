using UnityEngine;

[CreateAssetMenu(menuName = "CameraEffect/CameraShakeParam")]
[System.Serializable]
public class CameraShakeParam : ScriptableObject
{
    [System.Serializable]
    public class shakeParam
    {
        public float amplitudeX;
        public float amplitudeY;
        public float duration;
        public float delayTime;
    }
    public shakeParam[] shakeParams;
}



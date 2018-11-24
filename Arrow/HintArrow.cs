using System;
using System.Collections;
using System.Collections.Generic;
using FrameWork;
using UnityEngine;

public enum ArrowType
{
    NONE = -1,
    DEFAULT = 0, //默认
    DEAD = 1, //死亡尸体
    COUNT = 2,
}

public class HintArrow : MonoBehaviour
{
    readonly Rect _screenRect = new Rect(0, 0, 1, 1);
    readonly Vector2 _centerPos = new Vector2(0.5f,0.5f);
    [SerializeField] private Vector2 _xClamp = new Vector2(0.05f,0.95f);
    [SerializeField] private Vector2 _yClamp = new Vector2(0.05f,0.95f);
    [SerializeField] private GameObject _template;
    [SerializeField] private List<GameObject> arrowTemplate;

    Dictionary<int,HintArrowTarget> _arrowHintDic = new Dictionary<int, HintArrowTarget>();
    List<int> deleteCache = new List<int>();

    public GameObject GetTemplateObj(ArrowType type)
    {
        if(null != arrowTemplate && arrowTemplate.Count > (int)type)
        {
            return arrowTemplate[(int)type];
        }
        return _template;
    }

    public void AddHintArrow(Transform target,ArrowType type = ArrowType.DEFAULT)
    {
        _arrowHintDic[target.GetInstanceID()] = new HintArrowTarget(target, GetTemplateObj(type));
    }

    public void RemoveHintArrow(Transform target)
    {
        var instanceId = target.GetInstanceID();
        if (!deleteCache.Contains(instanceId))
        {
            deleteCache.Add(instanceId);    
        }
    }
    
    void PositionHint(HintArrowTarget hintArrowTarget)
    {
        if (hintArrowTarget.Target == null || Global.Instance.MainCamera == null)
        {
            return;
        } 
        
        var viewPos = Global.Instance.MainCamera.WorldToViewportPoint(hintArrowTarget.Target.position);
        if (viewPos.z < 0.01f)
        {
            viewPos = -viewPos;
        }
        var vec2Pos = new Vector2(viewPos.x,viewPos.y);
        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        if (_screenRect.Contains(vec2Pos))
        {
            hintArrowTarget.Hint.SetActive(false);
            return;
        }
        hintArrowTarget.Hint.SetActive(true);
        
        viewPos.x -= 0.5f;
        viewPos.y -= 0.5f;
        viewPos.z = 0;
        
        var fAngle = Mathf.Atan2(viewPos.x, viewPos.y);
        viewPos.x = Mathf.Lerp(_xClamp.x,_xClamp.y, Mathf.InverseLerp(-1, 1, Mathf.Sin(fAngle)));
        viewPos.y = Mathf.Lerp(_yClamp.x,_yClamp.y, Mathf.InverseLerp(-1, 1, Mathf.Cos(fAngle)));
        viewPos.z = Global.Instance.MainCamera.nearClipPlane + 0.01f;
        
        var screenPos = Global.Instance.MainCamera.ViewportToScreenPoint(viewPos);
        var cameraPos = Global.Instance.UiCamera.ScreenToWorldPoint(screenPos);
        var dir = (vec2Pos - _centerPos).normalized;
        hintArrowTarget.Hint.transform.forward = dir;
        var rectTrans = hintArrowTarget.Hint.transform.GetComponent<RectTransform>();
        rectTrans.position = cameraPos;
        var tempLocPos = rectTrans.localPosition;
        tempLocPos.z = 0;
        rectTrans.localPosition = tempLocPos;
    }

    private void Update()
    {
        if (_arrowHintDic.Count == 0)
        {
            return;
        }
        foreach (var kv in _arrowHintDic)
        {
            PositionHint(kv.Value);
        }
    }

    private void LateUpdate()
    {
        foreach (var v in deleteCache)
        {
            if (_arrowHintDic.ContainsKey(v))
            {
                _arrowHintDic[v].Destroy();
                _arrowHintDic.Remove(v);
            }
        }
        deleteCache.Clear();
    }

    class HintArrowTarget
    {
        public GameObject Hint { private set; get; }
        public Transform Target { private set; get; }
        public HintArrowTarget(Transform target,GameObject template)
        {
            Hint = CachedClone.Clone(template);
            Target = target;
        }

        public void Destroy()
        {
            Debug.Log("destroy");
            CachedClone.RemoveClone(Hint);
        }
    }
}
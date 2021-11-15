using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCorners : MonoBehaviour
{
    public GameObject mObjUL = null;
    public GameObject mObjDR = null;

    publicÅ@float GetScreenWidth()
    {
        return mObjDR.transform.localPosition.x- mObjUL.transform.localPosition.x;
    }
    public float GetScreenHeight()
    {
        return mObjUL.transform.localPosition.y - mObjDR.transform.localPosition.y;
    }
}

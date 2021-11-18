using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.UI;

public class PanelMoveManager : MonoBehaviour
{
    public CanvasCorners mCorner = null;
    public ScreenTransformGesture mGesture =null;
    public RectTransform mTrnsPanel = null;
    public RectTransform mTrnsFlickArea =null;
    public eLocation mOutLocate = eLocation.Up;//スライド表示前は四隅のどこに配置するか
    public bool mIsDispNow = false;//スライド表示済みフラグ
    public float[] mSlideChangeValue = new float[2] { 200, 400 };//スライドした値がこの値を超えると表示を切り替える※[0]X, [1]Y
    public GameObject[] mObjFlickImage = new GameObject[2];//[0]スライド前の表示、[1]スライド後の表示

    Vector2 mStartPosition;
    Vector2 mSlideVal;
    bool mIsPress;
    bool mIsAnimation;

    /// <summary>
    /// スライドが非表示の時にどの位置にいるか
    /// </summary>
    public enum eLocation
    {
        Up=0,
        Down,
        Left,
        Right,
    };

    /// <summary>
    /// 本当はStartじゃなくてInit関数にして外部から初期化する
    /// </summary>
    void Start()
    {
        mIsAnimation = false;
        //スライド開始イベント
        mGesture.TransformStarted += new EventHandler<EventArgs>((sender, e) =>
        {           
            if (mIsAnimation) { return; }
            var gesture = sender as ScreenTransformGesture;
            mStartPosition = gesture.ScreenPosition;
            mSlideVal = new Vector2(0, 0);
            mIsPress = true;
        });
        //スライド終了イベント
        EventHandler<EventArgs> evEnd= new EventHandler<EventArgs>((sender, e) =>
        {
            if (mIsAnimation) { return; }
            mIsPress = false;
            //タップ離したときに移動値に応じてパネル移動
            float animeTime = 0.1f;
            if(IsOkDispChange())
            {
                mIsDispNow = !mIsDispNow;
                animeTime = 0.5f;
            }
            MovePanel(mIsDispNow, true,animeTime);
        });
        mGesture.TransformCompleted += evEnd;
        mGesture.Cancelled += evEnd;
        //スライド中イベント
        mGesture.StateChanged += new EventHandler<GestureStateChangeEventArgs>((sender, e) =>
        {
            if (mIsAnimation) { return; }
            if (!mIsPress) { return; }
            var gesture = sender as ScreenTransformGesture;
            Vector2 moveVector = gesture.ScreenPosition - mStartPosition;
            mStartPosition = gesture.ScreenPosition;
            mSlideVal += moveVector;
            SlidePanel(moveVector);
        });
        //スライドを初期位置に移動させる
        MovePanel(mIsDispNow, false);
    }

    /// <summary>
    /// Fixの位置にパネルを移動させる
    /// </summary>
    /// <param name="IsDisp">表示ON/OFF</param>
    /// <param name="IsSetAnimation">移動時にアニメーションするか</param>
    /// <param name="AnimeTime">アニメする場合の移動時間</param>
    private void MovePanel(bool IsDisp,bool IsSetAnimation=false,float AnimeTime=0)
    {
        //切り替え後の位置設定
        Vector3 setPosi = new Vector3(0, 0,0);
        if (!IsDisp)
        {
            switch (mOutLocate)
            {
                case eLocation.Up:
                    setPosi.x = mTrnsPanel.localPosition.x;
                    setPosi.y = (mCorner.GetScreenHeight() + mTrnsPanel.sizeDelta.y)/2- mTrnsFlickArea.sizeDelta.y;
                    break;
                case eLocation.Down:
                    setPosi.x = mTrnsPanel.localPosition.x;
                    setPosi.y = -1*(mCorner.GetScreenHeight() + mTrnsPanel.sizeDelta.y) / 2 + mTrnsFlickArea.sizeDelta.y;
                    break;
                case eLocation.Right:
                    setPosi.x = (mCorner.GetScreenWidth() + mTrnsPanel.sizeDelta.x) / 2 - mTrnsFlickArea.sizeDelta.x;
                    setPosi.y = mTrnsPanel.localPosition.y;
                    break;
                case eLocation.Left:
                    setPosi.x = -1 * (mCorner.GetScreenWidth() + mTrnsPanel.sizeDelta.x) / 2 + mTrnsFlickArea.sizeDelta.x;
                    setPosi.y = mTrnsPanel.localPosition.y;
                    break;
            }
        }
        else
        {
            switch (mOutLocate)
            {
                case eLocation.Up:
                case eLocation.Down:
                    setPosi.x = mTrnsPanel.localPosition.x;
                    setPosi.y = 0;
                    break;
                case eLocation.Right:
                case eLocation.Left:
                    setPosi.x = 0;
                    setPosi.y = mTrnsPanel.localPosition.y;
                    break;
            }
        }
        //スライド表示前エリアの画像切り替え
        mObjFlickImage[0]?.SetActive(!IsDisp);
        mObjFlickImage[1]?.SetActive(IsDisp);
        //アニメーションつけて移動するか判定
        if (!IsSetAnimation)
        {
            mTrnsPanel.localPosition=setPosi;
        }
        else
        {
            Sequence SequenceSerifAnime = DOTween.Sequence()
            .OnStart(() =>
            {
                mIsPress = false;
                mIsAnimation = true;
            })
            .Append(mTrnsPanel.DOAnchorPos(setPosi, AnimeTime).SetEase(Ease.Unset))
            .AppendCallback(() =>
            {
                mIsAnimation = false;
                mIsPress = false;
            })
            .SetLink(gameObject);
        }
    }
    /// <summary>
    /// パネルスライド
    /// </summary>
    /// <param name="MoveVector">移動する値</param>
    private void SlidePanel(Vector2 MoveVector)
    {
        //切り替え後の位置設定
        Vector3 setPosi = new Vector3(0, 0, 0);
        switch (mOutLocate)
        {
            case eLocation.Up:
            case eLocation.Down:
                setPosi.x = mTrnsPanel.localPosition.x;
                setPosi.y = mTrnsPanel.localPosition.y + MoveVector.y;
                break;
            case eLocation.Right:
            case eLocation.Left:
                setPosi.x = mTrnsPanel.localPosition.x+ MoveVector.x;
                setPosi.y = mTrnsPanel.localPosition.y;
                break;
        }
        mTrnsPanel.localPosition = setPosi;
    }
    /// <summary>
    /// パネル表示ON/OFF切り替え許可判定
    /// </summary>
    /// <returns></returns>
    private bool IsOkDispChange()
    {
        bool isPanelDisp = false;
        //累積スライド値を正の値に変換
        mSlideVal.x = Math.Abs(mSlideVal.x);
        mSlideVal.y = Math.Abs(mSlideVal.y);
        //累積スライド値が範囲を超えた名合、表示切り替える
        if (mOutLocate == eLocation.Left || mOutLocate == eLocation.Right)
        {
            if (mSlideVal.x > mSlideChangeValue[0])
            {
                isPanelDisp = true;
            }
        }
        else if (mOutLocate == eLocation.Up || mOutLocate == eLocation.Down)
        {
            if (mSlideVal.y > mSlideChangeValue[1])
            {
                isPanelDisp = true;
            }
        }
        else { }
        return isPanelDisp;
    }
}

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
    public eLocation mOutLocate = eLocation.Up;//�X���C�h�\���O�͎l���̂ǂ��ɔz�u���邩
    public bool mIsDispNow = false;//�X���C�h�\���ς݃t���O
    public float[] mSlideChangeValue = new float[2] { 200, 400 };//�X���C�h�����l�����̒l�𒴂���ƕ\����؂�ւ��遦[0]X, [1]Y
    public GameObject[] mObjFlickImage = new GameObject[2];//[0]�X���C�h�O�̕\���A[1]�X���C�h��̕\��

    Vector2 mStartPosition;
    Vector2 mSlideVal;
    bool mIsPress;
    bool mIsAnimation;

    /// <summary>
    /// �X���C�h����\���̎��ɂǂ̈ʒu�ɂ��邩
    /// </summary>
    public enum eLocation
    {
        Up=0,
        Down,
        Left,
        Right,
    };

    /// <summary>
    /// �{����Start����Ȃ���Init�֐��ɂ��ĊO�����珉��������
    /// </summary>
    void Start()
    {
        mIsAnimation = false;
        //�X���C�h�J�n�C�x���g
        mGesture.TransformStarted += new EventHandler<EventArgs>((sender, e) =>
        {           
            if (mIsAnimation) { return; }
            var gesture = sender as ScreenTransformGesture;
            mStartPosition = gesture.ScreenPosition;
            mSlideVal = new Vector2(0, 0);
            mIsPress = true;
        });
        //�X���C�h�I���C�x���g
        EventHandler<EventArgs> evEnd= new EventHandler<EventArgs>((sender, e) =>
        {
            if (mIsAnimation) { return; }
            mIsPress = false;
            //�^�b�v�������Ƃ��Ɉړ��l�ɉ����ăp�l���ړ�
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
        //�X���C�h���C�x���g
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
        //�X���C�h�������ʒu�Ɉړ�������
        MovePanel(mIsDispNow, false);
    }

    /// <summary>
    /// Fix�̈ʒu�Ƀp�l�����ړ�������
    /// </summary>
    /// <param name="IsDisp">�\��ON/OFF</param>
    /// <param name="IsSetAnimation">�ړ����ɃA�j���[�V�������邩</param>
    /// <param name="AnimeTime">�A�j������ꍇ�̈ړ�����</param>
    private void MovePanel(bool IsDisp,bool IsSetAnimation=false,float AnimeTime=0)
    {
        //�؂�ւ���̈ʒu�ݒ�
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
        //�X���C�h�\���O�G���A�̉摜�؂�ւ�
        mObjFlickImage[0]?.SetActive(!IsDisp);
        mObjFlickImage[1]?.SetActive(IsDisp);
        //�A�j���[�V�������Ĉړ����邩����
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
    /// �p�l���X���C�h
    /// </summary>
    /// <param name="MoveVector">�ړ�����l</param>
    private void SlidePanel(Vector2 MoveVector)
    {
        //�؂�ւ���̈ʒu�ݒ�
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
    /// �p�l���\��ON/OFF�؂�ւ�������
    /// </summary>
    /// <returns></returns>
    private bool IsOkDispChange()
    {
        bool isPanelDisp = false;
        //�ݐσX���C�h�l�𐳂̒l�ɕϊ�
        mSlideVal.x = Math.Abs(mSlideVal.x);
        mSlideVal.y = Math.Abs(mSlideVal.y);
        //�ݐσX���C�h�l���͈͂𒴂��������A�\���؂�ւ���
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

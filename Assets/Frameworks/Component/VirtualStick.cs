using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using Framework.Noti;
using Framework.Utils;

namespace UnityEngine.UI
{
    public class VirtualStick : MonoBehaviour
    {
        public RectTransform rectTrans;
        public RectTransform bigCircle;
        public RectTransform smallCircle;
        public bool autoHide;

        private float _fieldOffsetX;
        private float _fieldOffsetY;
        private Vector2 _origBigCirclePos;
        private float _bigRadius;
        private float _smallRadius;
        private float _diffRadius;

        private bool _dragging;
        public Vector2 pushDir;
        public Vector3 pushDirV3
        {
            get { return new Vector3(pushDir.x, 0f, pushDir.y); }
        }
        public float pushScale;
        public bool hasInput
        {
            get { return pushScale > 0; }
        }

        void Awake()
        {
            _fieldOffsetX = rectTrans.rect.width * 0.5f;
            _fieldOffsetY = rectTrans.rect.height * 0.5f;
            _origBigCirclePos = bigCircle.anchoredPosition;
            bigCircle.gameObject.SetActive(!autoHide);

            _bigRadius = bigCircle.sizeDelta.x * 0.5f;
            _smallRadius = smallCircle.sizeDelta.x * 0.5f;
            _diffRadius = _bigRadius - _smallRadius;
        }

        void OnEnable()
        {
            NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_DOWN, OnPointerDown);
            NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_UP_IGNORE_UI, OnPointerUp);
            NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_MOVE, OnPointerMove);
        }

        void OnDisable()
        {
            NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_DOWN, OnPointerDown);
            NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_UP_IGNORE_UI, OnPointerUp);
            NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_MOVE, OnPointerMove);
        }

        public void OnPointerDown(INotification noti)
        {
            Notification<int, Vector2> tNoti = (Notification<int, Vector2>)noti;
            if (tNoti.data1 == -1 && rectTrans.rect.Contains(tNoti.data2 - new Vector2(_fieldOffsetX, _fieldOffsetY)))
            {
                bigCircle.anchoredPosition = tNoti.data2 - new Vector2(_fieldOffsetX, 0f);
                bigCircle.gameObject.SetActive(true);
                _dragging = true;
            }
        }

        public void OnPointerUp(INotification noti)
        {
            Notification<int, Vector2> tNoti = (Notification<int, Vector2>)noti;
            if (tNoti.data1 == -1 && _dragging)
            {
                bigCircle.anchoredPosition = _origBigCirclePos;
                bigCircle.gameObject.SetActive(!autoHide);
                pushDir = Vector2.zero;
                pushScale = 0f;
                smallCircle.anchoredPosition = Vector2.zero;
                _dragging = false;
                NotiCenter.DispatchStatic("NOTI_VIRTUAL_STICK_RELEASE");
            }
        }

        public void OnPointerMove(INotification noti)
        {
            Notification<int, Vector2, Vector2> tNoti = (Notification<int, Vector2, Vector2>)noti;
            if (tNoti.data1 == -1 && _dragging)
            {
                Vector2 vec = tNoti.data2 - new Vector2(_fieldOffsetX, 0f) - bigCircle.anchoredPosition;
                pushDir = vec.normalized;
                pushScale = Mathf.Clamp01(vec.sqrMagnitude / (_diffRadius * _diffRadius));
                smallCircle.anchoredPosition = pushDir * _diffRadius * pushScale;
                NotiCenter.DispatchStatic<Vector2, float>("NOTI_VIRTUAL_STICK_PUSH", pushDir, pushScale);
            }
        }
    }
}

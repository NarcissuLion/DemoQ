using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Framework.Noti;

namespace Framework.Utils
{
    public class ScreenToucher : MonoBehaviour
    {
        public static string NOTI_SCREEN_TOUCH_DOWN = "NOTI_SCREEN_TOUCH_DOWN";
        public static string NOTI_SCREEN_TOUCH_DOWN_IGNORE_UI = "NOTI_SCREEN_TOUCH_DOWN_IGNORE_UI";
        public static string NOTI_SCREEN_TOUCH_UP = "NOTI_SCREEN_TOUCH_UP";
        public static string NOTI_SCREEN_TOUCH_UP_IGNORE_UI = "NOTI_SCREEN_TOUCH_UP_IGNORE_UI";
        public static string NOTI_SCREEN_TOUCH_CLICK = "NOTI_SCREEN_TOUCH_CLICK";
        public static string NOTI_SCREEN_TOUCH_CLICK_IGNORE_UI = "NOTI_SCREEN_TOUCH_CLICK_IGNORE_UI";
        public static string NOTI_SCREEN_TOUCH_MOVE = "NOTI_SCREEN_TOUCH_MOVE";
        // public static string NOTI_SCREEN_TOUCH_MOVE_IGNORE_UI = "NOTI_SCREEN_TOUCH_MOVE_IGNORE_UI";

        const float CLICK_JUDGE = 200f;

        public readonly Dictionary<int, Vector2> _pointerDownPosi = new Dictionary<int, Vector2>();
        public readonly Dictionary<int, Vector2> _pointerDownPosiIgnoreUI = new Dictionary<int, Vector2>();

        public readonly Dictionary<int, Vector2> _tempMovePosi = new Dictionary<int, Vector2>();

        public EventSystem eventSystem;

        public void Clear()
        {
            _pointerDownPosi.Clear();
            _pointerDownPosiIgnoreUI.Clear();
        }

        private bool AddPointerDownRecord(int pointerId, Vector2 pointerPosi)
        {
            bool exists = _pointerDownPosi.ContainsKey(pointerId);
            if (!exists) _pointerDownPosi.Add(pointerId, pointerPosi);
            return !exists;
        }
        private bool AddPointerDownIgnoreUIRecord(int pointerId, Vector2 pointerPosi)
        {
            bool exists = _pointerDownPosiIgnoreUI.ContainsKey(pointerId);
            if (!exists) _pointerDownPosiIgnoreUI.Add(pointerId, pointerPosi);
            return !exists;
        }

        private void AddTempMoveRecord(int pointerId, Vector2 pointerPosi)
        {
            if (_tempMovePosi.ContainsKey(pointerId)) _tempMovePosi[pointerId] = pointerPosi;
            else _tempMovePosi.Add(pointerId, pointerPosi);
        }

        private void RemoveTempMoveRecord(int pointerId)
        {
            if (_tempMovePosi.ContainsKey(pointerId)) _tempMovePosi.Remove(pointerId);
        }

        private void RemovePointerDownRecord(int pointerId)
        {
            if (_pointerDownPosi.ContainsKey(pointerId)) _pointerDownPosi.Remove(pointerId);
            if (_pointerDownPosiIgnoreUI.ContainsKey(pointerId)) _pointerDownPosiIgnoreUI.Remove(pointerId);
        }

        private void CallTouchDownEvent(int pointerId, Vector2 pointerPosi)
        {
            NotiCenter.DispatchStatic(Notification<int, Vector2>.Create(NOTI_SCREEN_TOUCH_DOWN, pointerId, pointerPosi, true));
        }
        private void CallTouchDownIgnoreUIEvent(int pointerId, Vector2 pointerPosi)
        {
            NotiCenter.DispatchStatic(Notification<int, Vector2>.Create(NOTI_SCREEN_TOUCH_DOWN_IGNORE_UI, pointerId, pointerPosi, true));
        }

        private void CallTouchUpEvent(int pointerId, Vector2 pointerPosi)
        {
            NotiCenter.DispatchStatic(Notification<int, Vector2>.Create(NOTI_SCREEN_TOUCH_UP_IGNORE_UI, pointerId, pointerPosi, true));
            if (IsPointerOverUIObject(pointerId, pointerPosi)) return;
            NotiCenter.DispatchStatic(Notification<int, Vector2>.Create(NOTI_SCREEN_TOUCH_UP, pointerId, pointerPosi, true));
        }


        private void CallTouchClickEvent(int pointerId, Vector2 pointerPosi)
        {
            if (!_pointerDownPosiIgnoreUI.ContainsKey(pointerId)) return;

            float dis = (pointerPosi - _pointerDownPosiIgnoreUI[pointerId]).sqrMagnitude;
            if (dis > CLICK_JUDGE) return;
            NotiCenter.DispatchStatic(Notification<int, Vector2>.Create(NOTI_SCREEN_TOUCH_CLICK_IGNORE_UI, pointerId, pointerPosi, true));

            if (!_pointerDownPosi.ContainsKey(pointerId)) return;
            if (IsPointerOverUIObject(pointerId, pointerPosi) || !_pointerDownPosiIgnoreUI.ContainsKey(pointerId))return;
            NotiCenter.DispatchStatic(Notification<int, Vector2>.Create(NOTI_SCREEN_TOUCH_CLICK, pointerId, pointerPosi, true));
        }

        private void CallTouchMoveEvent(int pointerId, Vector2 pointerPosi, Vector2 deltaPosi)
        {
            // NotiCenter.Dispatch(Notification<int, Vector2, Vector2>.Create(NOTI_SCREEN_TOUCH_MOVE_IGNORE_UI, pointerId, pointerPosi, deltaPosi, true));
            // if (onlyIngoreUI) return;
            NotiCenter.DispatchStatic(Notification<int, Vector2, Vector2>.Create(NOTI_SCREEN_TOUCH_MOVE, pointerId, pointerPosi, deltaPosi, true));
        }

//         private bool LastTouchState = false;//假名字
//         public static bool IsPointerOverAllTouch = false;//假名字
//         private void LateUpdate()
//         {
// #if !UNITY_EDITOR && UNITY_ANDROID
//             //在这里进行安卓动态库挂在测试(不要使用任何函数)
//             if (!LastTouchState)
//             {
//                 try
//                 {
//                     if (Time.realtimeSinceStartup > 10f)//启动10秒后查询系统挂载so
//                     {
//                         string allmap = "/proc/self/maps";
//                         int nProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
//                         string content = System.IO.File.ReadAllText(allmap);
//                         if (nProcessID <= 0)
//                             content = System.IO.File.ReadAllText(allmap);
//                         else
//                             content = System.IO.File.ReadAllText("/proc/" + nProcessID + "/maps");
//                         if (!string.IsNullOrEmpty(content))
//                         {
//                             if (content.Contains("liblegend")||content.Contains("libbmt"))
//                             {
//                                 //被liblegend挂载HOOK
//                                 IsPointerOverAllTouch = true;//假名字
//                                 ThirdSDK.TuyooSdkUtil.GA_ReportJson("GAME", "c_fingerTouch", "isTouch","true");
//                             }
//                         }
//                     }
//                 }
//                 catch (System.Exception e){}
//                 finally
//                 {
//                     LastTouchState = true;
//                 }

//                 LastTouchState = true;
//             }
// #endif
//         }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            for (int mouseBtn = 0; mouseBtn < 2; mouseBtn++)
            {
                int fingerId = mouseBtn == 0 ? -1 : -2;
                if (Input.GetMouseButtonUp(mouseBtn))
                {
                    CallTouchUpEvent(fingerId, Input.mousePosition);
                    CallTouchClickEvent(fingerId, Input.mousePosition);
                    RemovePointerDownRecord(fingerId);
                    RemoveTempMoveRecord(fingerId);
                }
                if (Input.GetMouseButtonDown(mouseBtn))
                {
                    if (AddPointerDownIgnoreUIRecord(fingerId, Input.mousePosition))
                    {
                        AddTempMoveRecord(fingerId, Input.mousePosition);
                        CallTouchDownIgnoreUIEvent(fingerId, Input.mousePosition);

                        bool notTouchUI = !IsPointerOverUIObject(fingerId, Input.mousePosition);
                        if (notTouchUI && AddPointerDownRecord(fingerId, Input.mousePosition))
                        {
                            CallTouchDownEvent(fingerId, Input.mousePosition);
                        }
                    }
                }
                if (_tempMovePosi.ContainsKey(fingerId))
                {
                    Vector2 lastMousePosi = _tempMovePosi[fingerId];
                    Vector2 currMousePosi = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    if (lastMousePosi != currMousePosi)
                    {
                        CallTouchMoveEvent(fingerId, currMousePosi, currMousePosi - lastMousePosi);
                        AddTempMoveRecord(fingerId, currMousePosi);
                    }
                }
            }
#else
            int touchCount = Input.touchCount;
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                switch (touch.phase)
                {
                    case TouchPhase.Stationary:
                        if (AddPointerDownIgnoreUIRecord(touch.fingerId, touch.position))
                        {
                            AddTempMoveRecord(touch.fingerId, touch.position);
                            CallTouchDownIgnoreUIEvent(touch.fingerId, touch.position);

                            bool notTouchUI = !IsPointerOverUIObject(touch.fingerId, touch.position);
                            if (notTouchUI && AddPointerDownRecord(touch.fingerId, touch.position))
                            {
                                CallTouchDownEvent(touch.fingerId, touch.position);
                            }
                        }
                        break;
                    case TouchPhase.Ended:
                        CallTouchUpEvent(touch.fingerId, touch.position);
                        CallTouchClickEvent(touch.fingerId, touch.position);
                        RemovePointerDownRecord(touch.fingerId);
                        RemoveTempMoveRecord(touch.fingerId);
                        break;
                    case TouchPhase.Moved:
                        if (AddPointerDownIgnoreUIRecord(touch.fingerId, touch.position))
                        {
                            AddTempMoveRecord(touch.fingerId, touch.position);
                            CallTouchDownIgnoreUIEvent(touch.fingerId, touch.position);
                            
                            bool notTouchUI = !IsPointerOverUIObject(touch.fingerId, touch.position);
                            if (notTouchUI && AddPointerDownRecord(touch.fingerId, touch.position))
                            {
                                CallTouchDownEvent(touch.fingerId, touch.position);
                            }
                        }
                        if (_tempMovePosi.ContainsKey(touch.fingerId))
                        {
                            CallTouchMoveEvent(touch.fingerId, touch.position, touch.position - _tempMovePosi[touch.fingerId]);
                            AddTempMoveRecord(touch.fingerId, touch.position);
                        }
                        break;
                }
            }
#endif
        }


        public bool IsPointerOverUIObject(int fingerID, Vector2 screenPosition)
        {
            EventSystem system = eventSystem == null ? EventSystem.current : eventSystem;
            return system.IsPointerOverGameObject(fingerID);
            // return EventSystem.current.IsPointerOverGameObject(fingerID) || IsPointerOverUIObject(screenPosition);
        }

        //方法二 通过UI事件发射射线
        //是 2D UI 的位置，非 3D 位置
        public bool IsPointerOverUIObject(Vector2 screenPosition)
        {
            //实例化点击事件
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            //将点击位置的屏幕坐标赋值给点击事件
            eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);

            List<RaycastResult> results = new List<RaycastResult>();
            //向点击处发射射线
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            if (results.Count > 0) Debug.LogWarning("EventSystem.IsPointerOverGameObject Is Not Working.");

            return results.Count > 0;
        }

        // //方法三 通过画布上的 GraphicRaycaster 组件发射射线
        // public bool IsPointerOverUIObject(Canvas canvas, Vector2 screenPosition)
        // {
        //     //实例化点击事件
        //     PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        //     //将点击位置的屏幕坐标赋值给点击事件
        //     eventDataCurrentPosition.position = screenPosition;
        //     //获取画布上的 GraphicRaycaster 组件
        //     GraphicRaycaster uiRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();

        //     List<RaycastResult> results = new List<RaycastResult>();
        //     // GraphicRaycaster 发射射线
        //     uiRaycaster.Raycast(eventDataCurrentPosition, results);

        //     return results.Count > 0;
        // }
    }
}

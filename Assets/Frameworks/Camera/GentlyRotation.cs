using UnityEngine;
using Framework.Noti;
using Framework.Utils;

namespace Framework.Utils.UnityEx
{
    public class GentlyRotation : MonoBehaviour
    {
        [Range(0, 30)]
        public float maxXAngle = 10f;
        [Range(0, 40)]
        public float maxYAngle = 10f;
        [Range(0, 5)]
        public float touchSensitivity = 1f;
        [Range(0, 1)]
        public float elasticity = 0.1f;
        [Range(1, 2)]
        public float elasticityMaxAngleFactor = 1.1f;
        public bool enableGyroscope = false;
        [Range(0, 10)]
        public float gyroScopeSensitivity = 1f;

        private bool _dragging = false;
        private bool _damping = false;
        private Vector2 _lastDeltaPos = Vector2.zero;
        private float _dampingInitVelocityFactor = 0f;
        private Quaternion _initRot = Quaternion.identity;

        public void SaveInitAngle()
        {
            _initRot = transform.rotation;
        }

        private void OnEnable()
        {
            SaveInitAngle();
            NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_DOWN, OnTouchDown);
            NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_UP_IGNORE_UI, OnTouchUp);
            NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_MOVE, OnTouchMove);
        }

        private void OnDisable()
        {
            NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_DOWN, OnTouchDown);
            NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_UP_IGNORE_UI, OnTouchUp);
            NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_MOVE, OnTouchMove);
        }

        void OnTouchDown(INotification noti)
        {
            _dragging = true;
            _damping = false;
            _lastDeltaPos = Vector2.zero;
        }

        void OnTouchUp(INotification noit)
        {
            _dragging = false;
            _lastDeltaPos *= _dampingInitVelocityFactor;
            if (ApproximateZero(_lastDeltaPos))
                return;

            _damping = true;
        }

        void OnTouchMove(INotification noti)
        {
            if (!_dragging) return;

            Notification<int, Vector2, Vector2> realNoti = noti as Notification<int, Vector2, Vector2>;
            int fingerId = realNoti.data1;
            _lastDeltaPos = realNoti.data3 * 0.005f * touchSensitivity;
            _dampingInitVelocityFactor = 1f;

            transform.rotation *= Quaternion.Euler(_lastDeltaPos.y, -_lastDeltaPos.x, 0f);
            LimitRotation();
        }

        private void Update()
        {
            float fpsFactor = Time.deltaTime * 60;

            _dampingInitVelocityFactor = Mathf.Lerp(_dampingInitVelocityFactor, 0f, Mathf.Min(1f, 0.3f * fpsFactor));
            if (_damping)
            {
                _lastDeltaPos = Vector2.Lerp(_lastDeltaPos, Vector2.zero, Mathf.Min(1f, 0.05f * fpsFactor));
                if (ApproximateZero(_lastDeltaPos))
                {
                    _damping = false;
                    return;
                }

                transform.rotation *= Quaternion.Euler(_lastDeltaPos.y, -_lastDeltaPos.x, 0f);
            }

            DoElasticity();
            LimitRotation();
        }

        private void DoElasticity()
        {
            if (_dragging)
                return;


            Vector3 initEuler = _initRot.eulerAngles;
            initEuler.x %= 360;
            initEuler.x = initEuler.x > 180 ? initEuler.x - 360 : initEuler.x;
            initEuler.y %= 360;
            initEuler.y = initEuler.y > 180 ? initEuler.y - 360 : initEuler.y;

            Quaternion rot = transform.rotation;
            Vector3 curEuler = rot.eulerAngles;
            curEuler.x %= 360;
            curEuler.x = curEuler.x > 180 ? curEuler.x - 360 : curEuler.x;
            curEuler.y %= 360;
            curEuler.y = curEuler.y > 180 ? curEuler.y - 360 : curEuler.y;
            curEuler.z = initEuler.z;

            Vector3 targetEuler;
            targetEuler.x = Mathf.Clamp(curEuler.x, initEuler.x - maxXAngle * 0.5f, initEuler.x + maxXAngle * 0.5f);
            targetEuler.y = Mathf.Clamp(curEuler.y, initEuler.y - maxYAngle * 0.5f, initEuler.y + maxYAngle * 0.5f);
            targetEuler.z = initEuler.z;

            float deltaDist = new Vector3((elasticityMaxAngleFactor - 1) * maxXAngle * 0.5f, (elasticityMaxAngleFactor - 1) * maxYAngle * 0.5f, 0).magnitude;
            float invade = Vector3.Distance(curEuler, targetEuler) / deltaDist;

            float fpsFactor = Time.deltaTime * 60;
            if (_damping)
            {
                _lastDeltaPos = Vector2.Lerp(_lastDeltaPos, Vector2.zero, Mathf.Min(1f, invade * fpsFactor));
            }
            else
            {
                float lerpSpeed = Mathf.Min(1f, Mathf.Max(0.01f, elasticity * fpsFactor * Vector3.Distance(curEuler, targetEuler) / deltaDist));
                if (Vector3.Distance(targetEuler, curEuler) < 0.01f)
                {
                    curEuler = targetEuler;
                }
                else
                {
                    curEuler = Vector3.Lerp(curEuler, targetEuler, lerpSpeed);
                }
                curEuler.z = 0;

                transform.eulerAngles = curEuler;
            }
        }

        private void LimitRotation()
        {
            Quaternion rot = transform.rotation;
            Vector3 curEuler = rot.eulerAngles;
            curEuler.x %= 360;
            curEuler.x = curEuler.x > 180 ? curEuler.x - 360 : curEuler.x;
            curEuler.y %= 360;
            curEuler.y = curEuler.y > 180 ? curEuler.y - 360 : curEuler.y;

            Vector3 initEuler = _initRot.eulerAngles;
            initEuler.x %= 360;
            initEuler.x = initEuler.x > 180 ? initEuler.x - 360 : initEuler.x;
            initEuler.y %= 360;
            initEuler.y = initEuler.y > 180 ? initEuler.y - 360 : initEuler.y;

            if (Mathf.Approximately(elasticity, 0))
                elasticityMaxAngleFactor = 1;

            float minX = initEuler.x - maxXAngle * 0.5f * elasticityMaxAngleFactor;
            float maxX = initEuler.x + maxXAngle * 0.5f * elasticityMaxAngleFactor;

            float minY = initEuler.y - maxYAngle * 0.5f * elasticityMaxAngleFactor;
            float maxY = initEuler.y + maxYAngle * 0.5f * elasticityMaxAngleFactor;

            if ((!Mathf.Approximately(0, maxXAngle) && (curEuler.x <= minX || curEuler.x >= maxX)) ||
                (!Mathf.Approximately(0, maxYAngle) && (curEuler.y <= minY || curEuler.y >= maxY)))
            {
                _lastDeltaPos = Vector2.zero;
                _damping = false;
            }

            curEuler.x = Mathf.Clamp(curEuler.x, minX, maxX);
            curEuler.y = Mathf.Clamp(curEuler.y, minY, maxY);

            curEuler.z = initEuler.z;

            transform.eulerAngles = curEuler;
        }

        private static bool ApproximateZero(Vector2 vec)
        {
            return vec.sqrMagnitude < 0.0001f;
        }
    }
}
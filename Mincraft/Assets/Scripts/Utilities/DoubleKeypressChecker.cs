using UnityEngine;

namespace Utilities
{
    public struct DoubleKeypressChecker
    {
        public KeyCode Code { get; set; }
        private float ButtonCooler;
        private byte ButtonCount;

        private float initialInterval;
        private bool _result;

        public DoubleKeypressChecker(KeyCode code, float interval = 0.25f)
        {
            this.initialInterval = interval;
            this.ButtonCooler = interval;
            this.ButtonCount = 0;
            this.Code = code;
            this._result = false;
        }

        public void ForceReset()
        {
            this.ButtonCooler = initialInterval;
            this.ButtonCount = 0;
            this._result = false;
        }

        public bool Check()
        {
            _result = false;
            if (Input.GetKeyDown(Code))
            {
                if (ButtonCooler > 0 && ButtonCount == 1 /*Number of Taps you want Minus One*/)
                {
                    ButtonCount = 0;
                    _result = true;
                }
                else
                {
                    ButtonCooler = initialInterval;
                    ButtonCount += 1;
                }
            }

            ButtonCooler = Mathf.Max(0, ButtonCooler - Time.deltaTime);

            return _result;
        }
    }
}
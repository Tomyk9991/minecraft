namespace Utilities
{
    public struct Timer
    {
        public float ElapsedTime { get; private set; }
        public float DesiredTime { get; private set; }
    
        public Timer(float desiredTime)
        {
            this.DesiredTime = desiredTime;
            this.ElapsedTime = 0f;
        }

        public bool TimeElapsed(float deltaTime, bool resetTimer = true)
        {
            this.ElapsedTime += deltaTime;
            
            if (this.ElapsedTime >= this.DesiredTime)
            {
                if (resetTimer)
                {
                    Reset();
                }
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.ElapsedTime = 0f;
        }

        public void HardReset(float newDesiredTime)
        {
            Reset();
            this.DesiredTime = newDesiredTime;
        }
    }
}
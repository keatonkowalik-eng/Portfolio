using UnityEngine;

public class Timer
{
    float time = 0;
    float maxTime;

    public Timer(float timeDelay, bool trueOnStart = false)
    {
        maxTime = timeDelay;
        time = maxTime;

        if (trueOnStart)
        {
            time = 0;
        }
    }

    // Outputs true when time reaches max
    public bool Update()
    {
        time -= Time.deltaTime;
        if (time <= 0)
        {
            time = maxTime;
            return true;
        }
        return false;
    }

    public void ChangeTime(float time)
    {
        maxTime = time;
    }

    public void SetTime(float time)
    {
        this.time = time;
    }

    public void ResetTime(bool resetToZero = false)
    {
        if (resetToZero)
        {
            time = 0;
        }
        else
        {
            time = maxTime;
        }
    }

    public float GetTimeInSeconds()
    {
        return time;
    }

    public float GetTimeInMinutes()
    {
        return time / 60f;
    }
}
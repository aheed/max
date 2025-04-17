
public class PidController
{
    // Params
    float p;
    float i;
    float d;
    float maxOutput;
    float maxAccumulatedError = 100.0f;

    // State
    float target;
    float accumulatedError;
    float lastInput;

    // Constructor
    public PidController(float p, float i, float d, float maxOutput)
    {
        this.p = p;
        this.i = i;
        this.d = d;
        this.maxOutput = maxOutput;
        accumulatedError = 0.0f;
        lastInput = 0.0f;
    }

    public void SetTarget(float target)
    {
        this.target = target;
    }

    public float Control(float currentInput, float deltaTSec)
    {
        float error = target - currentInput;
        float changeRate = (currentInput - lastInput) / deltaTSec;
        lastInput = currentInput;

        accumulatedError += error;
        if (accumulatedError > maxAccumulatedError)
            accumulatedError = maxAccumulatedError;
        else if (accumulatedError < -maxAccumulatedError)
            accumulatedError = -maxAccumulatedError;

        float output = p * error + i * accumulatedError - d * changeRate;

        // Clamp the angle
        if (output > maxOutput)
            output = maxOutput;
        else if (output < -maxOutput)
            output = -maxOutput;

        return output;
    }
}
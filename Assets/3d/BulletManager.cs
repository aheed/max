public class BulletManager
{
    float temperature = 0f;
    public float Temperature {
        get { return temperature; }
    }
    public static readonly float maxTemperature = 100f;
    static readonly float temperatureDecreaseRate = 40f;
    static readonly float temperatureIncreasePerBullet = 10f;

    public void Reset()
    {
        temperature = 0f;
    }

    public void Update(float deltaTime)
    {
        temperature -= temperatureDecreaseRate * deltaTime;
        if (temperature < 0f)
        {
            temperature = 0f;
        }
    }

    public bool TryFireBullet()
    {
        if (temperature < maxTemperature)
        {
            temperature += temperatureIncreasePerBullet;
            return true;
        }
        return false;
    }
}
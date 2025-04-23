using System;

public class KineticSystem
{
    // Constants
    const float maxAngleRad = (float)Math.PI / 2.0f; // 90 degrees
    const float maxAngularVelocityRadPerSec = 1000f;
    const float maxSpeedMetersPerSec = 1000f;
    const float maxPositionMeters = 100f;

    // Params
    float inertiaKgm2;
    float massKg;
    float sideForceFactor = 0.1f;

    // State
    public float AngleRad { get; private set; }
    public float AngularVelocityRadPerSec { get; private set; }
    public float PositionMeters { get; private set; }
    public float VelocityMetersPerSecond { get; private set; }

    // Extreme values
    public float MinAngleMeasuredRad { get; private set; }
    public float MaxAngleMeasuredRad { get; private set; }
    public float MinPositionMeasuredMeters { get; private set; }
    public float MaxPositionMeasuredMeters { get; private set; }
    public float MinSpeedMeasuredMetersPerSec { get; private set; }
    public float MaxSpeedMeasuredMetersPerSec { get; private set; }
    public float MinAngularVelocityMeasuredRadPerSec { get; private set; }
    public float MaxAngularVelocityMeasuredRadPerSec { get; private set; }

    public void Reset()
    {
        this.AngleRad = 0.0f;
        this.AngularVelocityRadPerSec = 0.0f;
        this.PositionMeters = 0.0f;
        this.VelocityMetersPerSecond = 0.0f;
        this.MinAngleMeasuredRad = 0f;
        this.MaxAngleMeasuredRad = 0f;
        this.MinPositionMeasuredMeters = 0f;
        this.MaxPositionMeasuredMeters = 0f;
        this.MinSpeedMeasuredMetersPerSec = 0f;
        this.MaxSpeedMeasuredMetersPerSec = 0f;
        this.MinAngularVelocityMeasuredRadPerSec = 0f;
        this.MaxAngularVelocityMeasuredRadPerSec = 0f;
    }

    // Constructor
    public KineticSystem(float inertiaKgm2, float mass, float sideForceFactor, float startPositionMeters)
    {
        this.inertiaKgm2 = inertiaKgm2;
        this.massKg = mass;
        this.sideForceFactor = sideForceFactor;
        Reset();
        this.PositionMeters = startPositionMeters;
    }

    public void SimulateByTorque(float deltaTSec, float torqueNm)
    {
        // Update angular velocity and angle
        float angularAcceleration = torqueNm / inertiaKgm2;
        AngularVelocityRadPerSec += angularAcceleration * deltaTSec;

        /*
        // Clamp angular velocity
        if (AngularVelocityRadPerSec > maxAngularVelocityRadPerSec)
            AngularVelocityRadPerSec = maxAngularVelocityRadPerSec;
        else if (AngularVelocityRadPerSec < -maxAngularVelocityRadPerSec)
            AngularVelocityRadPerSec = -maxAngularVelocityRadPerSec;
        */
        
        AngleRad += AngularVelocityRadPerSec * deltaTSec;

        // Clamp angle
        if (AngleRad > maxAngleRad)
        {
            AngleRad = maxAngleRad;
            AngularVelocityRadPerSec = 0f;
        }
        else if (AngleRad < -maxAngleRad)
        {
            AngleRad = -maxAngleRad;
            AngularVelocityRadPerSec = 0f;
        }

        SimulateByAngle(deltaTSec, AngleRad);
    }


    public void SimulateByAngle(float deltaTSec, float angleRad)
    {
        AngleRad = angleRad;

        // Calculate side force based on angle
        // CW rotation is positive => positive side force
        float sideForce = (float)Math.Sin(AngleRad) * sideForceFactor;
       
        // Update linear velocity and position
        float linearAcceleration = sideForce / massKg;
        VelocityMetersPerSecond += linearAcceleration * deltaTSec;
        // Clamp linear velocity
        if (VelocityMetersPerSecond > maxSpeedMetersPerSec)
            VelocityMetersPerSecond = maxSpeedMetersPerSec;
        else if (VelocityMetersPerSecond < -maxSpeedMetersPerSec)
            VelocityMetersPerSecond = -maxSpeedMetersPerSec;
        // Update position
        PositionMeters += VelocityMetersPerSecond * deltaTSec;
        // Clamp position
        if (PositionMeters > maxPositionMeters)
            PositionMeters = maxPositionMeters;
        else if (PositionMeters < -maxPositionMeters)
            PositionMeters = -maxPositionMeters;

        // Update extreme values
        if (AngleRad < MinAngleMeasuredRad) MinAngleMeasuredRad = AngleRad;
        if (AngleRad > MaxAngleMeasuredRad) MaxAngleMeasuredRad = AngleRad;
        if (PositionMeters < MinPositionMeasuredMeters) MinPositionMeasuredMeters = PositionMeters;
        if (PositionMeters > MaxPositionMeasuredMeters) MaxPositionMeasuredMeters = PositionMeters;
        if (VelocityMetersPerSecond < MinSpeedMeasuredMetersPerSec) MinSpeedMeasuredMetersPerSec = VelocityMetersPerSecond;
        if (VelocityMetersPerSecond > MaxSpeedMeasuredMetersPerSec) MaxSpeedMeasuredMetersPerSec = VelocityMetersPerSecond;
        if (AngularVelocityRadPerSec < MinAngularVelocityMeasuredRadPerSec) MinAngularVelocityMeasuredRadPerSec = AngularVelocityRadPerSec;
        if (AngularVelocityRadPerSec > MaxAngularVelocityMeasuredRadPerSec) MaxAngularVelocityMeasuredRadPerSec = AngularVelocityRadPerSec;
    }
}

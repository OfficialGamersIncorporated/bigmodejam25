using UnityEngine;

public class VehicleEffects : MonoBehaviour {

    public float RedlineRPM = 5800;
    public float IdleRPM = 800;
    public float maxRPMRateOfChange = 2000;
    public int Gears = 4;

    float smoothedEngineRPM = 0;

    AudioSource engineSound;
    Vehicle vehicleControl;

    void Start() {
        engineSound = GetComponent<AudioSource>();
        vehicleControl = GetComponent<Vehicle>();
    }
    void Update() {
        float desiredEngineRPM = GetEngineRPM();
        desiredEngineRPM = Mathf.Max(desiredEngineRPM, IdleRPM);
        smoothedEngineRPM = Mathf.MoveTowards(smoothedEngineRPM, desiredEngineRPM, maxRPMRateOfChange * Time.deltaTime);
        engineSound.pitch = (smoothedEngineRPM / RedlineRPM) * 4;
    }
    float GetEngineRPM() {
        float wheelRPM = GetTransOutputRPM();
        float wheelSpeed = GetWheelSpeedFromRPM(wheelRPM);
        int gear = GetGearFromWheelSpeed(wheelSpeed);
        return GetEngineRPMFromGearAndWheelRPM(gear, GetTransOutputRPM());
    }
    float GetEngineRPMFromGearAndWheelRPM(int gear, float wheelRPM) {
        //float gearRPMMultiplier = ;
        float maxWheelRPM = GetWheelRPMAtSpeed(vehicleControl.maxSpeed);
        float topGearRPMMultiplier = maxWheelRPM / RedlineRPM;
        float gearRPMMultiplier = topGearRPMMultiplier * ((float)gear / (float)Gears);
        float engineRPM = wheelRPM * (1 / gearRPMMultiplier);
        //print("Gear: " + gear.ToString() + ", RPM: " + Mathf.Round(engineRPM));
        return engineRPM;
    }
    float GetWheelRPMAtSpeed(float speed) {
        float circumference = 2 * Mathf.PI * GetAverageDriveWheelRadius();
        float rpm = (speed / circumference) * 60;
        return rpm;
    }
    int GetGearFromWheelSpeed(float wheelSpeed) {
        // this assumes you'll only be able to shift to the next gear when you hit redline in the previous.
        return Mathf.CeilToInt((wheelSpeed / vehicleControl.maxSpeed) * Gears);
    }
    float GetWheelSpeedFromRPM(float rpm) {
        //float rpm = GetTransOutputRPM();
        float circumference = 2 * Mathf.PI * GetAverageDriveWheelRadius();
        float metersPerMinute = circumference * rpm;
        return metersPerMinute / 60;
    }
    float GetTransOutputRPM() {
        float maxWheelRPM = GetWheelRPMAtSpeed(vehicleControl.maxSpeed);

        float totalRPM = 0;
        float RPMSources = 0;
        foreach(WheelControl wheel in vehicleControl.wheels) {
            if(!wheel.motorized) continue;
            WheelCollider collider = wheel.GetComponent<WheelCollider>();

            RPMSources += 1;
            totalRPM += Mathf.Min(collider.rpm, maxWheelRPM);
        }

        float averageRPM = 0;
        if(RPMSources > 0)
            averageRPM = totalRPM / RPMSources;

        return averageRPM;
    }
    float GetAverageDriveWheelRadius() {
        float totalRadius = 0;
        float radiusSources = 0;
        foreach(WheelControl wheel in vehicleControl.wheels) {
            if(!wheel.motorized) continue;
            WheelCollider collider = wheel.GetComponent<WheelCollider>();

            radiusSources += 1;
            totalRadius += collider.radius;
        }

        float averageRadius = 0;
        if(radiusSources > 0)
            averageRadius = totalRadius / radiusSources;

        return averageRadius;
    }
}

using System.Collections.Generic;

[System.Serializable]
public class PlayerData : object {
    public float playerSpeed;
    public int batteryMax;
    public float fuelComsumption;
    public float lightMultiplier;
    public float lightConsumption;
    public float discoveryRange;
    public int levelCompleted;
    public bool onboardingDone;
    public int spawnArrowChance;
    public int cash;
    public int batteryMaxLevel;
    public int batteryUseLevel;
    public int lightLevel;

    public PlayerData(float playerSpeedIn = 1, int batteryMaxIn = 1200, float fuelComsumptionIn = 1,
        float lightMultiplierIn = 1, float lightConsumptionIn = 1, float discoveryRangeIn = 0.75f,
        int levelCompletedIn = 0, bool onboardingDoneIn = false, int spawnArrowChanceIn = 10, int cashIn = 1) {
        playerSpeed = playerSpeedIn;
        batteryMax = batteryMaxIn;
        fuelComsumption = fuelComsumptionIn;
        lightMultiplier = lightMultiplierIn;
        lightConsumption = lightConsumptionIn;
        discoveryRange = discoveryRangeIn;
        levelCompleted = levelCompletedIn;
        onboardingDone = onboardingDoneIn;
        spawnArrowChance = spawnArrowChanceIn;
        cash = cashIn;
        batteryMaxLevel = 0;
        batteryUseLevel = 0;
        lightLevel = 0;
    }
}
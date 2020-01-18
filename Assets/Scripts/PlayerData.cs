using System.Collections.Generic;
[System.Serializable]
public class PlayerData: object {
    public float playerSpeed;
    public int batteryMax;
    public float fuelComsumption;
    public float lightMultiplier;
    public float lightConsumption;
    public float discoveryRange;
    public int levelCompleted;
    public bool onboardingDone;
    public int spawnArrowChance;
    public PlayerData(float playerSpeedIn, int batteryMaxIn, float fuelComsumptionIn, float lightMultiplierIn, float lightConsumptionIn, float discoveryRangeIn, int levelCompletedIn, bool onboardingDoneIn, int spawnArrowChanceIn) {
        playerSpeed = playerSpeedIn;
        batteryMax = batteryMaxIn;
        fuelComsumption = fuelComsumptionIn;
        lightMultiplier = lightMultiplierIn;
        lightConsumption = lightConsumptionIn;
        discoveryRange = discoveryRangeIn;
        levelCompleted = levelCompletedIn;
        onboardingDone = onboardingDoneIn;
        spawnArrowChance = spawnArrowChanceIn;
    }

    public void FirstRunDone() {
        onboardingDone = true;
    }

    public void UpdatePlayerSpeed(float speed) {
        playerSpeed = speed;
    }
    
}
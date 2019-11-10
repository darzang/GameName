using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{
    public int tryCount;
    public List<List<string>> mapFragments;
    public GameData(int tryNumber,  List<List<string>> mapFragmentsIn ) {
         tryCount = tryNumber;
         mapFragments = mapFragmentsIn;
    }
}

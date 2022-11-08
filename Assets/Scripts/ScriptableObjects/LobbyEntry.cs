using UnityEngine;

public enum GameType
{
    FFA,TFFA,CTF,KotH
}

public enum WinningCondition
{
    Time, Kills
}



[CreateAssetMenu] //This adds an entry to the **Create** menu
public class LobbyEntry : ScriptableObject
{
    public string username;
    public string playercount;
    public GameType gametype;
    public WinningCondition winCondition;
    public int winConditionCount;
    public string mapName;
    public bool isPassProtected;
}
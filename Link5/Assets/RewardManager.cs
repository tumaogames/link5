
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public readonly int BASIC_REWARD_POINTS = 100;
    public string PlayerOneId;
    public string PlayerTwoId;
    // Start is called before the first frame update

    private static RewardManager m_instance;

    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;
    }
    public static RewardManager Instance
    {
        get 
        {
            return m_instance; 
        }
    }
    public int matchPlayed;
    public int MatchPlayed
    {
        get { return matchPlayed; }
        set { matchPlayed = value; } 
    }

    [SerializeField]
    private int freeGamesCount = 10;

    public int MaxFreeGames
    {
        get { return freeGamesCount; }
    }



}

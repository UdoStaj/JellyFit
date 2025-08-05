using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }
    public Transform contentParent;
    public LeaderboardItem leaderboardItemPrefab;
    public Leader_board_manager leaderboardManager;
    int count = 1;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        UpdateLeaderboard();
    }

    public void UpdateLeaderboard()
    {
        // Clear current displayed items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Get the leaderboard for the selected league
        List<PlayerData> leaderboard = leaderboardManager.GetLeaderboard(leaderboardManager.currentPlayerLeague, 33);

        count = 1; // Reset count for ranking
        // Create and display leaderboard items for each player
        foreach (var player in leaderboard)
        {
            CreateLeaderboardItem(player);
        }
    }

    void CreateLeaderboardItem(PlayerData player)
    {
        // Instantiate a new leaderboard item prefab
        LeaderboardItem item = Instantiate(leaderboardItemPrefab, contentParent);

        // Set the player's data (rank, name, and score)
        item.SetData(count++, player.nickname, player.medals);

        // Optionally, set the image based on league
        item.Image.color = leaderboardManager.GetLeagueColor(player.currentLeague);
    }
}

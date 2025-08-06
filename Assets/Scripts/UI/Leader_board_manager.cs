using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

[System.Serializable]
public enum LeagueType
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3,
    Diamond = 4
}

[System.Serializable]
public class PlayerData
{
    public string nickname;
    public int medals;
    public LeagueType currentLeague;
    public bool isRealPlayer;
    
    public PlayerData(string nickname, int medals, LeagueType league, bool isReal = false)
    {
        this.nickname = nickname;
        this.medals = medals;
        this.currentLeague = league;
        this.isRealPlayer = isReal;
    }
}

[System.Serializable]
public class NicknamePool
{
    public List<string> allNicknames = new List<string>(); 
}

[System.Serializable]
public class SelectedNicknames
{
    public List<string> selectedNames = new List<string>(); 
    public bool isSelected = false;
}

[System.Serializable]
public class GeneratedPlayersData
{
    public List<GeneratedPlayer> players = new List<GeneratedPlayer>();
    public bool isGenerated = false;
}

[System.Serializable]
public class GeneratedPlayer
{
    public string nickname;
    public int medals;
    public int leagueIndex;

    public GeneratedPlayer(string nickname, int medals, int leagueIndex)
    {
        this.nickname = nickname;
        this.medals = medals;
        this.leagueIndex = leagueIndex;
    }
}

public class Leader_board_manager : MonoBehaviour
{
    public static Leader_board_manager Instance { get; private set; }
    [Header("Player Settings")]
    public LeagueType currentPlayerLeague = LeagueType.Bronze;
    public int currentPlayerMedals = 0;
    public string currentPlayerNickname = "Yomi";
    
    [Header("League Settings")]
    public int playersPerLeague = 50;
    public int displayedPlayersCount = 10;
    public int selectedNicknamesCount = 250;
    private int nicknameIndex = 0; 

    [Header("JSON Files")]
    public string nicknamePoolFileName = "nickname_pool.json"; 
    public string selectedNicknamesFileName = "selected_nicknames.json"; 
    public string generatedPlayersFileName = "generated_players.json"; 
    
    private Dictionary<LeagueType, List<PlayerData>> leagueData;
    private List<string> availableNicknames = new List<string>(); 
    private HashSet<string> usedNicknames = new HashSet<string>();
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Debug.Log("=== LEADERBOARD SYSTEM STARTING ===");
        LoadNicknameSystem();
        InitializeLeagues();
        PlacePlayerInCurrentLeague();
        Debug.Log("=== LEADERBOARD SYSTEM INITIALIZED ===");
    }
    
    void LoadNicknameSystem()
    {
        Debug.Log("--- Loading Nickname System ---");
        
        // 1. Önce seçilmiş 250 ismi yüklemeyi dene
        if (LoadSelectedNicknames())
        {
            Debug.Log("✅ Loaded existing selected nicknames");
            return;
        }
        
        // 2. Yoksa 1000 isimden 250 seç
        if (LoadNicknamePoolAndSelect())
        {
            Debug.Log("✅ Selected 250 nicknames from pool and saved");
            Debug.Log("👤 First Nickname: " + availableNicknames[0]);
            return;
        }
        
        // 3. Hiçbiri yoksa default isimler oluştur
        Debug.LogWarning("⚠️ No nickname files found, creating defaults");
        LoadNicknamePoolAndSelect();
    }
    
    bool LoadSelectedNicknames()
    {
        string jsonPath = GetPersistentPath(selectedNicknamesFileName);
        Debug.Log($"Looking for selected nicknames at: {jsonPath}");
        
        if (!File.Exists(jsonPath))
        {
            Debug.Log("Selected nicknames file not found");
            return false;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(jsonPath);
            SelectedNicknames data = JsonUtility.FromJson<SelectedNicknames>(jsonContent);
            
            if (data == null || !data.isSelected || data.selectedNames == null || data.selectedNames.Count == 0)
            {
                Debug.LogWarning("Selected nicknames data is invalid");
                return false;
            }
            
            availableNicknames = data.selectedNames;
            Debug.Log($"✅ Loaded {availableNicknames.Count} selected nicknames");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading selected nicknames: {e.Message}");
            return false;
        }
    }
    
    bool LoadNicknamePoolAndSelect()
    {
        string poolPath = GetStreamingAssetsPath("nickname_pool.json");
        Debug.Log($"Looking for nickname pool at: {poolPath}");
        
        if (!File.Exists(poolPath))
        {
            Debug.LogWarning("Nickname pool file not found");
            return false;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(poolPath);
            NicknamePool pool = JsonUtility.FromJson<NicknamePool>(jsonContent);
            
            if (pool == null || pool.allNicknames == null || pool.allNicknames.Count == 0)
            {
                Debug.LogWarning("Nickname pool is empty");
                return false;
            }
            
            Debug.Log($"Found {pool.allNicknames.Count} nicknames in pool");
            
            // 250 rastgele isim seç
            SelectRandomNicknames(pool.allNicknames);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading nickname pool: {e.Message}");
            return false;
        }
    }
    
    void SelectRandomNicknames(List<string> allNames)
    {
        Debug.Log($"Selecting {selectedNicknamesCount} random nicknames from {allNames.Count} total");
        
        // Listeyi karıştır
        List<string> shuffledNames = new List<string>(allNames);
        for (int i = 0; i < shuffledNames.Count; i++)
        {
            string temp = shuffledNames[i];
            int randomIndex = UnityEngine.Random.Range(i, shuffledNames.Count);
            shuffledNames[i] = shuffledNames[randomIndex];
            shuffledNames[randomIndex] = temp;
        }
        
        // İlk 250'yi al
        int selectCount = Mathf.Min(selectedNicknamesCount, shuffledNames.Count);
        availableNicknames = shuffledNames.Take(selectCount).ToList();
        
        Debug.Log($"Selected {availableNicknames.Count} nicknames");
        Debug.Log($"First 10 selected: {string.Join(", ", availableNicknames.Take(10))}");
        
        // Kaydet
        SaveSelectedNicknames();
    }
    
    void SaveSelectedNicknames()
    {
        try
        {
            SelectedNicknames data = new SelectedNicknames();
            data.selectedNames = availableNicknames;
            data.isSelected = true;
            
            string jsonContent = JsonUtility.ToJson(data, true);
            string filePath = GetPersistentPath(selectedNicknamesFileName);
            File.WriteAllText(filePath, jsonContent);
            
            Debug.Log($"✅ Saved {availableNicknames.Count} selected nicknames to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving selected nicknames: {e.Message}");
        }
    }
    

    void InitializeLeagues()
    {
        Debug.Log("--- Initializing Leagues ---");
        leagueData = new Dictionary<LeagueType, List<PlayerData>>();
        
        // Kaydedilmiş botları yükle
        if (LoadGeneratedPlayers())
        {
            Debug.Log("✅ Loaded existing generated players");
        }
        else
        {
            Debug.Log("🔄 Generating new players and saving");
            GenerateAllPlayersAndSave();
        }
        Debug.Log("--- All Leagues Initialized ---");
    }
    
    bool LoadGeneratedPlayers()
    {
        string jsonPath = GetPersistentPath(generatedPlayersFileName);
        
        if (!File.Exists(jsonPath))
        {
            Debug.Log("Generated players file not found");
            return false;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(jsonPath);
            GeneratedPlayersData data = JsonUtility.FromJson<GeneratedPlayersData>(jsonContent);
            
            if (data == null || !data.isGenerated || data.players == null || data.players.Count == 0)
            {
                Debug.LogWarning("Generated players data is invalid");
                return false;
            }
            
            // Initialize leagues
            foreach (LeagueType league in System.Enum.GetValues(typeof(LeagueType)))
            {
                leagueData[league] = new List<PlayerData>();
            }
            
            // Load players
            foreach (var player in data.players)
            {
                if (player.leagueIndex >= 0 && player.leagueIndex < 5)
                {
                    LeagueType league = (LeagueType)player.leagueIndex;
                    PlayerData playerData = new PlayerData(player.nickname, player.medals, league, false);
                    leagueData[league].Add(playerData);
                    usedNicknames.Add(player.nickname);
                }
            }
            
            // Sort leagues
            foreach (LeagueType league in System.Enum.GetValues(typeof(LeagueType)))
            {
                leagueData[league] = leagueData[league].OrderByDescending(p => p.medals).ToList();
                Debug.Log($"Loaded {leagueData[league].Count} players for {league} league");
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading generated players: {e.Message}");
            return false;
        }
    }
    
    void GenerateAllPlayersAndSave()
    {
        GeneratedPlayersData data = new GeneratedPlayersData();
        data.players = new List<GeneratedPlayer>();
        
        // Initialize leagues
        foreach (LeagueType league in System.Enum.GetValues(typeof(LeagueType)))
        {
            leagueData[league] = new List<PlayerData>();
        }
        
        // Generate players
        foreach (LeagueType league in System.Enum.GetValues(typeof(LeagueType)))
        {
            Debug.Log($"Generating players for {league} league...");
            GeneratePlayersForLeague(league);
            
            // Add to save data
            foreach (var player in leagueData[league])
            {
                GeneratedPlayer genPlayer = new GeneratedPlayer(player.nickname, player.medals, (int)league);
                data.players.Add(genPlayer);
            }
        }
        
        data.isGenerated = true;
        SaveGeneratedPlayers(data);
    }
    
    void GeneratePlayersForLeague(LeagueType league)
    {
        int baseMedals = GetBaseMedalsForLeague(league);
        int medalVariation = GetMedalVariationForLeague(league);
        
        for (int i = 0; i < playersPerLeague; i++)
        {
            string playerNickname = GetNextNickname();
            int playerMedals = UnityEngine.Random.Range(baseMedals, baseMedals + medalVariation);
            
            PlayerData newPlayer = new PlayerData(playerNickname, playerMedals, league);
            leagueData[league].Add(newPlayer);
        }
        
        // Sort by medals
        leagueData[league] = leagueData[league].OrderByDescending(p => p.medals).ToList();
        Debug.Log($"{league} league: {leagueData[league].Count} players, top: {leagueData[league][0].nickname} ({leagueData[league][0].medals} medals)");
    }
    string GetNextNickname()
{
    if (nicknameIndex >= availableNicknames.Count)
    {
        return "Player" + UnityEngine.Random.Range(1000, 9999); 
    }

    string nickname = availableNicknames[nicknameIndex];
    nicknameIndex++;
    return nickname;
}

    string GetRandomNickname()
    {
        if (availableNicknames.Count == 0)
        {
            return "Player" + UnityEngine.Random.Range(1000, 9999);
        }
        
        // 250 isimden rastgele seç (tekrar kullanılabilir)
        return availableNicknames[UnityEngine.Random.Range(0, availableNicknames.Count)];
    }
    
    void SaveGeneratedPlayers(GeneratedPlayersData data)
    {
        try
        {
            string jsonContent = JsonUtility.ToJson(data, true);
            string filePath = GetPersistentPath(generatedPlayersFileName);
            File.WriteAllText(filePath, jsonContent);
            
            Debug.Log($"✅ Saved {data.players.Count} generated players to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving generated players: {e.Message}");
        }
    }
    
    // Helper methods
    string GetStreamingAssetsPath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, fileName);
    }
    
    string GetPersistentPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    
    int GetBaseMedalsForLeague(LeagueType league)
    {
        switch (league)
        {
            case LeagueType.Bronze: return 0;
            case LeagueType.Silver: return 250;
            case LeagueType.Gold: return 750;
            case LeagueType.Platinum: return 2000;
            case LeagueType.Diamond: return 5000;
            default: return 0;
        }
    }
    
    int GetMedalVariationForLeague(LeagueType league)
    {
        switch (league)
        {
            case LeagueType.Bronze: return 250;
            case LeagueType.Silver: return 500;
            case LeagueType.Gold: return 1250;
            case LeagueType.Platinum: return 3000;
            case LeagueType.Diamond: return 5000;
            default: return 250;
        }
    }
    
    
    void PlacePlayerInCurrentLeague()
    {
        Debug.Log($"--- Placing player '{currentPlayerNickname}' in {currentPlayerLeague} league ---");
        
        // Remove from all leagues
        foreach (var league in leagueData.Keys.ToList())
        {
            leagueData[league].RemoveAll(p => p.isRealPlayer);
        }
        
        // Add to current league
        PlayerData realPlayer = new PlayerData(currentPlayerNickname, currentPlayerMedals, currentPlayerLeague, true);
        leagueData[currentPlayerLeague].Add(realPlayer);
        
        // Re-sort
        leagueData[currentPlayerLeague] = leagueData[currentPlayerLeague].OrderByDescending(p => p.medals).ToList();
        
        AdjustPlayerPosition();
        
        int finalRank = GetPlayerRank(currentPlayerLeague);
        Debug.Log($"Player final rank: {finalRank}");
    }
    
    void AdjustPlayerPosition()
    {
        var currentLeagueList = leagueData[currentPlayerLeague];
        var playerIndex = currentLeagueList.FindIndex(p => p.isRealPlayer);
        
        if (playerIndex == 0) // If player is first
        {
            int targetPosition = UnityEngine.Random.Range(1, Mathf.Min(5, currentLeagueList.Count));
            
            for (int i = 0; i < targetPosition; i++)
            {
                if (!currentLeagueList[i].isRealPlayer)
                {
                    currentLeagueList[i].medals = currentPlayerMedals + UnityEngine.Random.Range(1, 10);
                }
            }
            
            leagueData[currentPlayerLeague] = currentLeagueList.OrderByDescending(p => p.medals).ToList();
        }
    }
    
    public void UpdatePlayerMedals(int newMedals)
    {
        Debug.Log($"*** UPDATING PLAYER MEDALS: {currentPlayerMedals} → {newMedals} ***");
        currentPlayerMedals = newMedals;
        PlacePlayerInCurrentLeague();
        CheckForLeaguePromotion();
        RankingManager.Instance.UpdateLeaderboard(); // Update UI leaderboard
    }
    
    void CheckForLeaguePromotion()
    {
        int playerRank = GetPlayerRank(currentPlayerLeague);
        
        if (playerRank == 1 && currentPlayerLeague != LeagueType.Diamond)
        {
            LeagueType oldLeague = currentPlayerLeague;
            currentPlayerLeague = (LeagueType)((int)currentPlayerLeague + 1);
            
            Debug.Log($"🎉 PROMOTION! {oldLeague} → {currentPlayerLeague}");
            PlacePlayerInCurrentLeague();
            
            OnLeaguePromotion?.Invoke(currentPlayerLeague);
        }
    }
    
    public List<PlayerData> GetLeaderboard(LeagueType league, int count = -1)
    {
        if (count == -1) count = displayedPlayersCount;
        
        if (leagueData.ContainsKey(league))
        {
            return leagueData[league].Take(count).ToList();
        }
        
        return new List<PlayerData>();
    }
    
    public int GetPlayerRank(LeagueType league)
    {
        if (leagueData.ContainsKey(league))
        {
            var playerIndex = leagueData[league].FindIndex(p => p.isRealPlayer);
            return playerIndex + 1;
        }
        return -1;
    }
    

    public System.Action<LeagueType> OnLeaguePromotion;
    
    public string GetLeagueName(LeagueType league)
    {
        return league.ToString();
    }
    
    public Color GetLeagueColor(LeagueType league)
    {
        switch (league)
        {
            case LeagueType.Bronze: return new Color(0.8f, 0.5f, 0.2f);
            case LeagueType.Silver: return new Color(0.75f, 0.75f, 0.75f);
            case LeagueType.Gold: return new Color(1f, 0.84f, 0f);
            case LeagueType.Platinum: return new Color(0.9f, 0.9f, 0.95f);
            case LeagueType.Diamond: return new Color(0.7f, 0.9f, 1f);
            default: return Color.white;
        }
    }
    
    // Test methods
    [ContextMenu("Add 10 Medals")]
    public void TestAddMedals()
    {
        UpdatePlayerMedals(currentPlayerMedals + 10);
    }
    
    [ContextMenu("Print Current Leaderboard")]
    void PrintCurrentLeaderboard()
    {
        var leaderboard = GetLeaderboard(currentPlayerLeague);
        Debug.Log($"=== {GetLeagueName(currentPlayerLeague)} League ===");
        
        for (int i = 0; i < leaderboard.Count; i++)
        {
            string prefix = leaderboard[i].isRealPlayer ? "[YOU] " : "";
            Debug.Log($"{i + 1}. {prefix}{leaderboard[i].nickname} - {leaderboard[i].medals} medals");
        }
    }
    
    [ContextMenu("Regenerate All")]
    void RegenerateAll()
    {
        Debug.Log("🔄 Regenerating everything...");
        usedNicknames.Clear();
        
        // Delete old files
        try
        {
            File.Delete(GetPersistentPath(selectedNicknamesFileName));
            File.Delete(GetPersistentPath(generatedPlayersFileName));
        }
        catch { }
        
        LoadNicknameSystem();
        GenerateAllPlayersAndSave();
        PlacePlayerInCurrentLeague();
        Debug.Log("✅ Everything regenerated!");
    }
}
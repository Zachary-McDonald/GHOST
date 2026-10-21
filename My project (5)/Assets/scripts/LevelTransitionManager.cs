
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionManager : MonoBehaviour
{
    [Header("Next Level")]
    public string nextSceneName = "Level2";
    public float countdownDuration = 10f;

    [Header("Countdown Appearance")]
    public int countdownFontSize = 36;

    private int startingEnemyCount;
    private float countdownTimer;
    private bool countdownStarted;
    private bool levelTransitioned;

    private GUIStyle countdownStyle;

    void Start()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        startingEnemyCount = enemies.Length;

        Debug.Log("Level manager found " + startingEnemyCount + " enemies.");

        if (startingEnemyCount == 0)
        {
            Debug.LogWarning(
                "No enemies found. Make sure enemies have EnemyHealth."
            );
        }
    }

    void Update()
    {
        if (levelTransitioned || startingEnemyCount == 0)
            return;

        EnemyHealth[] remainingEnemies =
            FindObjectsOfType<EnemyHealth>();

        if (!countdownStarted && remainingEnemies.Length == 0)
        {
            countdownStarted = true;
            countdownTimer = countdownDuration;

            Debug.Log("All enemies defeated! Countdown started.");
        }

        if (!countdownStarted)
            return;

        countdownTimer -= Time.deltaTime;

        if (countdownTimer <= 0f)
        {
            LoadNextLevel();
        }
    }

    void OnGUI()
    {
        if (!countdownStarted || levelTransitioned)
            return;

        if (countdownStyle == null)
        {
            countdownStyle = new GUIStyle(GUI.skin.label);
            countdownStyle.fontSize = countdownFontSize;
            countdownStyle.fontStyle = FontStyle.Bold;
            countdownStyle.alignment = TextAnchor.MiddleCenter;
            countdownStyle.normal.textColor = Color.white;
        }

        string message =
            "Next level in: " +
            Mathf.Max(0, Mathf.CeilToInt(countdownTimer));

        float width = 500f;
        float height = 80f;

        Rect position = new Rect(
            (Screen.width - width) / 2f,
            (Screen.height - height) / 2f,
            width,
            height
        );

        GUI.Label(position, message, countdownStyle);
    }

    void LoadNextLevel()
    {
        if (levelTransitioned)
            return;

        levelTransitioned = true;

        Debug.Log("Loading next level: " + nextSceneName);

        SceneManager.LoadScene(nextSceneName);
    }
}

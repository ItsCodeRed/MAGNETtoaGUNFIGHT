using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] int maxEnemies = 6;
    [SerializeField] int timeBetweenEnemies = 30;
    [SerializeField] GameObject firstEnemy;
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] Transform firstEnemySpawn;
    [SerializeField] Transform[] enemySpawns;
    [SerializeField] GameObject menuSong;
    [SerializeField] GameObject battleSong;

    [SerializeField] Transform playerSpawn;
    [SerializeField] GameObject playerPrefab;

    [SerializeField] Leaderboard leaderboard;

    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject deathScreen;
    [SerializeField] TextMeshProUGUI deathScoreText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] Animation mainMenuAnim;
    [SerializeField] Animation cameraAnim;
    [SerializeField] Animation titleTextAnim;
    [SerializeField] Animation namePromptAnim;
    [SerializeField] PointParticle pointParticles;
    [SerializeField] GameObject deathParticles;

    public Player player;

    public int score;

    private float spawnTimer = 0;

    private int numEnemies = 1;

    public List<Enemy> enemies = new List<Enemy>();
    public List<GameObject> throwables = new List<GameObject>();
    private bool isPaused = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Multiple Gamemanagers in scene! Destroying this one....");
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        StartCoroutine(IntroCoroutine());
    }

    IEnumerator IntroCoroutine()
    {
        yield return new WaitForSeconds(3);

        mainMenuAnim.Play("ShowMainMenu");
    }

    public void SpawnEnemy()
    {
        GameObject chosenPrefab = Random.Range(0f, 1f) < 0.3f ? enemyPrefabs[0] : enemyPrefabs[1];
        Transform chosenSpawn = enemySpawns[Random.Range(0, enemySpawns.Length)];

        enemies.Add(Instantiate(chosenPrefab, chosenSpawn.position, chosenSpawn.rotation).GetComponent<Enemy>());
    }

    public void CleanScene()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        enemies.Clear();

        foreach (GameObject throwable in throwables)
        {
            if (throwable != null)
                Destroy(throwable);
        }

        throwables.Clear();
    }

    public void Die()
    {
        CleanScene();
        StartCoroutine(SubmitScoreRoutine());
        battleSong.SetActive(false);
        deathScreen.SetActive(true);
        deathScoreText.text = scoreText.text;
        scoreText.gameObject.SetActive(false);
        player.magnet.grabbedBodies.Clear();
        Instantiate(deathParticles, player.transform.position, Quaternion.identity);
        Destroy(player.gameObject);
    }

    IEnumerator SubmitScoreRoutine()
    {
        bool needUsername = PlayerPrefs.GetString("Username") == "";
        if (needUsername)
        {
            namePromptAnim.Play("ShowNamePrompt");
        }

        yield return new WaitWhile(() => PlayerPrefs.GetString("Username") == "");

        yield return leaderboard.SubmitScoreRoutine(score);
    }

    public void HideNamePrompt()
    {
        namePromptAnim.Play("HideNamePrompt");
    }

    IEnumerator TutorialRoutine()
    {
        tutorialText.text = "WASD to MOVE";
        yield return new WaitForSeconds(2);
        tutorialText.text = "HOLD RIGHT CLICK to ATTRACT";
        yield return new WaitForSeconds(2);
        tutorialText.text = "HOLD LEFT CLICK to REPEL";
        yield return new WaitForSeconds(2);
        tutorialText.text = "";
        PlayerPrefs.SetInt("HavePlayed", 1);
        PlayerPrefs.Save();
    }

    IEnumerator BeginGameCoroutine()
    {
        player = Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation).GetComponent<Player>();
        menuSong.SetActive(false);

        yield return new WaitForSeconds(1);

        scoreText.gameObject.SetActive(true);
        scoreText.text = "Score: 0";
        battleSong.SetActive(true);
        player.canMove = true;
        enemies.Add(Instantiate(firstEnemy, firstEnemySpawn.position, firstEnemySpawn.rotation).GetComponent<Enemy>());

        if (PlayerPrefs.GetInt("HavePlayed") != 1)
        {
            yield return TutorialRoutine();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && player != null)
        {
            PauseAndResume();
        }

        if (player != null)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > timeBetweenEnemies && numEnemies < maxEnemies)
            {
                numEnemies++;
                SpawnEnemy();
                spawnTimer = 0;
            }
        }
    }

    public void PauseAndResume()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        pauseScreen.SetActive(isPaused);
    }

    private void ResetGameValues()
    {
        spawnTimer = 0;
        numEnemies = 1;
        score = 0;
    }

    public void RetryGame()
    {
        isPaused = false;
        Time.timeScale = 1;

        CleanScene();
        ResetGameValues();

        if (player != null)
            Destroy(player.gameObject);

        deathScreen.SetActive(false);
        pauseScreen.SetActive(false);
        battleSong.SetActive(false);

        StartCoroutine(BeginGameCoroutine());
    }

    public void BackToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1;

        CleanScene();
        ResetGameValues();
        if (player != null)
            Destroy(player.gameObject);

        scoreText.gameObject.SetActive(false);
        pauseScreen.SetActive(false);
        menuSong.SetActive(true);
        deathScreen.SetActive(false);
        battleSong.SetActive(false);

        cameraAnim.Play("MenuCam");
        titleTextAnim.Play("ShowTitle");
        mainMenuAnim.Play("ShowMainMenu");
    }

    public void BeginGame()
    {
        CleanScene();
        ResetGameValues();

        cameraAnim.Play("GameCam");
        titleTextAnim.Play("HideTitle");
        mainMenuAnim.Play("HideMainMenu");

        StartCoroutine(BeginGameCoroutine());
    }

    public void AddScore(Transform transform, int score)
    {
        PointParticle particles = Instantiate(pointParticles, transform.position, Quaternion.identity);
        particles.SetScoreValue(score);
        this.score += score;
        scoreText.text = "Score: " + this.score;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int commonCount;
        public int fastCount;
        public int tankCount;
    }

    public List<Wave> waveList;
    public Transform[] spawnPoints;
    public GameObject commonEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;

    public Text waveTextUI;
    public Image wavePanel;
    public float timeBetweenWaves = 5f;
    public float fadeDuration = 0.5f;
    public float targetPanelAlpha = 0.85f;

    private int currentWaveIndex = 0;
    private bool waitingForNextWave = false;
    private bool waveInProgress = false;
    private bool gameEnded = false;

    void Start()
    {
        SetAlpha(0f);
        StartCoroutine(StartNextWave());
    }

    void Update()
    {
        if (gameEnded) return;

        if (AllTreesBurnt())
        {
            EndGame("A floresta foi consumida pela chama");
            return;
        }

        if (waveInProgress &&
            !waitingForNextWave &&
            Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0)
        {
            waitingForNextWave = true;
            StartCoroutine(WaitAndStartNextWave());
        }
    }

    IEnumerator WaitAndStartNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWaveIndex++;
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waveList.Count)
        {
            if (!AllTreesBurnt())
                EndGame("Voce conseguiu proteger a floresta");
            yield break;
        }

        Wave currentWave = waveList[currentWaveIndex];

        waveTextUI.text = $"Wave {currentWaveIndex + 1}";
        waveTextUI.color = Color.white;

        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeOut());

        SpawnEnemies(currentWave);

        waveInProgress = true;
        waitingForNextWave = false;
    }

    void SpawnEnemies(Wave wave)
    {
        for (int i = 0; i < wave.commonCount; i++)
            Instantiate(commonEnemyPrefab, GetRandomSpawnPoint().position, Quaternion.identity);

        for (int i = 0; i < wave.fastCount; i++)
            Instantiate(fastEnemyPrefab, GetRandomSpawnPoint().position, Quaternion.identity);

        for (int i = 0; i < wave.tankCount; i++)
            Instantiate(tankEnemyPrefab, GetRandomSpawnPoint().position, Quaternion.identity);
    }

    Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    bool AllTreesBurnt()
    {
        Tree[] trees = GameObject.FindGameObjectsWithTag("Tree")
                                 .Select(go => go.GetComponent<Tree>())
                                 .Where(tree => tree != null)
                                 .ToArray();

        if (trees.Length == 0) return false;

        return trees.All(tree => tree.IsBurnt());
    }

    void EndGame(string message)
    {
        gameEnded = true;
        waveTextUI.text = message;
        waveTextUI.color = message.Contains("consumida") ? Color.red : Color.green;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float textAlpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            float panelAlpha = Mathf.Lerp(0f, targetPanelAlpha, t / fadeDuration);
            SetAlpha(textAlpha, panelAlpha);
            yield return null;
        }
        SetAlpha(1f, targetPanelAlpha);
    }

    IEnumerator FadeOut()
    {
        if (gameEnded) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float textAlpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            float panelAlpha = Mathf.Lerp(targetPanelAlpha, 0f, t / fadeDuration);
            SetAlpha(textAlpha, panelAlpha);
            yield return null;
        }
        SetAlpha(0f, 0f);
    }

    void SetAlpha(float textAlpha)
    {
        SetAlpha(textAlpha, 0f);
    }

    void SetAlpha(float textAlpha, float panelAlpha)
    {
        Color panelColor = wavePanel.color;
        panelColor.a = panelAlpha;
        wavePanel.color = panelColor;

        Color textColor = waveTextUI.color;
        textColor.a = textAlpha;
        waveTextUI.color = textColor;
    }
}

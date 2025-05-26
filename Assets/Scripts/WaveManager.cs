using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    public OakTutorial oakTutorial;

    private int currentWaveIndex = 0;
    private bool waitingForNextWave = false;
    private bool waveInProgress = false;
    private bool gameEnded = false;
    private bool wavesStarted = false;
    private bool waitingForTreeHealing = false;
    private bool treeHealMessageShown = false;

    void Start()
    {
        SetAlpha(0f, 0f);
    }

    void Update()
    {
        if (gameEnded) return;

        if (!wavesStarted)
        {
            if (oakTutorial != null && oakTutorial.finishedTutorial)
            {
                wavesStarted = true;
                StartCoroutine(StartNextWave());
            }
            return;
        }

        if (AllTreesBurnt())
        {
            EndGame("A floresta foi consumida pela chama", Color.red);
            return;
        }

        if (waitingForTreeHealing)
        {
            if (AllTreesHealed())
            {
                EndGame("Voce conseguiu proteger a floresta", Color.green);
            }
            return;
        }

        if (waveInProgress && !waitingForNextWave && Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0)
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
            if (AllTreesHealed())
            {
                EndGame("Voce conseguiu proteger a floresta", Color.green);
            }
            else
            {
                if (!treeHealMessageShown)
                {
                    treeHealMessageShown = true;
                    StartCoroutine(ShowTreeHealMessage());
                }
                waitingForTreeHealing = true;
            }
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

    IEnumerator ShowTreeHealMessage()
    {
        waveTextUI.text = "Cure as arvores";
        waveTextUI.color = Color.yellow;

        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(FadeOut());
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

    bool AllTreesHealed()
    {
        Tree[] trees = GameObject.FindGameObjectsWithTag("Tree")
            .Select(go => go.GetComponent<Tree>())
            .Where(tree => tree != null)
            .ToArray();

        if (trees.Length == 0) return false;

        return trees.All(tree => !tree.IsBurnt());
    }

    void EndGame(string message, Color color)
    {
        if (gameEnded) return;

        gameEnded = true;

        waveTextUI.text = message;
        waveTextUI.color = new Color(color.r, color.g, color.b, 0f);

        StartCoroutine(ShowEndMessageAndRestart());
    }

    IEnumerator ShowEndMessageAndRestart()
    {
        yield return StartCoroutine(FadeIn());

        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alphaText = Mathf.Lerp(0f, 1f, t / fadeDuration);
            float alphaPanel = Mathf.Lerp(0f, targetPanelAlpha, t / fadeDuration);

            Color textColor = waveTextUI.color;
            waveTextUI.color = new Color(textColor.r, textColor.g, textColor.b, alphaText);

            SetAlphaPanel(alphaPanel);

            yield return null;
        }

        Color finalTextColor = waveTextUI.color;
        waveTextUI.color = new Color(finalTextColor.r, finalTextColor.g, finalTextColor.b, 1f);
        SetAlphaPanel(targetPanelAlpha);
    }

    IEnumerator FadeOut()
    {
        if (gameEnded) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alphaText = Mathf.Lerp(1f, 0f, t / fadeDuration);
            float alphaPanel = Mathf.Lerp(targetPanelAlpha, 0f, t / fadeDuration);
            SetAlpha(alphaText, alphaPanel);
            yield return null;
        }
        SetAlpha(0f, 0f);
    }

    void SetAlpha(float textAlpha, float panelAlpha)
    {
        SetAlphaPanel(panelAlpha);

        Color textColor = waveTextUI.color;
        waveTextUI.color = new Color(textColor.r, textColor.g, textColor.b, textAlpha);
    }

    void SetAlphaPanel(float panelAlpha)
    {
        Color panelColor = wavePanel.color;
        panelColor.a = panelAlpha;
        wavePanel.color = panelColor;
    }

    public void ShowDeathMessage()
    {
        waveTextUI.text = "Voce morreu";
        waveTextUI.color = Color.red;
        StartCoroutine(ShowEndMessageAndRestart());
    }
}

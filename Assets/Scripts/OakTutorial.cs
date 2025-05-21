using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OakTutorial : Tree
{
    public GameObject waveManager;
    public GameObject textToDisable1;
    public GameObject textToDisable2;
    public GameObject textToEnable;
    public GameObject panelToFade;

    public bool finishedTutorial { get; private set; } = false;


    void Update()
    {
        if (health <= 0f && !isBurnt)
        {
            if (fireArea != null)
            {
                Instantiate(fireArea, transform.position, Quaternion.identity);
            }
            BurnTree();
        }

        if (isBurnt && health >= 100f)
        {
            ReviveTree();
        }

        if (isHealing)
        {
            if (healingEffect != null && !healingEffect.isPlaying)
            {
                healingEffect.Play();
            }
        }
        else
        {
            if (healingEffect != null && healingEffect.isPlaying)
            {
                healingEffect.Stop();
            }
        }

        if (!finishedTutorial && !IsBurnt() && health >= 100f)
        {
            finishedTutorial = true;
            StartCoroutine(HandleTutorialEvents());
        }
    }

    IEnumerator HandleTutorialEvents()
    {
        Coroutine fade1 = null;
        Coroutine fade2 = null;

        if (textToDisable1 != null) fade1 = StartCoroutine(FadeOutCanvasGroup(textToDisable1));
        if (textToDisable2 != null) fade2 = StartCoroutine(FadeOutCanvasGroup(textToDisable2));

        if (fade1 != null) yield return fade1;
        if (fade2 != null) yield return fade2;

        if (textToEnable != null)
        {
            CanvasGroup group = textToEnable.GetComponent<CanvasGroup>();
            if (group == null) group = textToEnable.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            textToEnable.SetActive(true);

            float durationIn = 1f;
            float elapsedIn = 0f;

            while (elapsedIn < durationIn)
            {
                elapsedIn += Time.deltaTime;
                group.alpha = Mathf.Lerp(0f, 1f, elapsedIn / durationIn);
                yield return null;
            }

            group.alpha = 1f;
        }

        yield return new WaitForSeconds(10f);

        if (textToEnable != null) yield return StartCoroutine(FadeOutCanvasGroup(textToEnable));
        if (panelToFade != null) yield return StartCoroutine(FadeOutCanvasGroup(panelToFade));
        if (waveManager != null) waveManager.SetActive(true);
    }

    IEnumerator FadeOutCanvasGroup(GameObject obj)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = obj.AddComponent<CanvasGroup>();
            group.alpha = 1f;
        }

        float duration = 1.5f;
        float elapsed = 0f;
        float startAlpha = group.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        group.alpha = 0f;
        obj.SetActive(false);
    }
}

using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public Camera introCamera;
    public GameObject player;
    public GameObject canvasMenu;
    public GameObject canvasHUD;
    public Transform cameraTargetPosition;

    public GameObject panelTutorial;
    public GameObject textoTutorial1;
    public GameObject textoTutorial2;

    public float transitionSpeed = 2f;
    private bool isTransitioning = false;

    private CanvasGroup hudCanvasGroup;
    private CanvasGroup panelGroup;
    private CanvasGroup texto1Group;
    private CanvasGroup texto2Group;

    void Start()
    {
        introCamera.gameObject.SetActive(true);
        player.SetActive(false);
        canvasMenu.SetActive(true);
        canvasHUD.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        hudCanvasGroup = GetOrAddCanvasGroup(canvasHUD);
        hudCanvasGroup.alpha = 0f;

        panelGroup = GetOrAddCanvasGroup(panelTutorial);
        texto1Group = GetOrAddCanvasGroup(textoTutorial1);
        texto2Group = GetOrAddCanvasGroup(textoTutorial2);

        panelTutorial.SetActive(false);
        textoTutorial1.SetActive(false);
        textoTutorial2.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnPlayClicked()
    {
        isTransitioning = true;
        canvasMenu.SetActive(false);

        panelTutorial.SetActive(true);
        textoTutorial1.SetActive(true);
        textoTutorial2.SetActive(true);

        panelGroup.alpha = 0f;
        texto1Group.alpha = 0f;
        texto2Group.alpha = 0f;

        StartCoroutine(FadeIn(panelGroup));
        StartCoroutine(FadeIn(texto1Group));
        StartCoroutine(FadeIn(texto2Group));
    }

    void Update()
    {
        if (isTransitioning)
        {
            introCamera.transform.position = Vector3.Lerp(introCamera.transform.position, cameraTargetPosition.position, Time.deltaTime * transitionSpeed);

            float distance = Vector3.Distance(introCamera.transform.position, cameraTargetPosition.position);
            if (distance < 0.1f)
            {
                FinishTransition();
            }
        }
    }

    void FinishTransition()
    {
        isTransitioning = false;

        player.SetActive(true);
        introCamera.gameObject.SetActive(false);

        canvasHUD.SetActive(true);
        StartCoroutine(FadeIn(hudCanvasGroup));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator FadeIn(CanvasGroup group)
    {
        float duration = 1.5f;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            group.alpha = Mathf.Clamp01(currentTime / duration);
            yield return null;
        }

        group.alpha = 1f;
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }
}

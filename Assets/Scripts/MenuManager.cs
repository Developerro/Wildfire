using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public Camera introCamera;
    public GameObject player;
    public GameObject canvasMenu;
    public GameObject canvasHUD;
    public Transform cameraTargetPosition;

    public float transitionSpeed = 2f;
    private bool isTransitioning = false;

    private CanvasGroup hudCanvasGroup;

    void Start()
    {
        introCamera.gameObject.SetActive(true);
        player.SetActive(false);
        canvasMenu.SetActive(true);
        canvasHUD.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        hudCanvasGroup = canvasHUD.GetComponent<CanvasGroup>();
        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 0f;
        }
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
        StartCoroutine(FadeInHUD());

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator FadeInHUD()
    {
        float duration = 2f; 
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(currentTime / duration);
            if (hudCanvasGroup != null)
                hudCanvasGroup.alpha = alpha;

            yield return null;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class BowScript : MonoBehaviour
{
    float _charge;

    public float chargeMax;
    public float chargeRate;
    public float maxPullback = 0.5f;

    public KeyCode fireButton;

    public Transform spawn;
    public Rigidbody arrowObj;

    public RawImage aim;
    public Image reloadBar;

    public Animator animator;

    public AudioSource releaseAudio;

    public Player player;

    private Vector2 originalAimSize;

    private bool canShoot = true;
    private float cooldown = 0.2f;
    private float cooldownTimer = 0f;

    private bool isCharging = false;
    private float pullDuration = 0.6667f;
    private float animTime = 0f;

    private Rigidbody currentArrow;

    void Start()
    {
        originalAimSize = aim.rectTransform.sizeDelta;
        reloadBar.fillAmount = 0f;
    }

    void Update()
    {
        if (!canShoot)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canShoot = true;
                cooldownTimer = 0f;
                reloadBar.fillAmount = 0f;
            }
            else
            {
                reloadBar.fillAmount = cooldownTimer / cooldown;
            }
        }

        bool canUseBow = canShoot && !player.IsUsingMana && !player.IsCastingUlt;

        if (canUseBow)
        {
            if (Input.GetKeyDown(fireButton))
            {
                isCharging = true;
                _charge = 0f;
                animTime = 0f;

                animator.Play("Shoot", 0, 0f);
                animator.speed = 0f;

                currentArrow = Instantiate(arrowObj, spawn.position, spawn.rotation);
                currentArrow.isKinematic = true;
            }

            if (Input.GetKey(fireButton) && isCharging)
            {
                if (_charge < chargeMax)
                    _charge += Time.deltaTime * chargeRate;

                float chargePercent = Mathf.Clamp01(_charge / chargeMax);

                animTime += Time.deltaTime * chargeRate / chargeMax * pullDuration;
                animTime = Mathf.Clamp(animTime, 0f, pullDuration);

                animator.Play("Shoot", 0, animTime);
                animator.speed = 0f;

                float scaleFactor = Mathf.Lerp(1f, 0.5f, chargePercent);
                aim.rectTransform.sizeDelta = originalAimSize * scaleFactor;

                if (currentArrow)
                {
                    Vector3 pullOffset = -spawn.forward * chargePercent * maxPullback;
                    currentArrow.transform.position = spawn.position + pullOffset;
                    currentArrow.transform.rotation = spawn.rotation * Quaternion.Euler(-90, 0, 0);
                }
            }

            if (Input.GetKeyUp(fireButton) && isCharging)
            {
                isCharging = false;

                if (currentArrow)
                {
                    currentArrow.isKinematic = false;
                    currentArrow.transform.position = spawn.position;
                    currentArrow.transform.rotation = spawn.rotation * Quaternion.Euler(-90, 0, 0);
                    currentArrow.AddForce(spawn.forward * _charge, ForceMode.Impulse);
                    Destroy(currentArrow.gameObject, 5f);
                    currentArrow = null;

                    if (releaseAudio != null)
                        releaseAudio.Play();
                }

                canShoot = false;
                cooldownTimer = cooldown;
                reloadBar.fillAmount = 1f;

                _charge = 0f;
                animTime = 0f;
                aim.rectTransform.sizeDelta = originalAimSize;

                animator.Play("Shoot", 0, pullDuration);
                animator.speed = 1f;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    public bool isHealing = false;

    public virtual void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }


    public virtual void TakeDamage(float amount)
    {
        if (health >= 0)
        {
            health -= amount;
        }
    }
}


public class Player : Health
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    public GameObject rightArm;
    public GameObject leftArm;
    public Healing healingArea;

    public GameObject leftArmGlowPart;
    public Material glowMaterial;
    public Material originalMaterial;
    public float timeToGlow = 0.5f;
    public float glowDuration = 1f;

    public Image redOverlay;
    public float overlayDuration = 0.5f;

    public float maxMana = 100f;
    public float currentMana = 100f;
    public float manaRegenDelay = 3f;
    public float manaRegenDuration = 3f;
    public float manaCostPerSecond = 5f;
    public float launchForceUlt = 20f;
    public Image flashOverlay;
    public GameObject ultimatePrefab;
    public Transform ultimateSpawnPoint;

    public AudioSource footstepSource;
    public AudioClip walkClip;
    public AudioClip runClip;

    private float manaRegenTimer = 0f;
    private bool isUsingMana = false;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;

    private float glowTimer = 0f;
    private bool glowActivated = false;
    private Renderer glowRenderer;
    private Material glowInstance;
    private Color currentEmission;
    private Color targetEmission = new Color(88f / 255f, 191f / 255f, 0f / 255f);

    private Vector3 cameraOriginalPos;
    private float shakeTimer = 0f;
    private float shakeDuration = 0.15f;
    private float shakeMagnitude = 0.05f;
    private float smoothShakeSpeed = 10f;
    private float overlayTimer = 0f;

    public WaveManager waveManager;

    private Vector3 rightArmStartPos = new Vector3(-0.1f, -5.6f, -3f);
    private Quaternion rightArmStartRot = Quaternion.Euler(3.54f, -85.94f, -0.77f);
    private Vector3 rightArmEndPos = new Vector3(-0.03f, -0.72f, 1.67f);
    private Quaternion rightArmEndRot = Quaternion.Euler(9.69f, -177.6f, 15.65f);

    private bool isDead = false;
    private Quaternion deathRotation;

    private bool isCastingUlt = false;
    private float ultCastDuration = 1.5f;
    private float ultCastTimer = 0f;
    private bool hasThrownUlt = false;
    private float ultCooldownTimer = 0f;

    public bool IsUsingMana { get { return isUsingMana; } }
    public bool IsCastingUlt { get { return isCastingUlt; } }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (leftArm != null)
            leftArm.SetActive(true);

        glowInstance = new Material(glowMaterial);
        glowInstance.EnableKeyword("_EMISSION");
        glowInstance.SetColor("_EmissionColor", Color.black);

        if (leftArmGlowPart != null)
        {
            glowRenderer = leftArmGlowPart.GetComponent<Renderer>();
            glowInstance = new Material(glowMaterial);
            glowInstance.EnableKeyword("_EMISSION");
            glowRenderer.material = originalMaterial;
        }

        if (playerCamera != null)
            cameraOriginalPos = playerCamera.transform.localPosition;

        if (rightArm != null)
        {
            rightArm.SetActive(true);
            rightArm.transform.localPosition = rightArmStartPos;
            rightArm.transform.localRotation = rightArmStartRot;
            StartCoroutine(RaiseRightArm());
        }

        if (footstepSource != null)
        {
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
        }
    }

    IEnumerator RaiseRightArm()
    {
        float duration = 1f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            rightArm.transform.localPosition = Vector3.Lerp(rightArmStartPos, rightArmEndPos, progress);
            rightArm.transform.localRotation = Quaternion.Lerp(rightArmStartRot, rightArmEndRot, progress);
            yield return null;
        }
    }

    public void DisableFlash()
    {
        if (flashOverlay != null)
        {
            flashOverlay.color = Color.clear;
        }
    }

    public void EnableFlash()
    {
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(0f, 1f, 4f / 255f, 194f / 255f);
        }
    }

    IEnumerator ClearFlash()
    {
        yield return null;
        flashOverlay.color = Color.clear;
    }

    void Update()
    {
        if (isDead)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, deathRotation, Time.deltaTime * 2f);
            return;
        }

        if(health >= maxHealth)
        {
            health = maxHealth;
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        if (ultCooldownTimer > 0f)
            ultCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && !isCastingUlt && currentMana >= maxMana)
        {
            isCastingUlt = true;
            ultCastTimer = ultCastDuration;
            hasThrownUlt = false;
            currentMana = 0f;
            ultCooldownTimer = 10f;
        }

        HandleRightArm();
        HandleLeftArm();
        HandleManaRegen();
        HandleCameraShake();
        HandleRedOverlay();
        HandleUltimate();
        HandleFootsteps(characterController.velocity.magnitude > 0.1f, isRunning);
    }

    void HandleRightArm()
    {
        if (rightArm != null)
        {
            Vector3 startPos = rightArmEndPos;
            Vector3 endPos = rightArmStartPos;
            Quaternion startRot = rightArmEndRot;
            Quaternion endRot = rightArmStartRot;
            float speed = 5f;

            bool isRightArmDown = (Input.GetMouseButton(1) && currentMana > 0f) || isCastingUlt;

            if (isRightArmDown)
            {
                isUsingMana = Input.GetMouseButton(1) && currentMana > 0f;
                if (isUsingMana)
                {
                    manaRegenTimer = manaRegenDelay;
                    currentMana -= manaCostPerSecond * Time.deltaTime;
                    currentMana = Mathf.Max(currentMana, 0f);
                }

                healingArea.gameObject.SetActive(true);
                rightArm.transform.localPosition = Vector3.Lerp(rightArm.transform.localPosition, endPos, Time.deltaTime * speed);
                rightArm.transform.localRotation = Quaternion.Lerp(rightArm.transform.localRotation, endRot, Time.deltaTime * speed);
            }
            else
            {
                isUsingMana = false;
                healingArea.gameObject.SetActive(false);
                rightArm.transform.localPosition = Vector3.Lerp(rightArm.transform.localPosition, startPos, Time.deltaTime * speed);
                rightArm.transform.localRotation = Quaternion.Lerp(rightArm.transform.localRotation, startRot, Time.deltaTime * speed);
            }
        }
    }

    void HandleLeftArm()
    {
        if (leftArm != null)
        {
            leftArm.SetActive(true);
            Vector3 startPos = new Vector3(-1.313309f, -2.882696f, -5.200942f);
            Vector3 endPos = new Vector3(-0.950336f, -5.801865f, -2.641529f);
            Quaternion startRot = Quaternion.Euler(3.543f, -85.937f, -33.969f);
            Quaternion endRot = Quaternion.Euler(3.543f, -85.937f, 3.077f);
            float speed = 5f;

            bool castingOrHealing = (Input.GetMouseButton(1) && currentMana > 0f) || isCastingUlt;

            if (castingOrHealing)
            {
                leftArm.transform.localPosition = Vector3.Lerp(leftArm.transform.localPosition, endPos, Time.deltaTime * speed);
                leftArm.transform.localRotation = Quaternion.Lerp(leftArm.transform.localRotation, endRot, Time.deltaTime * speed);

                glowTimer += Time.deltaTime;
                if (glowTimer >= timeToGlow && !glowActivated)
                {
                    glowActivated = true;
                    if (glowRenderer != null)
                        glowRenderer.material = glowInstance;
                    currentEmission = Color.black;
                }

                if (glowActivated)
                {
                    Color finalEmission = Color.Lerp(currentEmission, targetEmission, Time.deltaTime / glowDuration);
                    glowInstance.SetColor("_EmissionColor", finalEmission * 6f);
                    currentEmission = finalEmission;

                    float shakeX = Mathf.Sin(Time.time * 60f) * 0.0007f;
                    float shakeY = Mathf.Sin(Time.time * 90f) * 0.0007f;
                    Vector3 shakeOffset = new Vector3(shakeX, shakeY, 0f);
                    leftArm.transform.localPosition += shakeOffset;
                }

                healingArea.gameObject.SetActive(Input.GetMouseButton(1));
            }
            else
            {
                glowTimer = 0f;
                glowActivated = false;
                leftArm.transform.localPosition = Vector3.Lerp(leftArm.transform.localPosition, startPos, Time.deltaTime * speed);
                leftArm.transform.localRotation = Quaternion.Lerp(leftArm.transform.localRotation, startRot, Time.deltaTime * speed);
                healingArea.gameObject.SetActive(false);
                if (glowRenderer != null)
                    glowRenderer.material = originalMaterial;
            }
        }
    }

    void HandleManaRegen()
    {
        if (!isUsingMana && ultCooldownTimer <= 0f)
        {
            if (manaRegenTimer > 0f)
            {
                manaRegenTimer -= Time.deltaTime;
            }
            else if (currentMana < maxMana)
            {
                float regenRate = maxMana / manaRegenDuration;
                currentMana += regenRate * Time.deltaTime;
                currentMana = Mathf.Min(currentMana, maxMana);
            }
        }
    }

    void HandleCameraShake()
    {
        if (shakeTimer > 0)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;
            Vector3 targetPos = cameraOriginalPos + randomOffset;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPos, Time.deltaTime * smoothShakeSpeed);
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraOriginalPos, Time.deltaTime * smoothShakeSpeed);
        }
    }

    void HandleRedOverlay()
    {
        if (overlayTimer > 0)
        {
            overlayTimer -= Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.4f, overlayTimer / overlayDuration);
            redOverlay.color = new Color(1f, 0f, 0f, alpha);
        }
        else
        {
            redOverlay.color = Color.clear;
        }
    }

    void HandleUltimate()
    {
        if (isCastingUlt)
        {
            ultCastTimer -= Time.deltaTime;

            if (ultCastTimer <= 0f && !hasThrownUlt)
            {
                if (ultimatePrefab != null && ultimateSpawnPoint != null)
                {
                    GameObject ult = Instantiate(ultimatePrefab, ultimateSpawnPoint.position, playerCamera.transform.rotation);
                    Rigidbody rb = ult.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(playerCamera.transform.forward * launchForceUlt, ForceMode.Impulse);
                    }
                }
                hasThrownUlt = true;
            }

            if (hasThrownUlt && ultCastTimer <= -0.5f)
            {
                isCastingUlt = false;
            }
        }
    }

    void HandleFootsteps(bool isMoving, bool isRunning)
    {
        if (characterController.isGrounded && isMoving)
        {
            AudioClip targetClip = isRunning ? runClip : walkClip;

            if (footstepSource.clip != targetClip)
            {
                footstepSource.clip = targetClip;
                footstepSource.Play();
            }
            else if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            footstepSource.Stop();
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        shakeTimer = shakeDuration;
        overlayTimer = overlayDuration;

        if (health <= 0f && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        canMove = false;
        deathRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 90f);
        redOverlay.color = new Color(1f, 0f, 0f, 0.4f);
        overlayTimer = overlayDuration;
        waveManager.ShowDeathMessage();
    }

    public override void Heal(float amount)
    {
        base.Heal(amount);
    }
}

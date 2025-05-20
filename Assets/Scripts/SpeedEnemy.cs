using UnityEngine;
using System.Collections;

public class SpeedEnemy : Enemy
{
    [Header("Weapon Settings - Speed Enemy")]
    public Transform weaponTransform;

    private Vector3 idlePosition = new Vector3(-0.0034f, -1.6599f, 0.4688f);
    private Vector3 idleRotation = Vector3.zero;

    private Vector3 attackPosition = new Vector3(0.2545f, -1.588f, 0.5901f);
    private Vector3 attackRotation = new Vector3(0f, 70.476f, 0f);

    private Vector3 hitPosition = new Vector3(-0.0034f, -1.44f, 0.4688f);
    private Vector3 hitRotation = Vector3.zero;

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private float transitionSpeed = 5f;

    private bool isInHitState = false;

    private float swayAmplitude = 0.02f; 
    private float swayFrequency = 2f;    

    protected override void Start()
    {
        base.Start();

        if (weaponTransform != null)
        {
            targetPosition = idlePosition;
            targetRotation = idleRotation;
            weaponTransform.localPosition = idlePosition;
            weaponTransform.localRotation = Quaternion.Euler(idleRotation);
        }
    }

    protected override void Update()
    {
        base.Update();
        UpdateWeaponTransform();
    }

    private void UpdateWeaponTransform()
    {
        if (weaponTransform == null) return;

        Vector3 currentTargetPos = targetPosition;

        if (!isInHitState && targetPosition == idlePosition)
        {
            float swayOffset = Mathf.Sin(Time.time * swayFrequency) * swayAmplitude;
            currentTargetPos.x += swayOffset;
        }

        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, currentTargetPos, Time.deltaTime * transitionSpeed);
        weaponTransform.localRotation = Quaternion.Lerp(weaponTransform.localRotation, Quaternion.Euler(targetRotation), Time.deltaTime * transitionSpeed);
    }

    protected override void SetFlameActive(bool active)
    {
        base.SetFlameActive(active);

        if (isInHitState) return;

        if (active)
        {
            targetPosition = attackPosition;
            targetRotation = attackRotation;
        }
        else
        {
            targetPosition = idlePosition;
            targetRotation = idleRotation;
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        StartCoroutine(ShowHitPose());
    }

    private IEnumerator ShowHitPose()
    {
        isInHitState = true;

        targetPosition = hitPosition;
        targetRotation = hitRotation;

        yield return new WaitForSeconds(0.2f);

        isInHitState = false;

        SetFlameActive(flameObject != null && flameObject.activeSelf);
    }
}

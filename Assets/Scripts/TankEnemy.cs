using UnityEngine;
using System.Collections;

public class TankEnemy : Enemy
{
    [Header("Weapon Settings - Tank Enemy")]
    public Transform weaponTransform;

    private Vector3 idlePosition = new Vector3(0.612f, -3.502f, 0.202f);
    private Vector3 idleRotation = new Vector3(-1.875f, 8.604f, 8.283f);

    private Vector3 attackPosition = new Vector3(0.415f, -3.262f, 1.379f);
    private Vector3 attackRotation = new Vector3(0.701f, 71.27f, -2.472f);

    private Vector3 hitPosition = new Vector3(0.569f, -3.106f, 0.295f);
    private Vector3 hitRotation = new Vector3(-1.875f, 8.604f, 8.283f);

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private float transitionSpeed = 5f;

    private bool isInHitState = false;

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

        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
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

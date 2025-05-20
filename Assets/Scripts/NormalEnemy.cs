using UnityEngine;
using System.Collections;

public class NormalEnemy : Enemy
{
    [Header("Weapon Settings - Normal Enemy")]
    public Transform weaponTransform;

    private Vector3 idlePosition = new Vector3(0.22f, -3.2399f, 1.265461f);
    private Vector3 idleRotation = new Vector3(-0.81f, 5.12f, 8.971f);

    private Vector3 attackPosition = new Vector3(1.55f, -3.043f, 3.161f);
    private Vector3 attackRotation = new Vector3(-8.986f, 86.041f, 0.619f);

    private Vector3 hitPosition = new Vector3(0.22f, -2.54f, 1.265461f);
    private Vector3 hitRotation = new Vector3(-0.81f, 5.12f, 8.971f); 

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

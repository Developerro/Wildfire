using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BowData bowData;
    [SerializeField] private Transform cam;
    

    float timeSinceLastShot;

    private void Start()
    {
        PlayerShoot.shootInput += Shoot;
    }

    private bool CanShoot() => !bowData.reloading && timeSinceLastShot > 1f / (bowData.fireRate / 60f);
    private void Shoot() {
        if (bowData.currentAmmo > 0) {
            if (CanShoot()) {
                if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, bowData.maxDistance)){
                    //IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
                    //damageable?.TakeDamage(bowData.damage);
                }

                bowData.currentAmmo--;
                timeSinceLastShot = 0;
                OnWeaponShot();
            }
        }
    }

    private void OnWeaponShot() {  }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(cam.position, cam.forward * bowData.maxDistance);
    }


}

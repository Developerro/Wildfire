using UnityEngine;

[CreateAssetMenu(fileName="Weapon", menuName="Bow/Weapon")]
public class BowData : ScriptableObject
{
    [Header("Info")]
    public new string name;

    [Header("Shooting")]
    public float damage;
    public float maxDistance;

    [Header("Reloading")]
    public int currentAmmo;
    public int magSize;
    public float fireRate;
    public float reloadTime;
    [HideInInspector]
    public bool reloading;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

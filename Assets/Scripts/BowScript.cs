using UnityEngine;
using UnityEngine.Rendering;

public class BowScript : MonoBehaviour
{
    float _charge;

    public float chargeMax;
    public float chargeRate;

    public KeyCode fireButton;

    public Transform spawn;
    public Rigidbody arrowObj;
    void Start()
    {

    }


    void Update()
    {
        if (Input.GetKey(fireButton) && _charge < chargeMax)
        {
            _charge += Time.deltaTime * chargeRate;
            Debug.Log(_charge.ToString());
        }

        if (Input.GetKeyUp(fireButton))
        {
            Rigidbody arrow = Instantiate(arrowObj, spawn.position, spawn.rotation * Quaternion.Euler(-90, 0, 0)) as Rigidbody;
            arrow.AddForce(spawn.forward * _charge, ForceMode.Impulse);
            _charge = 0;
        }
    }
}

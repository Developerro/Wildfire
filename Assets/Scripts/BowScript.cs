using UnityEngine;
using UnityEngine.UI;

public class BowScript : MonoBehaviour
{
    float _charge;

    public float chargeMax;
    public float chargeRate;

    public KeyCode fireButton;

    public Transform spawn;
    public Rigidbody arrowObj;

    public RawImage aim;

    private Vector2 originalAimSize;

    void Start()
    {
        originalAimSize = aim.rectTransform.sizeDelta;
    }

    void Update()
    {
        if (Input.GetKey(fireButton) && _charge < chargeMax)
        {
            _charge += Time.deltaTime * chargeRate;

            float scaleFactor = Mathf.Lerp(1f, 0.5f, _charge / chargeMax);
            aim.rectTransform.sizeDelta = originalAimSize * scaleFactor;
        }

        if (Input.GetKeyUp(fireButton))
        {
            Rigidbody arrow = Instantiate(arrowObj, spawn.position, spawn.rotation * Quaternion.Euler(-90, 0, 0)) as Rigidbody;
            arrow.AddForce(spawn.forward * _charge, ForceMode.Impulse);

            Destroy(arrow.gameObject, 5f);

            _charge = 0;
            aim.rectTransform.sizeDelta = originalAimSize;
        }
    }
}

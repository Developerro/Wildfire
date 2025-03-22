using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public int damage;

    private void OnTriggerEnter(Collider col)
    {
        if (Input.GetMouseButtonDown(0))
        {
            //if (col.GetComponent<EnemyStats>())
            //{
            //    EnemyStats stats = col.GetComponent<EnemyStats>();
            //    stats.Hit(damage);
            //}
        }
    }
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

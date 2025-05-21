using UnityEngine;

public class Tree : Health
{
    public GameObject normalModel;
    public GameObject burntModel;
    public FireArea fireArea;
    public ParticleSystem healingEffect;

    public bool isBurnt = false;

    void Update()
    {
        if (health <= 0f && !isBurnt)
        {
            if (fireArea != null)
            {
                Instantiate(fireArea, transform.position, Quaternion.identity);
            }
            BurnTree();
        }

        if (isBurnt && health >= 100f)
        {
            ReviveTree();
        }

        if (isHealing)
        {
            if (healingEffect != null && !healingEffect.isPlaying)
            {
                healingEffect.Play();
            }
        }
        else
        {
            if (healingEffect != null && healingEffect.isPlaying)
            {
                healingEffect.Stop();
            }
        }
    }

    public void BurnTree()
    {
        isBurnt = true;

        if (normalModel != null) normalModel.SetActive(false);
        if (burntModel != null) burntModel.SetActive(true);
    }

    public void ReviveTree()
    {
        isBurnt = false;

        if (normalModel != null) normalModel.SetActive(true);
        if (burntModel != null) burntModel.SetActive(false);
    }

    public bool IsBurnt()
    {
        return isBurnt;
    }
}

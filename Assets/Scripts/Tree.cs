using UnityEngine;

public class Tree : Health
{
    public GameObject normalModel;
    public GameObject burntModel;

    private bool isBurnt = false;

    public void Update()
    {
        if (health <= 0f)
        {
            BurnTree();
        }           
        if (isBurnt && health >= 100f)
        {
            ReviveTree();
        }
    }

    void BurnTree()
    {
        isBurnt = true;

        if (normalModel != null) normalModel.SetActive(false);
        if (burntModel != null) burntModel.SetActive(true);
    }

    void ReviveTree()
    {
        isBurnt = false;

        if (normalModel != null) normalModel.SetActive(true);
        if (burntModel != null) burntModel.SetActive(false);
    }
}
using UnityEngine;

public class FireTutorial : FireScript
{
    public ParticleSystem particle1;
    public ParticleSystem particle2;

    private bool started = false;

    void Update()
    {
        if (!started && (particle1.isPlaying || particle2.isPlaying))
        {
            started = true;
        }

        if (started && !particle1.isPlaying && !particle2.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class EngineSoundBlender : MonoBehaviour
{
    public Powertrain powertrain; // Reference to the Powertrain script

    [Header("Audio Sources")]
    public AudioSource idle;    // AudioSource for idle sound
    public AudioSource med_on;  // AudioSource for mid-range sound
    public AudioSource high_on; // AudioSource for high RPM sound

    [Header("RPM Settings")]
    public float minRPM = 500f;    // Minimum RPM (idle)
    public float maxRPM = 5000f;   // Maximum RPM

    void Update()
    {
        if (powertrain == null) return;

        // Get the current RPM from the Powertrain script
        float rpm = powertrain.GetCurrentRpm();

        // Normalize RPM to a 0-1 range
        float normalizedRPM = Mathf.InverseLerp(minRPM, maxRPM, rpm);

        // Clear overlap and ensure hard muting
        if (normalizedRPM <= 0.5f)
        {
            // Low to mid RPM: blend between idle and mid
            idle.volume = Mathf.Lerp(1.0f, 0.0f, normalizedRPM * 2.0f);
            med_on.volume = Mathf.Lerp(0.0f, 1.0f, normalizedRPM * 2.0f);
            high_on.volume = 0.0f; // Hard mute high
        }
        else
        {
            // Mid to high RPM: blend between mid and high
            idle.volume = 0.0f; // Hard mute idle
            med_on.volume = Mathf.Lerp(1.0f, 0.0f, (normalizedRPM - 0.5f) * 2.0f);
            high_on.volume = Mathf.Lerp(0.0f, 1.0f, (normalizedRPM - 0.5f) * 2.0f);
        }

        // Adjust pitch based on normalized RPM for realism
        float pitch = Mathf.Lerp(1f, 2.5f, normalizedRPM);
        idle.pitch = pitch;
        med_on.pitch = pitch;
        high_on.pitch = pitch;

        // Ensure correct sounds are playing
        if (idle.volume > 0 && !idle.isPlaying) idle.Play();
        if (med_on.volume > 0 && !med_on.isPlaying) med_on.Play();
        if (high_on.volume > 0 && !high_on.isPlaying) high_on.Play();

        // Stop irrelevant sounds
        if (idle.volume == 0 && idle.isPlaying) idle.Stop();
        if (med_on.volume == 0 && med_on.isPlaying) med_on.Stop();
        if (high_on.volume == 0 && high_on.isPlaying) high_on.Stop();
    }
}

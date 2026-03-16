using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class CaneHapticSystem : MonoBehaviour
{
    [Header("Haptic Output")]
    public HapticImpulsePlayer hapticPlayer;

    [Header("Default Haptic Settings")]
    [Range(0f, 1f)]
    public float defaultAmplitude = 0.3f;

    public float defaultDuration = 0.08f;

    [Tooltip("0 = use device default frequency")]
    public float defaultFrequency = 0f;

    public void PlayHapticBySurface(SurfaceType surfaceType)
    {
        float amplitude = defaultAmplitude;
        float duration = defaultDuration;
        float frequency = defaultFrequency;

        switch (surfaceType)
        {
            case SurfaceType.Ground:
                amplitude = 0.2f;
                duration = 0.05f;
                break;

            case SurfaceType.Wall:
                amplitude = 0.6f;
                duration = 0.12f;
                break;

            case SurfaceType.Obstacle:
                amplitude = 0.5f;
                duration = 0.10f;
                break;

            case SurfaceType.Wood:
                amplitude = 0.25f;
                duration = 0.06f;
                break;

            case SurfaceType.Metal:
                amplitude = 0.45f;
                duration = 0.09f;
                break;

            case SurfaceType.TactilePaving:
                amplitude = 0.35f;
                duration = 0.07f;
                break;
        }

        SendHaptic(amplitude, duration, frequency);
    }

    public void SendHaptic(float amplitude, float duration, float frequency = 0f)
    {
        if (hapticPlayer == null)
        {
            Debug.LogWarning("CaneHapticSystem: Haptic Player reference is missing.");
            return;
        }

        bool success = hapticPlayer.SendHapticImpulse(amplitude, duration, frequency);

        if (!success)
        {
            Debug.LogWarning("CaneHapticSystem: Failed to send haptic impulse.");
        }
    }
}
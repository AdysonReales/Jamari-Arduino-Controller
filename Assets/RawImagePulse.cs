using UnityEngine;
using UnityEngine.UI;

public class RawImagePulse : MonoBehaviour
{
    public RawImage image;       // Assign your RawImage
    public float scaleAmount = 1.1f; // How big it gets
    public float speed = 0.48f;     // How fast it pulses

    private Vector3 originalScale;

    void Start()
    {
        originalScale = image.rectTransform.localScale;
    }

    void Update()
    {
        float scale = Mathf.Sin(Time.time * speed) * (scaleAmount - 1f) + 1f;
        image.rectTransform.localScale = originalScale * scale;
    }
}

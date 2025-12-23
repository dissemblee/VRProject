using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
  public Light pointLight;
  public float minIntensity = 1.5f;
  public float maxIntensity = 4f;
  public float flickerSpeed = 0.1f;

  private float timer;
  private float targetIntensity;

  void Start()
  {
    if (pointLight == null)
      pointLight = GetComponent<Light>();
    targetIntensity = pointLight.intensity;
  }

  void Update()
  {
    timer -= Time.deltaTime;
    if (timer <= 0)
    {
      targetIntensity = Random.Range(minIntensity, maxIntensity);
      timer = flickerSpeed;
    }

    pointLight.intensity = Mathf.Lerp(pointLight.intensity, targetIntensity, Time.deltaTime * 10f);
  }
} 
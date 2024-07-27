using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    Light lightSource;
    void Start()
    {
        if (!TryGetComponent<Light>(out lightSource))
        {
            enabled = false;
        }
        sinMultiplier = maxIntensity - minIntensity;
    }
    float sinMultiplier;
    [SerializeField]
    float maxIntensity;
    [SerializeField]

    float period;
    [SerializeField]

    float minIntensity;
    // Update is called once per frame
    static public bool editorPreview = true;

    public Light light;

    public bool loop;

    public bool animateIntensity;
    public float intensityStart = 8f;
    public float intensityEnd = 0f;
    public float intensityDuration = 0.5f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public bool perlinIntensity;
    public float perlinIntensitySpeed = 1f;
    public bool fadeIn;
    public float fadeInDuration = 0.5f;
    public bool fadeOut;
    public float fadeOutDuration = 0.5f;

    public bool animateRange;
    public float rangeStart = 8f;
    public float rangeEnd = 0f;
    public float rangeDuration = 0.5f;
    public AnimationCurve rangeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public bool perlinRange;
    public float perlinRangeSpeed = 1f;

    public bool animateColor;
    public Gradient colorGradient;
    public float colorDuration = 0.5f;
    public AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public bool perlinColor;
    public float perlinColorSpeed = 1f;





    void Update()
    {

        if (animateIntensity)
        {
            float delta = loop ? Mathf.Clamp01((Time.time % intensityDuration) / intensityDuration) : Mathf.Clamp01(Time.time / intensityDuration);
            delta = perlinIntensity ? Mathf.PerlinNoise(Time.time * perlinIntensitySpeed, 0f) : intensityCurve.Evaluate(delta);
            light.intensity = Mathf.LerpUnclamped(intensityEnd, intensityStart, delta);

            if (fadeIn && Time.time < fadeInDuration)
            {
                light.intensity *= Mathf.Clamp01(Time.time / fadeInDuration);
            }
        }

        if (animateRange)
        {
            float delta = loop ? Mathf.Clamp01((Time.time % rangeDuration) / rangeDuration) : Mathf.Clamp01(Time.time / rangeDuration);
            delta = perlinRange ? Mathf.PerlinNoise(Time.time * perlinRangeSpeed, 10f) : rangeCurve.Evaluate(delta);
            light.range = Mathf.LerpUnclamped(rangeEnd, rangeStart, delta);
        }

        if (animateColor)
        {
            float delta = loop ? Mathf.Clamp01((Time.time % colorDuration) / colorDuration) : Mathf.Clamp01(Time.time / colorDuration);
            delta = perlinColor ? Mathf.PerlinNoise(Time.time * perlinColorSpeed, 0f) : colorCurve.Evaluate(delta);
            light.color = colorGradient.Evaluate(delta);
        }
    }


}

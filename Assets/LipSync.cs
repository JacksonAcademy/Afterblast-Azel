using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LipSync : MonoBehaviour
{
    public Sprite open;
    public Sprite closed;
    public List<Sprite> rn;
    public AudioSource source;

    public Image img;
    private float time;
    public float smoooth;
    public Transform imgTransform;
    private Vector2 startSCale;
    public Vector2 maxScale;


    public AudioSource audioSource;
    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;

    private float currentUpdateTime = 0f;

    [Range(0, 1)]public float clipLoudness;
    public float[] clipSampleData;
    public float loudnessMultiplier;

    public float smoothLoudness;

    // Use this for initialization
    void Awake()
    {
        clipSampleData = new float[sampleDataLength];
        startSCale = imgTransform.localScale;
    }

    private void FixedUpdate()
    {
        currentUpdateTime += Time.deltaTime;
        clipLoudness = 0;
        if (currentUpdateTime >= updateStep && audioSource.clip)
        {
            currentUpdateTime = 0f;
            audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
            clipLoudness = 0f;
            foreach (var sample in clipSampleData)
            {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength;
            clipLoudness *= loudnessMultiplier; //clipLoudness is what you are looking for
        }
        smoothLoudness = Mathf.Lerp(smoothLoudness, clipLoudness, smoooth * Time.deltaTime);
        if (smoothLoudness <= .1f)
            img.sprite = closed;
        else if(Time.time > time)
        {
            img.sprite = rn[Random.Range(0, rn.Count)];
            time = Time.time + Random.Range(.2f, .1f);
        }
        imgTransform.localScale = Vector2.Lerp(startSCale, maxScale, smoothLoudness);

    }
}

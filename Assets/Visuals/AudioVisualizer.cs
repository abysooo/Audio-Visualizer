using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public AudioSource mic;
    public int sampleWindow = 64;
    public float minZ, maxZ;
    public float loudnessSensibility = 100;
    public float threshold = 0.1f;
    public AudioPitchEstimator estimator;
    public float estimateRate = 30;
    public Renderer sphere;
    // Start is called before the first frame update
    void Start()
    {
        MicrophoneToAudioClip();
        InvokeRepeating(nameof(EstimatePitch), 0, 1.0f / estimateRate);
    }

    void Update() 
    {
        float loudness = GetLoudnessFromMicrophone() * loudnessSensibility;
        float loudnessAndSense = loudness * loudnessSensibility;
        if (loudnessSensibility < threshold) 
        {
            loudness = 0;
        }
        float newZ = Mathf.Lerp(maxZ, minZ, loudnessAndSense);
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, newZ);
        transform.position = newPosition;
        
        float t = Mathf.Clamp01(loudness / 10.0f); 
        Color color = Color.Lerp(Color.blue, Color.red, t); 
        sphere.material.color = color;
    }

    // Update is called once per frame
    void EstimatePitch() {
        var frequency = estimator.Estimate(mic);
        if (float.IsNaN(frequency))
        {
            return;
        }
        var note = Mathf.RoundToInt(12 * Mathf.Log(frequency / 440) / Mathf.Log(2) + 69);
        float format = frequency / 10;
        Vector3 newPosition = new Vector3(note % 12 * 2, format + 10, transform.position.z);
        transform.position = newPosition;
    }
     public void MicrophoneToAudioClip()
    {
        string microphoneName = Microphone.devices[0];
        mic.clip = Microphone.Start(microphoneName, true, 20, AudioSettings.outputSampleRate);
    }

    public float GetLoudnessFromMicrophone()
    {
        string microphoneName = Microphone.devices[0];
        return GetLoudnessFromAudioClip(Microphone.GetPosition(microphoneName), mic.clip);
    }

    public float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;
        if (startPosition < 0)
        {
            return 0;
        }
        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        float totalLoudness = 0;

        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]);
        }

        return totalLoudness / sampleWindow;
    }
}
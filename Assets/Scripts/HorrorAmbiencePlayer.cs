using UnityEngine;
using System.Collections;

public class HorrorAmbiencePlayer : MonoBehaviour
{
    [Header("Sound Layers")]
    public AudioClip[] backgroundSounds; // Постоянный фон
    public AudioClip[] tensionSounds;    // Звуки напряжения
    public AudioClip[] scareSounds;      // Скримеры/пугающие звуки
    public AudioClip[] whisperSounds;    // Шепоты
    public AudioClip[] environmentalSounds; // Звуки окружения
    
    [Header("Layer Settings")]
    [Range(0f, 1f)] public float backgroundVolume = 0.1f;
    [Range(0f, 1f)] public float tensionVolume = 0.3f;
    [Range(0f, 1f)] public float scareVolume = 0.5f;
    [Range(0f, 1f)] public float whisperVolume = 0.2f;
    [Range(0f, 1f)] public float environmentalVolume = 0.4f;
    
    [Header("Timing")]
    public float tensionInterval = 30f;
    public float scareInterval = 60f;
    public float whisperInterval = 45f;
    public float environmentalInterval = 20f;
    
    [Header("Player Distance Effect")]
    public float maxHearingDistance = 30f;
    public Transform player;
    
    private AudioSource backgroundSource;
    private AudioSource[] dynamicSources = new AudioSource[4];
    private float tensionTimer;
    private float scareTimer;
    private float whisperTimer;
    private float environmentalTimer;
    private float playerDistance;

    void Start()
    {
        backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.loop = true;
        backgroundSource.spatialBlend = 0.8f;
        backgroundSource.rolloffMode = AudioRolloffMode.Linear;
        backgroundSource.maxDistance = maxHearingDistance;
        
        for (int i = 0; i < dynamicSources.Length; i++)
        {
            dynamicSources[i] = gameObject.AddComponent<AudioSource>();
            dynamicSources[i].spatialBlend = 1f;
            dynamicSources[i].maxDistance = maxHearingDistance;
        }
        
        if (backgroundSounds.Length > 0)
        {
            PlayBackgroundSound();
        }
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                player = Camera.main?.transform;
            }
        }
        
        ResetTimers();
    }

    void Update()
    {
        if (player != null)
        {
            playerDistance = Vector3.Distance(transform.position, player.position);
            UpdateVolumeBasedOnDistance();
        }
        
        CheckTimers();
    }

    void PlayBackgroundSound()
    {
        if (backgroundSounds.Length == 0) return;
        
        int index = Random.Range(0, backgroundSounds.Length);
        backgroundSource.clip = backgroundSounds[index];
        backgroundSource.volume = backgroundVolume;
        backgroundSource.Play();
        
        Invoke("PlayBackgroundSound", backgroundSource.clip.length);
    }

    void CheckTimers()
    {
        tensionTimer += Time.deltaTime;
        scareTimer += Time.deltaTime;
        whisperTimer += Time.deltaTime;
        environmentalTimer += Time.deltaTime;
        
        if (tensionTimer >= tensionInterval && tensionSounds.Length > 0)
        {
            PlaySound(tensionSounds, dynamicSources[0], tensionVolume);
            tensionTimer = 0f;
            tensionInterval = Random.Range(20f, 40f);
        }
        
        if (scareTimer >= scareInterval && scareSounds.Length > 0)
        {
            PlaySound(scareSounds, dynamicSources[1], scareVolume);
            scareTimer = 0f;
            scareInterval = Random.Range(40f, 90f);
        }
        
        if (whisperTimer >= whisperInterval && whisperSounds.Length > 0)
        {
            PlaySound(whisperSounds, dynamicSources[2], whisperVolume);
            whisperTimer = 0f;
            whisperInterval = Random.Range(30f, 60f);
        }
        
        if (environmentalTimer >= environmentalInterval && environmentalSounds.Length > 0)
        {
            PlaySound(environmentalSounds, dynamicSources[3], environmentalVolume);
            environmentalTimer = 0f;
            environmentalInterval = Random.Range(15f, 30f);
        }
    }

    void PlaySound(AudioClip[] clips, AudioSource source, float baseVolume)
    {
        if (clips.Length == 0 || source == null) return;
        
        int index = Random.Range(0, clips.Length);
        source.clip = clips[index];
        source.volume = baseVolume;
        source.pitch = Random.Range(0.9f, 1.1f);
        source.Play();
    }

    void UpdateVolumeBasedOnDistance()
    {
        if (playerDistance <= maxHearingDistance)
        {
            float distanceFactor = 1f - (playerDistance / maxHearingDistance);
            
            backgroundSource.volume = backgroundVolume * distanceFactor;
            
            foreach (var source in dynamicSources)
            {
                if (source.isPlaying)
                {
                    
                }
            }
        }
        else
        {
            backgroundSource.volume = 0f;
        }
    }

    void ResetTimers()
    {
        tensionTimer = Random.Range(0f, tensionInterval);
        scareTimer = Random.Range(0f, scareInterval);
        whisperTimer = Random.Range(0f, whisperInterval);
        environmentalTimer = Random.Range(0f, environmentalInterval);
    }

    public void PlayTensionSound()
    {
        PlaySound(tensionSounds, dynamicSources[0], tensionVolume);
        tensionTimer = 0f;
    }

    public void PlayScareSound()
    {
        PlaySound(scareSounds, dynamicSources[1], scareVolume);
        scareTimer = 0f;
    }

    public void PlayWhisperSound()
    {
        PlaySound(whisperSounds, dynamicSources[2], whisperVolume);
        whisperTimer = 0f;
    }

    public void PlayEnvironmentalSound()
    {
        PlaySound(environmentalSounds, dynamicSources[3], environmentalVolume);
        environmentalTimer = 0f;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxHearingDistance);
    }
}
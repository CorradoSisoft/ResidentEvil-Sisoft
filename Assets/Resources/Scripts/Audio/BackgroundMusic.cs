using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    public AudioSource audioSource;
    public AudioClip floorMusic;
    public AudioClip safeRoomMusic;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayFloor()
    {
        Play(floorMusic);
    }

    public void PlaySafeRoom()
    {
        Play(safeRoomMusic);
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return; // già in play
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGenerator : MonoBehaviour
{
    [SerializeField] AudioSource mySource;
    [SerializeField] AudioClip[] randomClips;
    [SerializeField] AudioClip swordSound, laserSound, BGM;
    [SerializeField] float defaultRangeStart;
    [SerializeField] float[] randRange;
    float delta, timePassed;
    bool addToBeat, addSwordSound, addLaserSound;
    bool endBeatElement;
    //float[] soundDelays;
    float soundDelay;
    int soundsAdded;

    // Start is called before the first frame update
    void Start()
    {
        delta = 0f;
        timePassed = 0f;
        soundsAdded = 0;
        defaultRangeStart = 0.5f;
        addToBeat = false;
        endBeatElement = false;
        mySource.PlayOneShot(BGM);
    }

    // Update is called once per frame
    void Update()
    {
        delta += Time.deltaTime;
        timePassed = delta / 60f;

        // Basic error testing
        if (Input.GetKeyDown(KeyCode.A)) { addSwordSound = true; }
        if (Input.GetKeyDown(KeyCode.S)) { addLaserSound = true; }
        if (Input.GetKeyDown(KeyCode.D)) { addToBeat = true; }

        // Check if sword or laser sound is supposed to be played
        if (addSwordSound) { mySource.PlayOneShot(swordSound); addSwordSound = false; }
        if (addLaserSound) { mySource.PlayOneShot(laserSound); addLaserSound = false; }
        AddBeat();
    }

    IEnumerator AddSoundAfterTime(float time, AudioClip sound)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        if (!endBeatElement) { mySource.PlayOneShot(sound); StartCoroutine(AddSoundAfterTime(time, sound)); }
    }

    // Adds sounds to beat randomly
    void AddBeat(){
        if (!addToBeat) { return; }
        soundsAdded++;
        //soundDelays[soundsAdded - 1] = Random.Range(defaultRangeStart, defaultRangeStart + randRange[soundsAdded - 1]);
        soundDelay = Random.Range(defaultRangeStart, defaultRangeStart + randRange[soundsAdded - 1]);
        StartCoroutine(AddSoundAfterTime(soundDelay, randomClips[Random.Range(0, randomClips.Length)]));
        addToBeat = false;
    }
}

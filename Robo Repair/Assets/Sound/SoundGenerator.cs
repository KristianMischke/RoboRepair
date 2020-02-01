using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGenerator : MonoBehaviour
{
    [SerializeField] AudioSource mySource;
    [SerializeField] AudioClip[] randomClips;
    [SerializeField] AudioClip swordSound, laserSound, walkSound, BGM;
    [SerializeField] float[] randRange;
    [SerializeField] float[] randRangeStart;
    float delta, timePassed;
    bool addToBeat, addSwordSound, addLaserSound, addWalkSound;
    bool endBeatElement;
    bool stopWalk;
    float soundDelay;
    int soundsAdded;
    float BGMTime;
    const float BGMTimeConst = 411f;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize necessary values so we can use them later
        delta = 0f;
        timePassed = 0f;
        soundsAdded = 0;
        addToBeat = false; addSwordSound = false; addLaserSound = false; addWalkSound = false;
        endBeatElement = false;
        stopWalk = false;
        BGMTime = 411f;
        mySource.PlayOneShot(BGM);
    }

    // Update is called once per frame
    void Update()
    {
        delta += Time.deltaTime;
        timePassed = delta / 60f;

        if (timePassed >= BGMTime) { BGMTime += BGMTimeConst; mySource.PlayOneShot(BGM); }
        // Basic error testing
        //if (Input.GetKeyDown(KeyCode.A)) { AddSwordSound(); }
        //if (Input.GetKeyDown(KeyCode.S)) { AddLaserSound(); }
        //if (Input.GetKeyDown(KeyCode.D)) { AddRandomSound(); }
        //if (Input.GetKeyDown(KeyCode.W)) { AddWalkSound(); }
        //if (Input.GetKeyUp(KeyCode.W)) { StopWalkSound(); }

        // Check if sword, laser, or walk sound is supposed to be played
        if (addSwordSound) { mySource.PlayOneShot(swordSound); addSwordSound = false; }
        if (addLaserSound) { mySource.PlayOneShot(laserSound); addLaserSound = false; }
        if (addWalkSound) {
            mySource.PlayOneShot(walkSound);
            stopWalk = false; addWalkSound = false;
            StartCoroutine(AddWalkLoop(1.5f, walkSound));
        }
        AddBeat();
    }

    void AddSwordSound() { addSwordSound = true; }
    void AddLaserSound() { addLaserSound = true; }
    void AddWalkSound() { addWalkSound = true; }
    void StopWalkSound() { addWalkSound = false; stopWalk = true; }
    void AddRandomSound() { addToBeat = true; }


    IEnumerator AddWalkLoop(float length, AudioClip sound)
    {
        yield return new WaitForSeconds(length);

        // Code to execute after the delay
        // If the time since the audioclip's entry into the beat began is greater than it should be, end the beat
        if (!stopWalk) { mySource.PlayOneShot(sound); StartCoroutine(AddWalkLoop(length, sound)); }
    }

    IEnumerator AddSoundAfterTime(float time, float timeStart, float length, AudioClip sound)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        // If the time since the audioclip's entry into the beat began is greater than it should be, end the beat
        endBeatElement = (timePassed - timeStart) >= length;
        if (!endBeatElement) { mySource.PlayOneShot(sound); StartCoroutine(AddSoundAfterTime(time, timeStart, length, sound)); }
    }

    // Adds sounds to beat randomly
    void AddBeat(){
        if (!addToBeat) { return; }
        // Add to current # of sounds in beat, set a delay for when to replay the sound within the beat
        soundsAdded++;
        soundDelay = Random.Range(randRangeStart[soundsAdded - 1], randRangeStart[soundsAdded - 1] + randRange[soundsAdded - 1]);
        // Get a random sound from the provided list of AudioClips, play it
        AudioClip randSound = randomClips[Random.Range(0, randomClips.Length)];
        mySource.PlayOneShot(randSound);
        // Start a coroutine to keep replaying the sound, include a random time the beat will continue for
        StartCoroutine(AddSoundAfterTime(soundDelay, timePassed, Random.Range(10f, 60f), randSound));
        addToBeat = false;
    }
}

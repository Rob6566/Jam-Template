//Handles switching between music and playing sounds
//We use adaptive music, with a MusicSet for each mood
//Call changeMusicMood from other scripts to switch between moods. We'll continue playing the current track, then switch to the new one on the beat

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MusicMood {none, intro, tutorial, game, shop, scoring, scoring_intense, scoring_very_intense, dead, intense, very_intense, boss_intro, boss_loop, victory, start_game};
public enum GameSound {deal, shuffle, flick, score, upgrade_card_allsuits, upgrade_card_copy, upgrade_card_rank_down, upgrade_card_rank_up, upgrade_card_remove, upgrade_card_timer, upgrade_card_score, upgrade_card_mult, upgrade_change_suit};
public class AudioManager : MonoBehaviour {
    [SerializeField] AudioSource sfxAudioSource;
    int playingMusic=-1;
    public Scrollbar musicScrollbar;
    public Scrollbar sfxScrollbar;
    public List<MusicSet> musicSets; //Music
    public List<MusicSet> sfxSets;   //SFX

    public List<AudioSource> audioSources = new List<AudioSource>();

    public MusicMood moodPlaying = MusicMood.none;
    public MusicMood desiredMood = MusicMood.none;
    MusicMood desiredMoodAfterEntireLoop = MusicMood.none;
    bool testRunning;
    double nextStartTime;
    double nextLoopTime;
    int selectedAudioSource=0;
    List<AudioClip> moodAudioClips = new List<AudioClip>();
    float MUSIC_BAR_LENGTH=3.2f; //All our tracks need to be the same length
    int musicClipPlaying;


    void Update() {
        if (moodPlaying==null) {
            return;
        }

        if (AudioSettings.dspTime > (nextLoopTime - .1f)) {
            if (desiredMood!=MusicMood.none && desiredMood != moodPlaying) {
                nextStartTime=nextLoopTime;
            }
            else {
                nextLoopTime+=MUSIC_BAR_LENGTH;
            }
        }

        //Prepare our next track if we're in the last half second of the current one
        if (AudioSettings.dspTime > (nextStartTime - .1f)) {
            audioSources[selectedAudioSource].SetScheduledEndTime(nextStartTime);
            selectedAudioSource = 1 - selectedAudioSource; //Swap the audio source

            int loopsInNewTrack=0;

            //Switch mood if that has been requested
            if (moodPlaying!=desiredMood) {
                moodPlaying=desiredMood;
                musicClipPlaying=0;
            }
            else {
                musicClipPlaying++;
                if (musicClipPlaying>=moodAudioClips.Count) {
                    if (desiredMoodAfterEntireLoop!=MusicMood.none) {
                        moodPlaying=desiredMoodAfterEntireLoop;
                        desiredMood=moodPlaying;
                        desiredMoodAfterEntireLoop=MusicMood.none;
                    }

                    musicClipPlaying=0;
                }
            }

            loopsInNewTrack=loadMusicMood(moodPlaying);

            Debug.Log("Prepping track "+musicClipPlaying+" for mood "+moodPlaying+", desired Mood= "+desiredMood+" lopsInNewTrack="+loopsInNewTrack);
            audioSources[selectedAudioSource].clip = moodAudioClips[musicClipPlaying];
            audioSources[selectedAudioSource].time=0;
            audioSources[selectedAudioSource].PlayScheduled(nextStartTime);
            nextStartTime+=MUSIC_BAR_LENGTH*loopsInNewTrack;
            nextLoopTime+=MUSIC_BAR_LENGTH;
        }
    }


    /*public void fadeInMusic(int selectedMusic, float fadeInTime, float endVolume) {

        if (playingMusic>=0) {
            StartCoroutine (AudioFade.FadeOutThenFadeIn(music[playingMusic], fadeInTime, music[selectedMusic], endVolume*musicScrollbar.value));
        }
        else {
            StartCoroutine (AudioFade.FadeIn(music[selectedMusic], fadeInTime, endVolume*musicScrollbar.value));
        }
        playingMusic=selectedMusic;
    }

    public void fadeOutMusic(int selectedMusic, float fadeOutTime) {
        StartCoroutine (AudioFade.FadeOut(music[selectedMusic], fadeOutTime));
    }*/

    public void playSound(GameSound soundToPlay, float volume, int desiredSound=-1) {
         foreach(MusicSet sfxSet in sfxSets) {
            if (sfxSet.gameSound==soundToPlay) {
                //Play a random sound from the sfxSet
                int selectedSound = desiredSound>-1 ? desiredSound : Random.Range(0, sfxSet.tracks.Count);
                Debug.Log("Play sound "+selectedSound+" from set "+sfxSet.gameSound+" with volume "+volume*sfxScrollbar.value);
                sfxAudioSource.PlayOneShot(sfxSet.tracks[selectedSound], volume*sfxScrollbar.value);
            }
        }        
    }

    public void setMusicVolume() {
        foreach(AudioSource audioSource in audioSources) {
            audioSource.volume=musicScrollbar.value;
        }
        Debug.Log("AudioSource Value set to "+musicScrollbar.value);
    }
    
    public void muteMusic() {
        foreach(AudioSource audioSource in audioSources) {
            audioSource.volume=0f;
        }
    }


    public void initAdaptiveMusic() {
        
        int barsInNewTrack=changeMusicMood(MusicMood.intro);

        nextStartTime = AudioSettings.dspTime + 1;
        
        musicClipPlaying=0;
        audioSources[selectedAudioSource].clip = moodAudioClips[musicClipPlaying];
        audioSources[selectedAudioSource].time = 0;
        audioSources[selectedAudioSource].PlayScheduled(nextStartTime);
        nextLoopTime=nextStartTime+MUSIC_BAR_LENGTH;
        nextStartTime+=MUSIC_BAR_LENGTH*barsInNewTrack;
    }



    //Public function to allow us to change which mood to switch to on next transition. Return number of bars in new track
    public int changeMusicMood(MusicMood mood) {

        Debug.Log("Change music mood to "+mood);
        int barsInNewTrack=0;
        if (moodPlaying==MusicMood.none) {
            moodPlaying=mood;
            desiredMood=mood;
            Debug.Log("Loading first mood"+mood);
            barsInNewTrack=loadMusicMood(mood);
        }
        if (moodPlaying!=mood) {
            desiredMood=mood;
        }
        desiredMoodAfterEntireLoop=MusicMood.none;
        return barsInNewTrack;
    }

    //Public function that allows us to play the entire current loop (multiple tracks), then switch to a new mood 
    public void changeMusicMoodAfterCurrentLoop(MusicMood mood) {
        desiredMoodAfterEntireLoop=mood;
    }

    //Returns number of loops in new track
    int loadMusicMood(MusicMood mood) {
        Debug.Log("Loading mood"+mood);
        moodAudioClips.Clear();
        foreach(MusicSet musicSet in musicSets) {
            if (musicSet.musicMood==mood) {
                foreach (AudioClip clip in musicSet.tracks) {
                    moodAudioClips.Add(clip);
                    Debug.Log("Adding track to mood clips: "+clip.name);
                }
                return musicSet.loopsInTrack;
            }
        }
        return 0;
    }

    public void testPlaySound() {
        playSound(GameSound.shuffle, 1f);
    }
}
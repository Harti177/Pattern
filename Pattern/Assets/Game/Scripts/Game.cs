using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

namespace Harti.Pattern
{
    public class Game : MonoBehaviour
    {
        public Arrange[] arranges;
        public AudioClip[] audioClipsGame;
        public AudioClip audioClipAmbience;
        public AudioSource ambience; 

        public Saber leftSaber;
        public Saber rightSaber;

        private Beat beatHovered;
        private Beat beatPrev;
        private Saber.HandType beatHoveredHand;
        private bool beatLocked = false;
        private List<Beat> beatsSmashedThisRound;
        private List<Beat> beatsSmashed;

        private Beat[][] beats;

        public InputActionReference activateLeftSaber;
        public InputActionReference activateRightSaber;

        public TextMeshProUGUI score1;
        public TextMeshProUGUI score2;
        public TextMeshProUGUI score3;
        public TextMeshProUGUI score4;

        public TextMeshProUGUI fps;

        private int points = 0;
        public float counter;
        int gameFactor = 0;

        //public LineRenderer lineRenderer;
        public GameObject lockLine;
        public GameObject lockLineStart;
        public GameObject lockLineEnd;

        private bool gameIsActive = false; 

        public GameObject initialUi;
        public GameObject gameUi;

        public TextMeshProUGUI[] highScores;
        public TextMeshProUGUI gameOverText;

        public GameObject leftRayInteractor;
        public GameObject rightRayInteractor;

        string highScoresKey = "PatternHighScore2606_"; 

        private void OnEnable()
        {
            leftSaber.onCollisionEnter.AddListener(CheckSaberCollisionEnter);
            rightSaber.onCollisionEnter.AddListener(CheckSaberCollisionEnter);
            leftSaber.onCollisionExit.AddListener(CheckSaberCollisionExit);
            rightSaber.onCollisionExit.AddListener(CheckSaberCollisionExit);
        }

        private void OnDisable()
        {
            leftSaber.onCollisionEnter.RemoveListener(CheckSaberCollisionEnter);
            rightSaber.onCollisionEnter.RemoveListener(CheckSaberCollisionEnter);
            leftSaber.onCollisionExit.RemoveListener(CheckSaberCollisionExit);
            rightSaber.onCollisionExit.RemoveListener(CheckSaberCollisionExit);
        }

        private void Start()
        {
            GetAndSetHighScores(); 
        }

        private void DestroyAllBeats()
        {
            List<GameObject> toBeDeleted = new List<GameObject>();
            if (beats != null)
            {
                for (int j = 0; j < beats.Length; j++)
                {
                    if (beats[j] != null)
                    {
                        for (int i = 0; i < beats[j].Length; i++)
                        {
                            if (beats[j][i] != null)
                                toBeDeleted.Add(beats[j][i].gameObject);
                        }
                    }
                }
            }

            foreach (GameObject go in toBeDeleted)
            {
                Destroy(go);
            }
        }

        private void GetAndSetHighScores()
        {
            for (int i = 0; i < highScores.Length; i++)
            {
                highScores[i].text = PlayerPrefs.GetInt(highScoresKey + i, 0) != 0 ? (i + 1) + ". " + PlayerPrefs.GetInt(highScoresKey + i, 0).ToString() : (i+1) + ". ";
            }
        }

        private void SetScore()
        {
            score1.text = "Score\n" + points.ToString();
            score2.text = "Score\n" + points.ToString();
            score3.text = "Score\n" + points.ToString();
            score4.text = "Score\n" + points.ToString();
        }

        private void SetGameOverText()
        {
            gameOverText.text = "Game Over! You scored - " + points.ToString();
            gameOverText.gameObject.SetActive(true);

            int nth = 0;

            for (int i = highScores.Length-1; i >= 0; i--)
            {
                if (PlayerPrefs.GetInt(highScoresKey + i, 0) == 0) 
                {
                    nth = i;
                    continue; 
                }

                if (PlayerPrefs.GetInt(highScoresKey + i, 0) > points)
                {
                    break; 
                }
            }

            int pointsPrev = 0; 
            for (int j = 0; j <= nth; j++)
            {
                if (PlayerPrefs.GetInt(highScoresKey + j, 0) > points)
                {
                    continue;
                }

                if (pointsPrev != 0)
                {
                    int temp = pointsPrev;
                    pointsPrev = PlayerPrefs.GetInt(highScoresKey + j, 0); 
                    PlayerPrefs.SetInt(highScoresKey + j, temp);
                }
                else
                {
                    pointsPrev = PlayerPrefs.GetInt(highScoresKey + j, 0);
                    PlayerPrefs.SetInt(highScoresKey + j, points);
                }
            }
        }

        public void ExitGame()
        {
            StartCoroutine(DelayedExitGame());
        }

        private IEnumerator DelayedExitGame()
        {
            yield return new WaitForSeconds(0.5f);

            Beat[] allBeats = Game.FindObjectsOfType<Beat>();
            foreach (Beat beat in allBeats)
            {
                beat.GameOver(); 
            }

            StartCoroutine(DelayedDestroy());
        }

        private IEnumerator DelayedDestroy()
        {
            yield return new WaitForSeconds(0.5f);

            SetGameOverText();

            DestroyAllBeats();

            leftSaber.gameObject.SetActive(false);
            rightSaber.gameObject.SetActive(false);

            ambience.Stop();
            ambience.clip = audioClipAmbience;
            ambience.Play();

            beatLocked = false;
            beatHovered = null;
            beatPrev = null;
            lockLine.SetActive(false);
            gameIsActive = false;

            counter = 0;
            points = 0;
            gameFactor = 0;

            gameUi.GetComponent<CanvasGroup>().interactable = (false);
            gameUi.GetComponent<CanvasGroup>().alpha = 0;
            initialUi.GetComponent<CanvasGroup>().interactable = (true);
            initialUi.GetComponent<CanvasGroup>().alpha = 1;

            leftRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;
            rightRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;

            GetAndSetHighScores();
        }

        public void StartGame()
        {
            DestroyAllBeats();

            leftSaber.gameObject.SetActive(true);
            rightSaber.gameObject.SetActive(true);

            ambience.Stop();
            ambience.clip = audioClipsGame[gameFactor];
            ambience.Play();

            beatsSmashedThisRound = new List<Beat>();
            beatsSmashed = new List<Beat>();

            beatLocked = false;
            beatHovered = null;
            beatPrev = null;
            lockLine.SetActive(false);
            gameIsActive = true;

            counter = 0;
            points = 0;
            gameFactor = 0;

            GameObject[][] beatsGOs = arranges[gameFactor].ArrangeGame();
            beats = new Beat[beatsGOs.Length][];

            for (int j = 0; j < beats.Length; j++)
            {
                beats[j] = new Beat[beatsGOs[j].Length];
                for (int i = 0; i < beats[j].Length; i++)
                {
                    Debug.Log(i + " " + j);
                    beats[j][i] = beatsGOs[j][i].GetComponent<Beat>();
                    beats[j][i].xPosition = i;
                    beats[j][i].yPosition = j;
                }
            }

            gameUi.GetComponent<CanvasGroup>().interactable = (true);
            gameUi.GetComponent<CanvasGroup>().alpha = 1;
            initialUi.GetComponent<CanvasGroup>().interactable = (false);
            initialUi.GetComponent<CanvasGroup>().alpha = 0;

            leftRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = false;
            rightRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = false;

            gameOverText.gameObject.SetActive(false);

        }

        void Update()
        {
            if (gameIsActive)
            {
                counter += Time.deltaTime;

                if(counter > 50 && !beatLocked)
                {
                    if(beatHovered != null)
                    {
                        beatHovered.UnHover();
                        beatHovered = null;
                        beatPrev = null;
                    }

                    gameFactor++;
                    if (gameFactor > arranges.Length) gameFactor = 0;

                    GameObject[][] beatsGOs = arranges[gameFactor].ArrangeGame();
                    for (int j = 0; j < beats.Length; j++)
                    {
                        for (int i = 0; i < beats[j].Length; i++)
                        {
                            if (beats[j][i].beatMode == Beat.BeatMode.smash)
                            {
                                Destroy(beats[j][i].gameObject);
                                beats[j][i] = beatsGOs[j][i].GetComponent<Beat>();
                                beats[j][i].xPosition = i;
                                beats[j][i].yPosition = j;
                            }else if (beats[j][i].beatMode == Beat.BeatMode.normal || beats[j][i].beatMode == Beat.BeatMode.hit)
                            {
                                counter = 0;
                                ExitGame();
                                return;
                            }
                        }
                    }

                    counter = 0;
                }

                if (activateLeftSaber.action.inProgress)
                {
                    if (beatHovered != null && beatHoveredHand == Saber.HandType.left)
                    {
                        beatLocked = true;
                        lockLine.SetActive(true);
                        lockLineStart.transform.position = leftSaber.tip.position;
                        lockLineEnd.transform.position = beatHovered.transform.position;
                        leftSaber.Lock();
                    }
                    else
                    {
                        leftSaber.Activate();
                    }
                }
                else
                {
                    leftSaber.DeActivate();
                    if (beatHoveredHand == Saber.HandType.left)
                    {
                        if (beatLocked)
                        {
                            beatHovered.UnHover();
                            beatHovered = null;
                            if (beatsSmashedThisRound != null) 
                            {
                                if(beatsSmashedThisRound.Count == 1)
                                {
                                    ExitGame();
                                    return; 
                                }
                                points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                            } 

                            beatsSmashed.AddRange(beatsSmashedThisRound);
                            SetScore();
                            beatsSmashedThisRound = new List<Beat>();
                        }
                        lockLine.SetActive(false);
                        beatLocked = false;
                    }
                }

                if (activateRightSaber.action.inProgress)
                {
                    if (beatHovered != null && beatHoveredHand == Saber.HandType.right)
                    {
                        beatLocked = true;
                        lockLine.SetActive(true);
                        lockLineStart.transform.position = rightSaber.tip.position;
                        lockLineEnd.transform.position = beatHovered.transform.position;
                        rightSaber.Lock();
                    }
                    else
                    {
                        rightSaber.Activate();
                    }
                }
                else
                {
                    rightSaber.DeActivate();
                    if (beatHoveredHand == Saber.HandType.right)
                    {
                        if (beatLocked)
                        {
                            beatHovered.UnHover();
                            beatHovered = null;
                            if (beatsSmashedThisRound != null)
                            {
                                if (beatsSmashedThisRound.Count == 1)
                                {
                                    ExitGame();
                                    return;
                                }
                                points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                            }
                            beatsSmashed.AddRange(beatsSmashedThisRound);
                            SetScore();
                            beatsSmashedThisRound = new List<Beat>();
                        }
                        lockLine.SetActive(false);
                        beatLocked = false;
                    }
                }

                fps.text = (Time.frameCount / Time.time).ToString("000");
            }
        }

        public void CheckSaberCollisionEnter(GameObject go, Saber.HandType handType, Vector3 position, Vector3 direction)
        {
            if (go.GetComponent<Beat>() != null)
            {
                Beat beat = go.GetComponent<Beat>();

                if (beat.beatMode == Beat.BeatMode.smash)
                {
                    beatPrev = beat;
                    return;
                }

                if (beatHovered != null)
                {
                    if(beatHovered == beat)
                    {
                        if(beatHoveredHand == handType)
                        {

                        }
                        else
                        {
                            if (beatLocked)
                            {
                                beatHovered.Smash(position, direction);
                                beatsSmashedThisRound.Add(beatHovered);
                            }
                        }
                    }
                    else
                    {
                        if (beatHoveredHand == handType)
                        {
                            if (!beatLocked)
                            {
                                beatHovered.UnHover();
                                beatHovered = beat;
                                beatHoveredHand = handType;
                                beatHovered.Hover();
                                if (beatsSmashedThisRound != null)
                                {
                                    if (beatsSmashedThisRound.Count == 1)
                                    {
                                        ExitGame();
                                        return;
                                    }
                                    points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                                }
                                beatsSmashed.AddRange(beatsSmashedThisRound);
                                beatsSmashedThisRound = new List<Beat>();
                                SetScore();
                            }
                        }
                        else
                        {
                            if (beatHovered.beatMode == Beat.BeatMode.smash)
                            {
                                if (beatHovered.xPosition == beat.xPosition)
                                {
                                    int yDifference = beatHovered.yPosition - beat.yPosition;

                                    if(yDifference > 0)
                                    {
                                        int length = Mathf.Abs(yDifference);
                                        int i = beatHovered.xPosition; 
                                        for (int j = beatHovered.yPosition; j > beatHovered.yPosition - length; j--)
                                        {
                                            if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                            {
                                                beatPrev = beat;
                                                return;
                                            }
                                        }

                                        if (beatLocked && beat.beatType == beatHovered.beatType)
                                        {
                                            if (beatsSmashedThisRound.Count > 1)
                                            {
                                                int yDifferenceBeatsSmashed = beatsSmashedThisRound[0].yPosition - beatsSmashedThisRound[1].yPosition;
                                                if (!(yDifferenceBeatsSmashed > 0))
                                                {
                                                    beatPrev = beat;
                                                    return; 
                                                }
                                            }

                                            beat.Smash(position, direction);
                                            beatsSmashedThisRound.Add(beat);
                                        }
                                    }
                                    else
                                    {
                                        int length = Mathf.Abs(yDifference);
                                        int i = beatHovered.xPosition;
                                        for (int j = beatHovered.yPosition; j < beatHovered.yPosition + length; j++)
                                        {
                                            if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                            {
                                                beatPrev = beat;
                                                return;
                                            }
                                        }

                                        if (beatLocked && beat.beatType == beatHovered.beatType)
                                        {
                                            if (beatsSmashedThisRound.Count > 1)
                                            {
                                                int yDifferenceBeatsSmashed = beatsSmashedThisRound[0].yPosition - beatsSmashedThisRound[1].yPosition;
                                                if (yDifferenceBeatsSmashed > 0)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            beat.Smash(position, direction);
                                            beatsSmashedThisRound.Add(beat);
                                        }
                                    }
                                }

                                if (beatHovered.yPosition == beat.yPosition)
                                {
                                    if((beatPrev.xPosition != 0 && beatPrev.xPosition != beats[0].Length - 1 && beatPrev.xPosition - beat.xPosition < 0)
                                        || (beatPrev.xPosition == 0 && beatPrev.xPosition - beat.xPosition == -1) 
                                        || (beatPrev.xPosition == beats[0].Length - 1 && beatPrev.xPosition - beat.xPosition != 1))
                                    {
                                        if (beat.xPosition > beatHovered.xPosition && beat.xPosition < beats[0].Length)
                                        {
                                            int xDifference = beatHovered.xPosition - beat.xPosition;
                                            int length = Mathf.Abs(xDifference);
                                            int j = beatHovered.yPosition;
                                            for (int i = beatHovered.xPosition; i < beatHovered.xPosition + length; i++)
                                            {
                                                if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            if (beatLocked && beat.beatType == beatHovered.beatType)
                                            {
                                                if (beatsSmashedThisRound.Count > 1)
                                                {
                                                    if ((beatsSmashedThisRound[0].xPosition != 0 && beatsSmashedThisRound[0].xPosition != beats[0].Length - 1 && !(beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition < 0))
                                                        || (beatsSmashedThisRound[0].xPosition == 0 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition != -1)
                                                        || (beatsSmashedThisRound[0].xPosition == beats[0].Length - 1 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition == 1))
                                                    {
                                                        beatPrev = beat;
                                                        return;
                                                    }
                                                }

                                                beat.Smash(position, direction);
                                                beatsSmashedThisRound.Add(beat);
                                            }
                                        }
                                        else
                                        {
                                            for (int i = beatHovered.xPosition; i < beats[0].Length; i++)
                                            {
                                                int j = beatHovered.yPosition;
                                                if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            for (int i = 0; i < beat.xPosition; i++)
                                            {
                                                int j = beatHovered.yPosition;
                                                if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            if (beatLocked && beat.beatType == beatHovered.beatType)
                                            {
                                                if (beatsSmashedThisRound.Count > 1)
                                                {
                                                    if ((beatsSmashedThisRound[0].xPosition != 0 && beatsSmashedThisRound[0].xPosition != beats[0].Length - 1 && !(beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition < 0))
                                                        || (beatsSmashedThisRound[0].xPosition == 0 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition != -1)
                                                        || (beatsSmashedThisRound[0].xPosition == beats[0].Length - 1 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition == 1))
                                                    {
                                                        beatPrev = beat;
                                                        return;
                                                    }
                                                }

                                                beat.Smash(position, direction);
                                                beatsSmashedThisRound.Add(beat);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (beat.xPosition < beatHovered.xPosition && beat.xPosition >= 0)
                                        {
                                            int xDifference = beatHovered.xPosition - beat.xPosition;
                                            int length = Mathf.Abs(xDifference);
                                            int j = beatHovered.yPosition;
                                            for (int i = beatHovered.xPosition; i > beatHovered.xPosition - length; i--)
                                            {
                                                if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            if (beatLocked && beat.beatType == beatHovered.beatType)
                                            {
                                                if (beatsSmashedThisRound.Count > 1)
                                                {
                                                    if ((beatsSmashedThisRound[0].xPosition != 0 && beatsSmashedThisRound[0].xPosition != beats[0].Length - 1 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition < 0)
                                                        || (beatsSmashedThisRound[0].xPosition == 0 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition == -1)
                                                        || (beatsSmashedThisRound[0].xPosition == beats[0].Length - 1 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition != 1))
                                                    {
                                                        beatPrev = beat;
                                                        return;
                                                    }
                                                }

                                                beat.Smash(position, direction);
                                                beatsSmashedThisRound.Add(beat);
                                            }
                                        }
                                        else
                                        {
                                            for (int i = beatHovered.xPosition; i >= 0; i--)
                                            {
                                                int j = beatHovered.yPosition;
                                                if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            for (int i = beats[0].Length - 1; i > beat.xPosition; i--)
                                            {
                                                int j = beatHovered.yPosition;
                                                if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                                {
                                                    beatPrev = beat;
                                                    return;
                                                }
                                            }

                                            if (beatLocked && beat.beatType == beatHovered.beatType)
                                            {
                                                if (beatsSmashedThisRound.Count > 1)
                                                {
                                                    if ((beatsSmashedThisRound[0].xPosition != 0 && beatsSmashedThisRound[0].xPosition != beats[0].Length - 1 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition < 0)
                                                        || (beatsSmashedThisRound[0].xPosition == 0 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition == -1)
                                                        || (beatsSmashedThisRound[0].xPosition == beats[0].Length - 1 && beatsSmashedThisRound[0].xPosition - beatsSmashedThisRound[1].xPosition != 1))
                                                    {
                                                        beatPrev = beat;
                                                        return;
                                                    }
                                                }

                                                beat.Smash(position, direction);
                                                beatsSmashedThisRound.Add(beat);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    beatHovered = beat;
                    beatHoveredHand = handType;
                    beatHovered.Hover();
                    if (beatsSmashedThisRound != null)
                    {
                        if (beatsSmashedThisRound.Count == 1)
                        {
                            ExitGame();
                            return;
                        }
                        points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                    }
                    beatsSmashed.AddRange(beatsSmashedThisRound);
                    SetScore();
                    beatsSmashedThisRound = new List<Beat>();
                }

                beatPrev = beat; 
            }
        }

        public void CheckSaberCollisionExit(GameObject go, Saber.HandType handType)
        {
            if (go.GetComponent<Beat>() != null)
            {
                Beat beat = go.GetComponent<Beat>();

                if (beatHovered != null)
                {
                    if (beatHovered == beat)
                    {
                        if (beatHoveredHand == handType)
                        {
                            if (!beatLocked)
                            {
                                beatHovered.UnHover();
                                beatHovered = null;
                                if (beatsSmashedThisRound != null)
                                {
                                    if (beatsSmashedThisRound.Count == 1)
                                    {
                                        ExitGame();
                                        return;
                                    }
                                    points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                                }
                                beatsSmashed.AddRange(beatsSmashedThisRound);
                                SetScore();
                                beatsSmashedThisRound = new List<Beat>();
                            }
                        }
                    }
                }
            }
        }
    }
}
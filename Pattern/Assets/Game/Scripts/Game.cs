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

        private int points = 0;

        public TextMeshProUGUI fps;
        public TextMeshProUGUI score;
        public TextMeshProUGUI scoreHigh;
        public TextMeshProUGUI counterText;
        public TextMeshProUGUI counterText1;
        public float counter;

        //public LineRenderer lineRenderer;
        public GameObject lockLine;
        public GameObject lockLineStart;
        public GameObject lockLineEnd;

        private bool gameIsActive = false; 
        private int currGameId = -1;

        public GameObject initialUi;
        public GameObject gameUi;

        public TextMeshProUGUI[] highScores;
        public TextMeshProUGUI failedText;

        public GameObject leftRayInteractor;
        public GameObject rightRayInteractor; 

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
            for (int i = 0; i < highScores.Length; i++)
            {
                highScores[i].text = PlayerPrefs.GetInt("PatternHighScore2006" + i, 0).ToString();
            }
        }

        public void ExitGame()
        {
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
            currGameId = -1;
            counter = 0;

            gameUi.GetComponent<CanvasGroup>().interactable = (false);
            gameUi.GetComponent<CanvasGroup>().alpha = 0;
            initialUi.GetComponent<CanvasGroup>().interactable = (true);
            initialUi.GetComponent<CanvasGroup>().alpha = 1;

            leftRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;
            rightRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;

            points = 0; 
        }

        private void DestroyAllBeats()
        {
            List<GameObject> toBeDeleted = new List<GameObject>();
            if (beats != null)
            {
                for (int j = 0; j < beats.Length; j++)
                {
                    if(beats[j] != null)
                    {
                        for (int i = 0; i < beats[j].Length; i++)
                        {
                            if(beats[j][i] != null)
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

        public void StartGame(int gameId)
        {
            beatsSmashedThisRound = new List<Beat>();
            beatsSmashed = new List<Beat>(); 
            gameUi.GetComponent<CanvasGroup>().interactable = (true);
            gameUi.GetComponent<CanvasGroup>().alpha = 1;
            initialUi.GetComponent<CanvasGroup>().interactable = (false);
            initialUi.GetComponent<CanvasGroup>().alpha = 0;

            failedText.gameObject.SetActive(false);

            gameIsActive = true; 

            DestroyAllBeats();

            ambience.Stop();
            ambience.clip = audioClipsGame[gameId];
            ambience.Play();

            leftSaber.gameObject.SetActive(true);
            rightSaber.gameObject.SetActive(true);

            beatLocked = false;
            beatHovered = null;
            beatPrev = null;
            lockLine.SetActive(false);

            GameObject[][] beatsGOs = arranges[gameId].ArrangeGame();
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
            counter = 0;
            currGameId = gameId;
            scoreHigh.text = "HighestScore\n" + PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0).ToString();
            score.text = "Score\n" + "0";

            leftRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = false;
            rightRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = false;

            points = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (gameIsActive)
            {
                if(beatsSmashed.Count == 72)
                {
                    ExitGame();
                    failedText.text = "Awesome! Play again :)"; 
                    failedText.gameObject.SetActive(true);
                }

                counter += Time.deltaTime;
                counterText.text = (10 - counter).ToString("0");
                counterText1.text = "Smashed " + beatsSmashed.Count.ToString() + " beats";

                if (counter > 10)
                {
                    ExitGame();
                    failedText.text = "Failed! Try again :)";
                    failedText.gameObject.SetActive(true);
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
                                    failedText.text = "Failed! Try again :)";
                                    failedText.gameObject.SetActive(true);
                                    return; 
                                }
                                points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                            } 

                            beatsSmashed.AddRange(beatsSmashedThisRound);
                            score.text = "Score\n" + points.ToString();
                            if (points > PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0))
                            {
                                PlayerPrefs.SetInt("PatternHighScore2006" + currGameId, points);
                                scoreHigh.text = "HighScore\n" + PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0).ToString();
                            }
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
                                    failedText.text = "Failed! Try again :)";
                                    failedText.gameObject.SetActive(true);
                                    return;
                                }
                                points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                            }
                            beatsSmashed.AddRange(beatsSmashedThisRound);
                            score.text = "Score\n" + points.ToString();
                            if (points > PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0))
                            {
                                PlayerPrefs.SetInt("PatternHighScore2006" + currGameId, points);
                                scoreHigh.text = "HighScore\n" + PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0).ToString();
                            }
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
                                counter = 0;
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
                                        failedText.text = "Failed! Try again :)";
                                        failedText.gameObject.SetActive(true);
                                        return;
                                    }
                                    points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                                }
                                beatsSmashed.AddRange(beatsSmashedThisRound);
                                score.text = "Score\n" + points.ToString();
                                if (points > PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0))
                                {
                                    PlayerPrefs.SetInt("PatternHighScore2006" + currGameId, points);
                                    scoreHigh.text = "HighScore\n" + PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0).ToString();
                                }
                                beatsSmashedThisRound = new List<Beat>();
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
                                            counter = 0;
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
                                            counter = 0;
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
                                                counter = 0;
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
                                                counter = 0;
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
                                                counter = 0;
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
                                                counter = 0;
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
                            failedText.text = "Failed! Try again :)";
                            failedText.gameObject.SetActive(true);
                            return;
                        }
                        points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                    }
                    beatsSmashed.AddRange(beatsSmashedThisRound);
                    score.text = "Score\n" + points.ToString();
                    if (points > PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0))
                    {
                        PlayerPrefs.SetInt("PatternHighScore2006" + currGameId, points);
                        scoreHigh.text = "HighScore\n" + PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0).ToString();
                    }
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
                                        failedText.text = "Failed! Try again :)";
                                        failedText.gameObject.SetActive(true);
                                        return;
                                    }
                                    points += (beatsSmashedThisRound.Count * beatsSmashedThisRound.Count);
                                }
                                beatsSmashed.AddRange(beatsSmashedThisRound);
                                score.text = "Score\n" + points.ToString();
                                if (points > PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0))
                                {
                                    PlayerPrefs.SetInt("PatternHighScore2006" + currGameId, points);
                                    scoreHigh.text = "HighScore\n" + PlayerPrefs.GetInt("PatternHighScore2006" + currGameId, 0).ToString();
                                }
                                beatsSmashedThisRound = new List<Beat>();
                            }
                        }
                    }
                }
            }
        }
    }
}
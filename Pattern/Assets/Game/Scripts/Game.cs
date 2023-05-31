using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace Harti.Pattern
{
    public class Game : MonoBehaviour
    {
        public Arrange arrange;
        public Saber leftSaber;
        public Saber rightSaber; 

        private Beat beatHovered;
        private Beat beatPrev;
        private Saber.HandType beatHoveredHand;
        private bool beatLocked = false; 

        private Beat[][] beats;

        public InputActionReference activateLeftSaber;
        public InputActionReference activateRightSaber;

        public TextMeshProUGUI fps; 

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

        // Start is called before the first frame update
        void Start()
        {
            GameObject[][] beatsGOs = arrange.ArrangeGame();
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
        }

        // Update is called once per frame
        void Update()
        {
            if (activateLeftSaber.action.inProgress)
            {
                if(beatHovered != null && beatHoveredHand == Saber.HandType.left)
                {
                    beatLocked = true;
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
                    }
                    beatLocked = false;
                }
            }

            if (activateRightSaber.action.inProgress)
            {
                if (beatHovered != null && beatHoveredHand == Saber.HandType.right)
                {
                    beatLocked = true;
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
                if(beatHoveredHand == Saber.HandType.right)
                {
                    if (beatLocked)
                    {
                        beatHovered.UnHover();
                        beatHovered = null;
                    }
                    beatLocked = false;
                }
            }

            fps.text = (Time.frameCount / Time.time).ToString("000"); 
        }

        public void CheckSaberCollisionEnter(GameObject go, Saber.HandType handType)
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
                                beatHovered.Smash();
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
                                            beat.Smash();
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
                                            beat.Smash();
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
                                                beat.Smash();
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
                                                beat.Smash();
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
                                                beat.Smash();
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
                                                beat.Smash();
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
                            }
                        }
                    }
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Harti.Pattern
{
    public class Game : MonoBehaviour
    {
        public Arrange arrange;

        private Beat beatHovered;
        private Saber.HandType beatHoveredHand;

        private Beat[][] beats; 

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

        }

        public void CheckSaberCollisionEnter(GameObject go, Saber.HandType handType)
        {
            if (go.GetComponent<Beat>() != null)
            {
                Beat beat = go.GetComponent<Beat>();

                if(beatHovered != null)
                {
                    if(beatHovered == beat)
                    {
                        if(beatHoveredHand == handType)
                        {

                        }
                        else
                        {
                            beatHovered.Smash();
                        }
                    }
                    else
                    {
                        if (beatHoveredHand == handType)
                        {
                            beatHovered.UnHover();
                            beatHovered = beat;
                            beatHoveredHand = handType;
                            beatHovered.Hover();
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
                                        for (int j = beatHovered.yPosition; j > beatHovered.yPosition - (length - 1); j--)
                                        {
                                            if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                            {
                                                return;
                                            }
                                        }

                                        beat.Smash(); 
                                    }
                                    else
                                    {
                                        int length = Mathf.Abs(yDifference);
                                        int i = beatHovered.xPosition;
                                        for (int j = beatHovered.yPosition; j > beatHovered.yPosition + (length-1); j++)
                                        {
                                            if (beats[j][i].beatMode != Beat.BeatMode.smash)
                                            {
                                                return;
                                            }
                                        }

                                        beat.Smash();
                                    }
                                }

                                if(beatHovered.yPosition == beat.yPosition)
                                {

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
                            beatHovered.UnHover();
                            beatHovered = null;
                        }
                    }
                }
            }
        }
    }
}
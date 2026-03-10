using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewIntroSequence", menuName = "Intro/IntroSequenceData")]
public class IntroSequenceData : ScriptableObject
{
    public string sequenceName;
    public string completionFlag;
    public List<IntroLine> lines = new List<IntroLine>();
}

[System.Serializable]
public class IntroLine
{
    [TextArea(3, 8)]
    public string text;
    public Sprite image;
    [Range(6, 72)] public float textSize = 24f;
    public float fadeSpeed = 1f;
    public float lineDelay = 2f;
}
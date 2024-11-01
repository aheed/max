using UnityEngine;
using UnityEngine.UIElements;

public class SimpleBlinker
{
    public static readonly Color[] colors = {Color.black, Color.yellow};
    public static readonly float intervalSec = 0.2f;
    private VisualElement visualElement;
    private float timer;
    private int colorIndex;

    public SimpleBlinker (VisualElement visualElement)
    {
        this.visualElement = visualElement;
        //timer = intervalSec;
    }

    public void Update (float deltaTime)
    {
        timer -= deltaTime;
        if (timer <= 0)
        {
            colorIndex = (colorIndex + 1) % 2;
            visualElement.style.backgroundColor = colors[colorIndex];
            timer = intervalSec;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class BlinkText : MonoBehaviour
{
    public Text targetText;
    public float onTime = 0.8f;
    public float offTime = 0.5f;

    private float timer;
    private bool isOn = true;

    void Start()
    {
        if (targetText == null)
            targetText = GetComponent<Text>();

        timer = onTime;
        targetText.enabled = true;
        isOn = true;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isOn = !isOn;
            targetText.enabled = isOn;

            timer = isOn ? onTime : offTime;
        }
    }
}

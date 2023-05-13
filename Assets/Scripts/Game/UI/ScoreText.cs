using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    private RectTransform rect;
    private Text text;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        var color = text.color;
        color.a = 1.0f;
        text.color = color;
    }

    public void StartAnimation()
    {
        StartCoroutine(SmoothMove(rect.anchoredPosition + new Vector2(0, 150), 1.0f));
        StartCoroutine(SmoothFade(0.0f, 0.5f));
    }

    private IEnumerator SmoothMove(Vector3 pos, float time)
    {
        var startPos = rect.anchoredPosition;
        var t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime / time;
            rect.anchoredPosition = Vector2.Lerp(startPos, pos, Mathf.SmoothStep(0, 1, t));
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator SmoothFade(float alpha, float time)
    {
        yield return new WaitForSeconds(0.5f);
        var startColor = text.color;
        var t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime / time;
            var newColor = startColor;
            newColor.a = Mathf.Lerp(startColor.a, alpha, Mathf.SmoothStep(0, 1, t));
            text.color = newColor;
            yield return new WaitForEndOfFrame();
        }
    }
}
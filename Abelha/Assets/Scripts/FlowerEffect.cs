using UnityEngine;
using System.Collections;
public class FlowerEffect : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnMouseDown()
    {
        StartCoroutine(ClickEffect());
    }

    IEnumerator ClickEffect()
    {
        transform.localScale = originalScale * 1.1f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
}

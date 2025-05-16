using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    [SerializeField] GameObject screenHider;
    [SerializeField] GameObject menu;
    [SerializeField] Transform flash;
    bool triggered = false;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered && collision.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(Animation());
        }
    }

    IEnumerator Animation()
    {
        yield return new WaitForSeconds(0.4f);
        flash.gameObject.SetActive(true);
        flash.DORotate(new(0, 0, 360), 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
        yield return new WaitForSeconds(2f);
        screenHider.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        yield return new WaitForSeconds(2f);
        menu.SetActive(true);
    }
}

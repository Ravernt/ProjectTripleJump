using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { private set; get; }

    [SerializeField] TMP_Text progressText;

    int current;
    int all;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterCollectible()
    {
        all++;
        UpdateUI();
    }

    public void CollectCollectible()
    {
        current++;
        UpdateUI();
    }

    void UpdateUI()
    {
        progressText.text = $"{current}/{all}";
    }
}

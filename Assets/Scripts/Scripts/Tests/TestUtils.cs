using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TestUtils
{
    public struct TestCase<T1, T2>
    {
        public T1 value1;
        public T2 value2;
    }

    public static void CreateNewScene()
    {
        SceneManager.LoadScene("empty");
    }

    public static (PlayerController, GameObject) Set()
    {
        SetCamera();
        var floor = SetFloor();
        var player = SetPlayer();

        return (player, floor);
    }

    static void SetCamera()
    {
        GameObject gameObject = new GameObject("Camera");
        gameObject.transform.position = new Vector3(0, 0, -10);
        var cam = gameObject.AddComponent<Camera>();
        cam.orthographic = true;
    }

    static PlayerController SetPlayer()
    {
        GameObject.Instantiate(Resources.Load("Player"), Vector2.zero, Quaternion.identity);
        var player = GameObject.FindAnyObjectByType<PlayerController>();

        var stats = GameObject.Instantiate(Resources.Load<ScriptableStats>("Stats"));
        player.Stats = stats;

        return player;
    }

    static GameObject SetFloor()
    {
        var floor = new GameObject();
        var boxCollider = floor.AddComponent<BoxCollider2D>();
        var rb = floor.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;


        var sr = floor.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("base");
        floor.transform.position = new Vector3(0, -1, 0);
        floor.transform.localScale = new(50, 1, 1);

        return floor;
    }

    public static void CreateWall(Vector2 position, Vector2 size)
    {
        GameObject gameObject = new GameObject();
        var boxCollider = gameObject.AddComponent<BoxCollider2D>();
        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;


        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("base");
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;
    }
}

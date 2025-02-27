using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] PlayerController controller;
    [SerializeField] Sprite[] idleSprites;
    [SerializeField] float idleAnimationSpeed;
    [SerializeField] Sprite[] runSprites;
    [SerializeField] float runAnimationSpeed;
    [Space]
    [SerializeField] Sprite jumpSprite;
    [SerializeField] Sprite fallSprite;

    SpriteRenderer sr;
    Coroutine animationCoroutine;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        controller.OnStateChange += OnStateChange;
        OnStateChange(PlayerState.Idle);
    }

    void Update()
    {
        if(Mathf.Abs(controller.FrameInput.Move.x) > 0.1f)
        {
            sr.flipX = controller.FrameInput.Move.x < 0;
        }
    }

    void OnStateChange(PlayerState state)
    {
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        switch(state)
        {
            case PlayerState.Idle:
                animationCoroutine = StartCoroutine(SpriteAnimation(idleSprites, idleAnimationSpeed));
                break;

            case PlayerState.Running:
                animationCoroutine = StartCoroutine(SpriteAnimation(runSprites, runAnimationSpeed));
                break;

            case PlayerState.Jumping:
                sr.sprite = jumpSprite;
                break;

            case PlayerState.Falling:
                sr.sprite = fallSprite;
                break;
        }
    }

    IEnumerator SpriteAnimation(Sprite[] sprites, float speed)
    {
        while(true)
        {
            for(int i=0; i<sprites.Length; i++)
            {
                sr.sprite = sprites[i];
                yield return new WaitForSeconds(speed);
            }
        }
    }
}

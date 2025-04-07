using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] Transform holder;
    [SerializeField] PlayerController controller;
    [SerializeField] Health health;
    [Space]
    [SerializeField] Sprite[] idleSprites;
    [SerializeField] float idleAnimationSpeed;
    [SerializeField] Sprite[] runSprites;
    [SerializeField] float runAnimationSpeed;
    [Space]
    [SerializeField] Sprite jumpSprite;
    [SerializeField] Sprite fallSprite;
    [SerializeField] Sprite doubleJumpSprite;
    [SerializeField] Sprite dashSprite;
    [SerializeField] Sprite deadSprite;
    [SerializeField] Sprite wallSlideSprite;
    [SerializeField] Sprite glideSprite;
    [Space]
    [SerializeField] ParticleSystem moveParticle;
    [SerializeField] ParticleSystem jumpParticle;
    [SerializeField] ParticleSystem dashParticle;
    [SerializeField] ParticleSystem doubleJumpParticle;
    AudioManager audioManager;
    
    SpriteRenderer sr;
    Coroutine animationCoroutine;
    PlayerState currentState;

    List<Sequence> activeSequences = new();
    
    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        controller.OnStateChange += OnStateChange;
        health.OnDeath += () => OnStateChange(PlayerState.Dead);
        OnStateChange(PlayerState.Idle);
    }

    void Update()
    {
        if(currentState != PlayerState.Dead && currentState != PlayerState.WallSlidingLeft
            && currentState != PlayerState.WallSlidingRight && Mathf.Abs(controller.FrameInput.Move.x) > 0.1f)
        {
            holder.localScale = new(controller.FrameInput.Move.x < 0? -1 : 1, 1);
        }
    }

    void OnStateChange(PlayerState state)
    {
        currentState = state;

        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        dashParticle.Stop(true);
        moveParticle.Stop(true);

        foreach (Sequence sequence in activeSequences)
            sequence.Kill();
        activeSequences.Clear();

        sr.transform.localScale = Vector3.one;
        sr.transform.localRotation = Quaternion.identity;

        switch (state)
        {
            case PlayerState.Idle:
                animationCoroutine = StartCoroutine(PlaySpriteAnimation(idleSprites, idleAnimationSpeed));
                break;

            case PlayerState.Running:
                PlayRunAnimation();
                animationCoroutine = StartCoroutine(PlaySpriteAnimation(runSprites, runAnimationSpeed));
                moveParticle.Play(true);
                break;

            case PlayerState.Jumping:
                sr.sprite = jumpSprite;
                PlayJumpAnimation();
                jumpParticle.Play(true);
                break;

            case PlayerState.Falling:
                sr.sprite = fallSprite;
                break;

            case PlayerState.DoubleJumping:
                sr.sprite = doubleJumpSprite;
                PlayJumpAnimation();
                doubleJumpParticle.Play(true);
                break;

            case PlayerState.Dashing:
                sr.sprite = dashSprite;
                PlayDashAnimation();
                dashParticle.Play(true);
                break;

            case PlayerState.Dead:
                sr.sprite = deadSprite;
                break;

            case PlayerState.WallSlidingLeft:
                holder.transform.localScale = Vector3.one;
                sr.sprite = wallSlideSprite;
                //animationCoroutine = StartCoroutine(DelayedSpriteChange(wallSlideSprite, 0.05f, Vector3.one));
                break;

            case PlayerState.WallSlidingRight:
                holder.transform.localScale = new Vector3(-1, 1, 1);
                sr.sprite = wallSlideSprite;
                //animationCoroutine = StartCoroutine(DelayedSpriteChange(wallSlideSprite, 0.05f, new Vector3(-1, 1, 1)));
                break;

            case PlayerState.Gliding:
                sr.sprite = glideSprite;
                //animationCoroutine = StartCoroutine(DelayedSpriteChange(glideSprite, 0.05f, Vector3.zero));
                break;
        }
    }

    void PlayRunAnimation()
    {
        sr.transform.DOKill();
        Sequence sequence1 = DOTween.Sequence();
        sequence1.Append(sr.transform.DOScale(new Vector3(1.075f, 0.95f, 1f), 0.15f));
        sequence1.Append(sr.transform.DOScale(Vector3.one, 0.15f));
        sequence1.SetLoops(-1);
        activeSequences.Add(sequence1);

        Sequence sequence2 = DOTween.Sequence();
        sequence2.Append(sr.transform.DORotate(new Vector3(0, 0, 3), 0.15f));
        sequence2.Append(sr.transform.DORotate(Vector3.zero, 0.15f));
        sequence2.Append(sr.transform.DORotate(new Vector3(0, 0, -3), 0.15f));
        sequence2.Append(sr.transform.DORotate(Vector3.zero, 0.15f));
        sequence2.SetLoops(-1);
        activeSequences.Add(sequence2);
    }

    void PlayJumpAnimation()
    {
        audioManager.PlaySFX(audioManager.jump);
        sr.transform.DOKill();
        sr.transform.DOScale(new Vector3(0.8f, 1.15f, 1), 0.075f).OnComplete(
            () => sr.transform.DOScale(Vector3.one, 0.125f));
    }

    void PlayDashAnimation()
    {
        sr.transform.DOKill();
        sr.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.025f).OnComplete(
            () => sr.transform.DOScale(new Vector3(1.15f, 0.8f, 1f), 0.055f).OnComplete(
                () => sr.transform.DOScale(new Vector3(1.15f, 0.8f, 1f), 0.15f).OnComplete(
                    () => sr.transform.DOScale(Vector3.one, 0.075f))));
    }

    IEnumerator DelayedSpriteChange(Sprite sprite, float delay, Vector3 scaleChange)
    {
        yield return new WaitForSeconds(delay);
        sr.sprite = sprite;
        if(scaleChange != Vector3.zero)
        {
            holder.transform.localScale = scaleChange;
        }
    }

    IEnumerator PlaySpriteAnimation(Sprite[] sprites, float animationSpeed)
    {
        while(true)
        {
            for(int i=0; i< sprites.Length; i++)
            {
                sr.sprite = sprites[i];
                yield return new WaitForSeconds(animationSpeed);
            }
        }
    }
}

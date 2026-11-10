using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shuffle.UI.Assets;
using Shuffle.UI.Graphics;

namespace Baccarat.Presentation.Animations;

public class DealingAnimationQueue
{
    private readonly Queue<CardAnimation> _queue = new();
    private readonly List<CardAnimation> _landedCards = new();
    private readonly List<CardDiscardAnimation> _discardAnimations = new();

    private CardAnimation? _currentAnimation;
    private Action? _onDiscardComplete;
    private float _pauseTimer = 0f;
    private const float PauseBetweenCards = 0.25f;

    public bool IsBusy => _currentAnimation != null || _queue.Count > 0 || _pauseTimer > 0f || _discardAnimations.Count > 0;

    public void Enqueue(CardAnimation animation)
    {
        _queue.Enqueue(animation);
    }

    // Discard the cards sequentially
    public void DiscardAllTo(Vector2 discardPos, Action? onComplete = null)
    {
        _discardAnimations.Clear();
        _onDiscardComplete = onComplete;

        for (int i = 0; i < _landedCards.Count; i++)
        {
            var landed = _landedCards[i];
            // Slight 25ms stagger so cards sweep naturally into a neat deck
            float duration = 0.38f + (i * 0.025f);

            _discardAnimations.Add(new CardDiscardAnimation(
                landed.Card,
                landed.TargetPosition,
                discardPos,
                landed.SlantPixels,
                duration
            ));
        }

        _landedCards.Clear();
    }

    public void Update(float deltaSeconds)
    {
        // Process discard sweep if active
        if (_discardAnimations.Count > 0)
        {
            bool allDone = true;
            for (int i = 0; i < _discardAnimations.Count; i++)
            {
                _discardAnimations[i].Update(deltaSeconds);
                if (!_discardAnimations[i].IsComplete)
                {
                    allDone = false;
                }
            }

            if (allDone)
            {
                _discardAnimations.Clear();
                var callback = _onDiscardComplete;
                _onDiscardComplete = null;
                callback?.Invoke();
            }

            return;
        }

        // Process dealing queue
        if (_pauseTimer > 0f)
        {
            _pauseTimer -= deltaSeconds;
            return;
        }

        if (_currentAnimation == null && _queue.Count > 0)
        {
            _currentAnimation = _queue.Dequeue();
        }

        if (_currentAnimation != null)
        {
            _currentAnimation.Update(deltaSeconds);

            if (_currentAnimation.IsComplete)
            {
                _landedCards.Add(_currentAnimation);
                _currentAnimation = null;
                _pauseTimer = PauseBetweenCards;
            }
        }
    }

    public void Draw(CardQuadRenderer quadRenderer, CardTextureAtlas atlas, int cardWidth, int cardHeight)
    {
        // Landed stationary cards on felt
        for (int i = 0; i < _landedCards.Count; i++)
        {
            _landedCards[i].DrawQuad(quadRenderer, atlas, cardWidth, cardHeight);
        }

        // Flying card being dealt
        _currentAnimation?.DrawQuad(quadRenderer, atlas, cardWidth, cardHeight);

        // Flying cards sweeping to discard tray
        for (int i = 0; i < _discardAnimations.Count; i++)
        {
            _discardAnimations[i].Draw(quadRenderer, atlas, cardWidth, cardHeight);
        }
    }

    public void Clear()
    {
        _queue.Clear();
        _landedCards.Clear();
        _discardAnimations.Clear();
        _currentAnimation = null;
        _onDiscardComplete = null;
        _pauseTimer = 0f;
    }
}
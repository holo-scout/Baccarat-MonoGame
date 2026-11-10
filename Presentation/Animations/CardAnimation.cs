using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Models.Cards;
using Shuffle.UI.Assets;
using Shuffle.UI.Graphics;

namespace Baccarat.Presentation.Animations;

public class CardAnimation
{
    public Card Card { get; }
    public Vector2 StartPosition { get; }
    public Vector2 TargetPosition { get; }

    public float DurationSeconds { get; }
    public float ElapsedSeconds { get; private set; }
    public float SlantPixels { get; }

    public bool IsComplete => ElapsedSeconds >= DurationSeconds;
    public bool IsFlipped { get; private set; }

    private static readonly Rectangle CardCropRect = new Rectangle(10, 12, 120, 168);

    public CardAnimation(Card card, Vector2 startPos, Vector2 targetPos, float durationSeconds = 0.55f, float slantPixels = 0f)
    {
        Card = card;
        StartPosition = startPos;
        TargetPosition = targetPos;
        DurationSeconds = durationSeconds;
        ElapsedSeconds = 0f;
        IsFlipped = false;
        SlantPixels = slantPixels;
    }

    public void Update(float deltaSeconds)
    {
        ElapsedSeconds += deltaSeconds;

        if (ElapsedSeconds > DurationSeconds)
        {
            ElapsedSeconds = DurationSeconds;
            IsFlipped = true; // Fully revealed when landed
        }
        else if (ElapsedSeconds >= DurationSeconds * 0.5f)  // Flip midway through flight
        {
            IsFlipped = true;
        }
    }

    public Vector2 GetCurrentPosition()
    {
        // Ease out animation
        float t = Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);
        float ease = 1f - MathF.Pow(1f - t, 3);

        return Vector2.Lerp(StartPosition, TargetPosition, ease);
    }

    // Rotates smoothly from -90° (horizontal sideways) to 0° (upright)
    public float GetCurrentRotation()
    {
        float t = Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);

        // Completes rotation by 65% of the total flight
        float rotT = Math.Clamp(t / 0.65f, 0f, 1f);
        float rotEase = 1f - MathF.Pow(1f - rotT, 2);

        // Starts horizontal (-90 degrees / -Pi/2) and finishes at 0 (upright)
        return MathHelper.Lerp(-MathHelper.PiOver2, 0f, rotEase);
    }

    // Interpolates slant so it starts rectangular at shoe and slants as it lands
    public float GetCurrentSlant()
    {
        float t = Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);
        float rotT = Math.Clamp(t / 0.65f, 0f, 1f);
        return MathHelper.Lerp(0f, SlantPixels, rotT);
    }

    // Flips during the last 40% of the flight (from 0.60 to 1.0)
    public (float scaleX, bool showFace) GetFlipState()
    {
        float t = Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);

        // First 60%: Flying face-down while rotating
        if (t < 0.60f)
        {
            return (1f, false);
        }

        // 60% to 80%: Shrinking face-down (1.0 -> 0.0)
        if (t < 0.80f)
        {
            float flipProgress = (t - 0.60f) / 0.20f;
            return (1f - flipProgress, false);
        }

        // 80% to 100%: Expanding face-up (0.0 -> 1.0)
        float revealProgress = (t - 0.80f) / 0.20f;
        return (revealProgress, true);
    }

    public void DrawQuad(CardQuadRenderer quadRenderer, CardTextureAtlas atlas, int width, int height)
    {
        Vector2 pos = GetCurrentPosition();
        float rotation = GetCurrentRotation();
        float currentSlant = GetCurrentSlant();
        var (scaleX, showFace) = GetFlipState();

        int scaledWidth = Math.Max(1, (int)(width * scaleX));
        Texture2D texture = showFace ? atlas.GetCardTexture(Card.Id) : atlas.CardBack;

        quadRenderer.DrawParallelogram(
            texture,
            pos,
            scaledWidth,
            height,
            currentSlant,
            CardCropRect,
            rotation
        );
    }
}


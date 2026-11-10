using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Models.Cards;
using Shuffle.UI.Assets;
using Shuffle.UI.Graphics;

namespace Baccarat.Presentation.Animations;

public class CardDiscardAnimation
{
    public Card Card { get; }
    public Vector2 StartPosition { get; }
    public Vector2 TargetPosition { get; }
    public float StartSlant { get; }
    public float DurationSeconds { get; }
    public float ElapsedSeconds { get; private set; }
    public bool IsComplete => ElapsedSeconds >= DurationSeconds;

    private static readonly Rectangle CardCropRect = new Rectangle(10, 12, 120, 168);

    public CardDiscardAnimation(Card card, Vector2 startPos, Vector2 targetPos, float startSlant, float durationSeconds = 0.45f)
    {
        Card = card;
        StartPosition = startPos;
        TargetPosition = targetPos;
        StartSlant = startSlant;
        DurationSeconds = durationSeconds;
        ElapsedSeconds = 0f;
    }

    public void Update(float deltaSeconds)
    {
        ElapsedSeconds += deltaSeconds;
    }

    // Flips face-down mid-flight (from Card Face to Card Back)
    public (float scaleX, bool showFace) GetFlipState()
    {
        float t = Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);

        // 0% to 15%: lifts and starts moving face-up
        if (t < 0.15f)
        {
            return (1f, true);
        }

        // 15% to 45%: shrinks horizontally face-up (1.0 -> 0.0)
        if (t < 0.45f)
        {
            float progress = (t - 0.15f) / 0.30f;
            return (1f - progress, true);
        }

        // 45% to 75%: expands horizontally face-down showing card back (0.0 -> 1.0)
        if (t < 0.75f)
        {
            float progress = (t - 0.45f) / 0.30f;
            return (progress, false);
        }

        // 75% to 100%: fully face-down as it slides into the discard tray
        return (1f, false);
    }

    public void Draw(CardQuadRenderer quadRenderer, CardTextureAtlas atlas, int width, int height)
    {
        float t = Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);

        // Smooth acceleration curve (Ease-In-Out Smoothstep)
        float ease = t * t * (3f - 2f * t);

        Vector2 pos = Vector2.Lerp(StartPosition, TargetPosition, ease);

        // Straighten perspective slant to 0 as it leaves the table slot
        float slant = MathHelper.Lerp(StartSlant, 0f, t);

        // Rotate smoothly from 0° (upright) to 90° (horizontal sideways)
        float rotation = MathHelper.Lerp(0f, MathHelper.PiOver2, ease);

        // 3D Flip state (face-up -> edge -> face-down)
        var (scaleX, showFace) = GetFlipState();
        int scaledWidth = Math.Max(1, (int)(width * scaleX));
        Texture2D texture = showFace ? atlas.GetCardTexture(Card.Id) : atlas.CardBack;

        quadRenderer.DrawParallelogram(
            texture,
            pos,
            scaledWidth,
            height,
            slant,
            CardCropRect,
            rotation
        );
    }
}
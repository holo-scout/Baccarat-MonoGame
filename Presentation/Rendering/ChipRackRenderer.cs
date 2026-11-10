using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shuffle.UI.Assets;
using Shuffle.UI.Graphics;

namespace Baccarat.Presentation.Rendering;

public class ChipRackRenderer
{
    public static readonly int[] Denominations = ChipTextureAtlas.Denominations;

    private readonly ChipTextureAtlas _chipAtlas;
    private readonly Rectangle[] _chipRackSlots = new Rectangle[Denominations.Length];

    private int? _pressedIndex = null;
    public int SelectedDenomination { get; private set; } = 25;

    public ChipRackRenderer(ChipTextureAtlas chipAtlas)
    {
        _chipAtlas = chipAtlas ?? throw new ArgumentNullException(nameof(chipAtlas));
    }

    public void UpdateLayout(Rectangle rackArea)
    {
        int count = Denominations.Length;
        int chipSize = Math.Min(rackArea.Height - 10, rackArea.Width / count - 8);
        int spacing = (rackArea.Width - (count * chipSize)) / (count + 1);

        for (int i = 0; i < count; i++)
        {
            int x = rackArea.X + spacing + (i * (chipSize + spacing));
            int y = rackArea.Y + (rackArea.Height - chipSize) / 2;

            _chipRackSlots[i] = new Rectangle(x, y, chipSize, chipSize);
        }
    }

    public bool HandleMouseDown(Point mousePosition)
    {
        for (int i = 0; i < Denominations.Length; i++)
        {
            if (_chipRackSlots[i].Contains(mousePosition))
            {
                _pressedIndex = i;
                return true;
            }
        }
        return false;
    }

    public bool HandleMouseUp(Point mousePosition)
    {
        if (_pressedIndex.HasValue)
        {
            int i = _pressedIndex.Value;
            _pressedIndex = null;

            // Only register if released while still inside the same chip
            if (_chipRackSlots[i].Contains(mousePosition))
            {
                SelectedDenomination = Denominations[i];
                return true;
            }
        }
        return false;
    }

    public void DrawRack(SpriteBatch spriteBatch, Point mousePos, bool isMouseDown)
    {
        for (int i = 0; i < Denominations.Length; i++)
        {
            int denom = Denominations[i];
            Rectangle rect = _chipRackSlots[i];
            Texture2D texture = _chipAtlas.GetTexture(denom);

            bool isHovered = rect.Contains(mousePos);
            bool isPressed = isHovered && isMouseDown && (_pressedIndex == i);

            // 1. SINK IN EFFECT: Nudge 2px down when pressed
            Rectangle drawRect = rect;
            if (isPressed)
            {
                drawRect.Y += 2;
            }

            // 2. WHITE GLOW: Always visible around the selected chip
            if (denom == SelectedDenomination)
            {
                int spread = 6;

                Rectangle glowRect = new Rectangle(
                    drawRect.X - spread,
                    drawRect.Y - spread,
                    drawRect.Width + (spread * 2),
                    drawRect.Height + (spread * 2)
                );

                spriteBatch.Draw(_chipAtlas.GlowTexture, glowRect, Color.White * 0.90f);
            }

            // 3. SHADING TINT:
            Color tint = Color.White;
            if (isPressed)
            {
                tint = new Color(205, 205, 205);
            }
            else if (isHovered)
            {
                tint = new Color(255, 255, 215);
            }

            spriteBatch.Draw(texture, drawRect, tint);
        }
    }
}

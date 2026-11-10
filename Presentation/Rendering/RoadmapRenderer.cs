using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Models.Rules;
using Baccarat.Models.Roadmaps;

namespace Baccarat.Presentation.Rendering;

public class RoadmapRenderer
{
    private readonly Texture2D _pixel;
    private readonly SpriteFont _font;

    // Standard Roadmap Colors
    private static readonly Color PlayerColor = new Color(41, 128, 185); // Rich Royal Blue
    private static readonly Color BankerColor = new Color(192, 57, 43);  // Deep Casino Red
    private static readonly Color TieColor = new Color(39, 174, 96);  // Emerald Green
    private static readonly Color GridColor = new Color(255, 255, 255, 40); // Faint Grid Lines

    public RoadmapRenderer(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _font = font;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void DrawBeadPlate(SpriteBatch spriteBatch, BeadPlate plate, Rectangle area)
    {
        int rows = BeadPlate.Rows; // 6
        int cellSize = area.Height / rows;
        int maxCols = area.Width / cellSize;

        // Dark background plate
        spriteBatch.Draw(_pixel, area, Color.Black * 0.55f);

        // Draw grid lines
        DrawGrid(spriteBatch, area, cellSize, rows, maxCols);

        // Auto-scroll so latest results are always visible
        int startCol = Math.Max(0, plate.MaxColumn - maxCols + 1);

        foreach (var cell in plate.Cells)
        {
            if (cell.Column < startCol || cell.Column >= startCol + maxCols) continue;

            int displayCol = cell.Column - startCol;
            int x = area.X + (displayCol * cellSize);
            int y = area.Y + (cell.Row * cellSize);
            Rectangle beadRect = new Rectangle(x + 2, y + 2, cellSize - 4, cellSize - 4);

            // Determine bead color
            Color beadColor = cell.Outcome.Winner switch
            {
                WinningSide.Player => PlayerColor,
                WinningSide.Banker => BankerColor,
                _ => TieColor
            };

            // Draw solid bead
            spriteBatch.Draw(_pixel, beadRect, beadColor);

            // Pair dots
            if (cell.Outcome.BankerPair)
            {
                // Red dot on top-left
                spriteBatch.Draw(_pixel, new Rectangle(x + 2, y + 2, 4, 4), BankerColor);
            }
            if (cell.Outcome.PlayerPair)
            {
                // Blue dot on bottom-right
                spriteBatch.Draw(_pixel, new Rectangle(x + cellSize - 6, y + cellSize - 6, 4, 4), PlayerColor);
            }
        }
    }

    public void DrawBigRoad(SpriteBatch spriteBatch, BigRoad road, Rectangle area)
    {
        int rows = BigRoad.Rows; // 6
        int cellSize = area.Height / rows;
        int maxCols = area.Width / cellSize;

        // Dark background plate
        spriteBatch.Draw(_pixel, area, Color.Black * 0.55f);

        // Draw grid lines
        DrawGrid(spriteBatch, area, cellSize, rows, maxCols);

        // Find max column in Big Road for auto-scrolling
        int startCol = Math.Max(0, road.MaxColumn - maxCols + 1);

        // If the shoe started with ties and no Player/Banker has won yet, display the tie slash in cell (0, 0)
        if (road.Cells.Count == 0 && road.InitialTieCount > 0)
        {
            Rectangle tieRect = new Rectangle(area.X + 2, area.Y + 2, cellSize - 4, cellSize - 4);
            DrawDiagonalSlash(spriteBatch, tieRect, TieColor, thickness: 2);

            if (road.InitialTieCount > 1)
            {
                string tieText = $"{road.InitialTieCount}";
                Vector2 textSize = _font.MeasureString(tieText);
                Vector2 textPos = new Vector2(tieRect.Center.X - (textSize.X / 2), tieRect.Center.Y - (textSize.Y / 2));
                spriteBatch.DrawString(_font, tieText, textPos, TieColor);
            }
        }

        foreach (var cell in road.Cells)
        {
            if (cell.Column < startCol || cell.Column >= startCol + maxCols) continue;

            int displayCol = cell.Column - startCol;
            int x = area.X + (displayCol * cellSize);
            int y = area.Y + (cell.Row * cellSize);
            Rectangle ringRect = new Rectangle(x + 2, y + 2, cellSize - 4, cellSize - 4);
            Color ringColor = cell.Winner == WinningSide.Player ? PlayerColor : BankerColor;

            // Draw hollow ring (border)
            DrawHollowBorder(spriteBatch, ringRect, ringColor, thickness: 2);

            // Draw green tie slash if tie occurred on this round
            if (cell.TieCount > 0)
            {
                DrawDiagonalSlash(spriteBatch, ringRect, TieColor, thickness: 2);

                // If multiple ties, write the count number (e.g. "2") in green
                if (cell.TieCount > 1)
                {
                    string tieText = $"{cell.TieCount}";
                    Vector2 textSize = _font.MeasureString(tieText);
                    Vector2 textPos = new Vector2(ringRect.Center.X - (textSize.X / 2), ringRect.Center.Y - (textSize.Y / 2));
                    spriteBatch.DrawString(_font, tieText, textPos, TieColor);
                }
            }

            // Pair dots on Big Road
            if (cell.BankerPair)
            {
                spriteBatch.Draw(_pixel, new Rectangle(x + 1, y + 1, 4, 4), BankerColor);
            }

            if (cell.PlayerPair)
            {
                spriteBatch.Draw(_pixel, new Rectangle(x + cellSize - 5, y + cellSize - 5, 4, 4), PlayerColor);
            }
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch, Rectangle area, int cellSize, int rows, int cols)
    {
        // Horizontal lines
        for (int r = 0; r <= rows; r++)
        {
            int y = area.Y + (r * cellSize);
            spriteBatch.Draw(_pixel, new Rectangle(area.X, y, cols * cellSize, 1), GridColor);
        }

        // Vertical lines
        for (int c = 0; c <= cols; c++)
        {
            int x = area.X + (c * cellSize);
            spriteBatch.Draw(_pixel, new Rectangle(x, area.Y, 1, rows * cellSize), GridColor);
        }
    }

    private void DrawHollowBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private void DrawDiagonalSlash(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        // Simple green diagonal line drawn across the cell
        int steps = Math.Min(rect.Width, rect.Height);

        for (int i = 0; i < steps; i++)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + i, rect.Bottom - i - thickness, thickness, thickness), color);
        }
    }
}


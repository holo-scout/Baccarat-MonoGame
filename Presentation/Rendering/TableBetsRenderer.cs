using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Models.Betting;
using Baccarat.Logic.Engine;
using Shuffle.UI.Assets;
using Shuffle.UI.Graphics;
using Baccarat.Presentation.Layout;

namespace Baccarat.Presentation.Rendering;

public class TableBetsRenderer
{
    private readonly BaccaratGameEngine _engine;
    private readonly TableLayout _layout;
    private readonly ChipTextureAtlas _chipAtlas;
    private readonly SpriteFont _balanceFont;
    private readonly SpriteFont _tableFont;

    public TableBetsRenderer(
        BaccaratGameEngine engine,
        TableLayout layout,
        ChipTextureAtlas chipAtlas,
        SpriteFont balanceFont,
        SpriteFont tableFont)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _chipAtlas = chipAtlas ?? throw new ArgumentNullException(nameof(chipAtlas));
        _balanceFont = balanceFont ?? throw new ArgumentNullException(nameof(balanceFont));
        _tableFont = tableFont ?? throw new ArgumentNullException(nameof(tableFont));
    }

    public void DrawBankrollInfo(SpriteBatch spriteBatch)
    {
        string balanceText = $"BALANCE: ${_engine.Bankroll.Balance:N0}";
        Vector2 pos = new Vector2(_layout.BalanceArea.X, _layout.BalanceArea.Y);

        spriteBatch.DrawString(_balanceFont, balanceText, pos + Vector2.One, Color.Black * 0.85f);
        spriteBatch.DrawString(_balanceFont, balanceText, pos, new Color(212, 175, 55));
    }

    public void DrawAllBettingBoxes(SpriteBatch spriteBatch)
    {
        DrawBoxBets(spriteBatch, BetType.Player, _layout.PlayerBetBox);
        DrawBoxBets(spriteBatch, BetType.Banker, _layout.BankerBetBox);
        DrawBoxBets(spriteBatch, BetType.Tie, _layout.TieBetBox);
        DrawBoxBets(spriteBatch, BetType.PlayerPair, _layout.PlayerPairBetBox);
        DrawBoxBets(spriteBatch, BetType.BankerPair, _layout.BankerPairBetBox);
    }

    private void DrawBoxBets(SpriteBatch spriteBatch, BetType type, Rectangle box)
    {
        var chipCounts = _engine.Table.GetChipCountsFor(type);
        decimal totalAmount = _engine.Table.GetBetAmount(type);
        string totalText = $"${totalAmount:N0}";

        if (chipCounts == null || chipCounts.Count == 0 || totalAmount <= 0) return;

        int xOffset = type switch
        {
            BetType.PlayerPair => box.X + 90,
            BetType.BankerPair => box.X + 120,
            BetType.Tie => box.X + 190,
            BetType.Player => box.X + 210,
            BetType.Banker => box.X + 225,
            _ => box.X + 10
        };

        int yOffset = (type == BetType.PlayerPair || type == BetType.BankerPair)
            ? box.Y + 160   // Pair boxes
            : box.Y + 130;  // Normal boxes

        Vector2 textPos = new Vector2(xOffset, yOffset);

        spriteBatch.DrawString(_tableFont, totalText, textPos + Vector2.One, Color.Black * 0.8f);
        spriteBatch.DrawString(_tableFont, totalText, textPos, new Color(224, 185, 75));

        List<(int Denomination, int Count)> stacks = OptimizeStacks((int)totalAmount);

        int chipWidth = 46;
        int chipHeight = (int)(chipWidth * 0.72f);
        int spacing = 6;

        if (type == BetType.PlayerPair || type == BetType.BankerPair)
        {
            int maxCols = 3;
            int rowStartY = box.Y + 35;

            for (int i = 0; i < stacks.Count && i < 6; i++)
            {
                int col = i % maxCols;
                int row = i / maxCols;

                int stacksInRow = Math.Min(stacks.Count - (row * maxCols), maxCols);
                int rowWidth = (stacksInRow * chipWidth) + ((stacksInRow - 1) * spacing);
                int startX = box.Center.X - (rowWidth / 2);

                int x = startX + (col * (chipWidth + spacing));
                int y = rowStartY + (row * (chipHeight + 16));

                DrawPhysicalChipStack(spriteBatch, stacks[i].Denomination, stacks[i].Count, x, y, chipWidth, chipHeight);
            }
        }
        else
        {
            int totalWidth = (stacks.Count * chipWidth) + ((stacks.Count - 1) * spacing);
            int startX = box.Center.X - (totalWidth / 2);
            int stackY = box.Y + 45;

            for (int i = 0; i < stacks.Count && i < 6; i++)
            {
                int x = startX + (i * (chipWidth + spacing));
                DrawPhysicalChipStack(spriteBatch, stacks[i].Denomination, stacks[i].Count, x, stackY, chipWidth, chipHeight);
            }
        }
    }

    // Greedy Denomination Algorithm
    private List<(int Denomination, int Count)> OptimizeStacks(int totalAmount)
    {
        var stacks = new List<(int Denomination, int Count)>();
        const int maxStacks = 6;
        const int maxChipsPerStack = 10;

        if (totalAmount <= 0) return stacks;

        int remaining = totalAmount;

        // 1. Resolve large denominations (500, 100, 50)
        int count500 = remaining / 500;
        remaining %= 500;

        int count100 = remaining / 100;
        remaining %= 100;

        int count50 = remaining / 50;
        remaining %= 50;

        // 2. Optimally resolve remainder (0 to 49) between 25, 10, and 1
        // Option A: Zero $25 chips
        int tensA = remaining / 10;
        int onesA = remaining % 10;
        int totalChipsA = tensA + onesA;

        // Option B: One $25 chip (only applicable if remainder >= 25)
        int count25 = 0;
        int count10 = tensA;
        int count1 = onesA;

        if (remaining >= 25)
        {
            int remB = remaining - 25;
            int tensB = remB / 10;
            int onesB = remB % 10;
            int totalChipsB = 1 + tensB + onesB;

            // Choose Option B only if it strictly reduces the total number of physical chips
            if (totalChipsB < totalChipsA)
            {
                count25 = 1;
                count10 = tensB;
                count1 = onesB;
            }
        }

        // 3. Partition chips into physical stacks of at most 10 chips (up to 6 stacks total)
        (int Denom, int Count)[] chips = new[]
        {
            (500, count500),
            (100, count100),
            (50, count50),
            (25, count25),
            (10, count10),
            (1, count1)
        };

        foreach (var (denom, count) in chips)
        {
            int unstacked = count;
            while (unstacked > 0 && stacks.Count < maxStacks)
            {
                int inStack = Math.Min(unstacked, maxChipsPerStack);
                stacks.Add((denom, inStack));
                unstacked -= inStack;
            }

            if (stacks.Count >= maxStacks)
                break;
        }

        // Reverse so lowest denominations sit on the left and highest on the right
        stacks.Reverse();
        return stacks;
    }

    public void DrawPhysicalChipStack(SpriteBatch spriteBatch, int denom, int count, int x, int baseY, int width, int height)
    {
        Texture2D chipTex = _chipAtlas.GetTexture(denom);
        int stackLayers = Math.Min(count, 10);

        Rectangle shadowRect = new Rectangle(x + 2, baseY + 6, width, height);
        spriteBatch.Draw(chipTex, shadowRect, Color.Black * 0.40f);

        for (int layer = 0; layer < stackLayers; layer++)
        {
            int y = baseY - (layer * 6);

            for (int rim = 5; rim >= 1; rim--)
            {
                Rectangle rimRect = new Rectangle(x, y + rim, width, height);
                float shade = 0.55f - (rim * 0.05f);
                spriteBatch.Draw(chipTex, rimRect, Color.Black * shade);
            }

            Rectangle faceRect = new Rectangle(x, y, width, height);
            spriteBatch.Draw(chipTex, faceRect, Color.White);
        }
    }
}

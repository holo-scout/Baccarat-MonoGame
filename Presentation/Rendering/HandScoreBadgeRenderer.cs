using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Logic.Engine;
using Baccarat.Presentation.Layout;

namespace Baccarat.Presentation.Rendering;

public class HandScoreBadgeRenderer
{
    private readonly BaccaratGameEngine _engine;
    private readonly TableLayout _layout;
    private readonly SpriteFont _tableFont;
    private readonly Texture2D _playerBadgeTexture;
    private readonly Texture2D _bankerBadgeTexture;

    public HandScoreBadgeRenderer(
        BaccaratGameEngine engine,
        TableLayout layout,
        SpriteFont tableFont,
        Texture2D playerBadgeTexture,
        Texture2D bankerBadgeTexture)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _tableFont = tableFont ?? throw new ArgumentNullException(nameof(tableFont));
        _playerBadgeTexture = playerBadgeTexture ?? throw new ArgumentNullException(nameof(playerBadgeTexture));
        _bankerBadgeTexture = bankerBadgeTexture ?? throw new ArgumentNullException(nameof(bankerBadgeTexture));
    }

    public void DrawHandScoreBadges(SpriteBatch spriteBatch)
    {
        if (_engine.PlayerHand.Count == 0 && _engine.BankerHand.Count == 0) return;

        // Player score badge (Blue)
        string pScoreText = $"{_engine.PlayerHand.Score}";
        Vector2 pPos = new Vector2(_layout.PlayerCardSlots[1].X - 15, _layout.PlayerCardSlots[0].Y + (_layout.ScreenHeight * 0.14f));
        DrawScoreBadge(spriteBatch, pScoreText, pPos, _playerBadgeTexture);

        // Banker score badge (Red)
        string bScoreText = $"{_engine.BankerHand.Score}";
        Vector2 bPos = new Vector2(_layout.BankerCardSlots[1].X - 15, _layout.BankerCardSlots[0].Y + (_layout.ScreenHeight * 0.14f));
        DrawScoreBadge(spriteBatch, bScoreText, bPos, _bankerBadgeTexture);
    }

    private void DrawScoreBadge(SpriteBatch spriteBatch, string text, Vector2 centerPos, Texture2D badgeTexture)
    {
        float scale = 1.0f;
        int badgeWidth = (int)(_layout.ScreenWidth * 0.052f);
        int badgeHeight = (int)(badgeWidth * (935f / 1024f));

        Rectangle badgeRect = new Rectangle(
            (int)(centerPos.X - (badgeWidth / 2f)),
            (int)(centerPos.Y - (badgeHeight / 2f)),
            badgeWidth,
            badgeHeight
        );

        spriteBatch.Draw(badgeTexture, badgeRect, Color.White);

        Vector2 textSize = _tableFont.MeasureString(text);
        Vector2 textPos = new Vector2(
            badgeRect.Center.X - (textSize.X / 2),
            badgeRect.Center.Y - (textSize.Y / 2) + 4
        );

        spriteBatch.DrawString(_tableFont, text, textPos + Vector2.One, new Color(20, 10, 5, 230), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_tableFont, text, textPos, new Color(245, 215, 110), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}

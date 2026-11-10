using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Logic.Engine;
using Baccarat.Presentation.Animations;
using Baccarat.Presentation.Layout;

namespace Baccarat.Presentation.Rendering;

public class TableBannersRenderer
{
    private readonly BaccaratGameEngine _engine;
    private readonly TableLayout _layout;
    private readonly SpriteFont _balanceFont;
    private readonly SpriteFont _tableFont;
    private readonly Texture2D _roundBannerTexture;
    private readonly Texture2D _backgroundTexture;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly DealingAnimationQueue _animQueue;


    public TableBannersRenderer(
        BaccaratGameEngine engine,
        TableLayout layout,
        SpriteFont balanceFont,
        SpriteFont tableFont,
        Texture2D roundBannerTexture,
        Texture2D backgroundTexture,
        GraphicsDevice graphicsDevice,
        DealingAnimationQueue animQueue)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _balanceFont = balanceFont ?? throw new ArgumentNullException(nameof(balanceFont));
        _tableFont = tableFont ?? throw new ArgumentNullException(nameof(tableFont));
        _roundBannerTexture = roundBannerTexture ?? throw new ArgumentNullException(nameof(roundBannerTexture));
        _backgroundTexture = backgroundTexture ?? throw new ArgumentNullException(nameof(backgroundTexture));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _animQueue = animQueue ?? throw new ArgumentNullException(nameof(animQueue));
    }

    public void DrawRoundOverAnnouncement(SpriteBatch spriteBatch)
    {
        if ((_engine.Phase != RoundPhase.RoundOver && _engine.Phase != RoundPhase.Reshuffle) || _animQueue.IsBusy)
            return;

        string title;
        string prompt;

        if (_engine.Phase == RoundPhase.Reshuffle)
        {
            title = "SHOE EXHAUSTED"; 
            prompt = "Click anywhere to reshuffle";
        }
        else
        {
            int pScore = _engine.PlayerHand.Score;
            int bScore = _engine.BankerHand.Score;

            if (pScore > bScore)
                title = "PLAYER WINS";
            else if (bScore > pScore)
                title = "BANKER WINS";
            else
                title = "TIE";

            prompt = "Click anywhere to continue";
        }

        int bannerWidth = (int)(_layout.ScreenWidth * 0.38f);
        int bannerHeight = (int)(bannerWidth / 3.0f);
        int bannerX = (_layout.ScreenWidth - bannerWidth) / 2;
        int bannerY = 0;

        Rectangle bannerRect = new Rectangle(bannerX, bannerY, bannerWidth, bannerHeight);
        spriteBatch.Draw(_roundBannerTexture, bannerRect, Color.White);

        Vector2 titleSize = _balanceFont.MeasureString(title);
        Vector2 titlePos = new Vector2(
            bannerRect.Center.X - (titleSize.X / 2),
            bannerRect.Center.Y - (titleSize.Y / 2) - 2
        );

        spriteBatch.DrawString(_balanceFont, title, titlePos + new Vector2(0, 1), new Color(255, 240, 200, 180));
        spriteBatch.DrawString(_balanceFont, title, titlePos, new Color(35, 20, 5));

        Vector2 promptSize = _tableFont.MeasureString(prompt);
        Vector2 promptPos = new Vector2(
            (_layout.ScreenWidth - promptSize.X) / 2,
            bannerRect.Bottom - 70
        );

        spriteBatch.DrawString(_tableFont, prompt, promptPos + Vector2.One, Color.Black);
        spriteBatch.DrawString(_tableFont, prompt, promptPos, Color.Black * 0.85f);
    }

    public void DrawRoadmapHeadings(SpriteBatch spriteBatch)
    {
        Color authenticGold = new Color(224, 185, 75);
        Color shadowColor = new Color(25, 18, 5, 220);

        string beadText = "BEAD PLATE";
        Vector2 beadSize = _balanceFont.MeasureString(beadText);
        Vector2 beadPos = new Vector2(
            _layout.BeadPlateArea.Center.X - (beadSize.X / 2) + 35,
            _layout.BeadPlateArea.Y - beadSize.Y + 13
        );

        spriteBatch.DrawString(_tableFont, beadText, beadPos + new Vector2(1, 2), shadowColor);
        spriteBatch.DrawString(_tableFont, beadText, beadPos, authenticGold);

        string roadText = "BIG ROAD";
        Vector2 roadSize = _balanceFont.MeasureString(roadText);
        Vector2 roadPos = new Vector2(
            _layout.BigRoadArea.Center.X - (roadSize.X / 2) + 25,
            _layout.BigRoadArea.Y - roadSize.Y + 13
        );

        spriteBatch.DrawString(_tableFont, roadText, roadPos + new Vector2(1, 2), shadowColor);
        spriteBatch.DrawString(_tableFont, roadText, roadPos, authenticGold);
    }

    public void DrawShoeCounter(SpriteBatch spriteBatch)
    {
        string text = $"{_engine.Shoe.RemainingCards}";

        // Positioned just to the right of the shoe
        Vector2 pos = new Vector2(
            _layout.ShoePosition.X + (_layout.ScreenWidth * 0.12f),
            _layout.ShoePosition.Y - 60
        );

        // Dark drop shadow for contrast + gold digits
        spriteBatch.DrawString(_balanceFont, text, pos + Vector2.One, Color.Black * 0.85f);
        spriteBatch.DrawString(_balanceFont, text, pos, new Color(224, 185, 75));
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Logic.Engine;
using Baccarat.Presentation.Animations;
using Baccarat.Presentation.Layout;

namespace Baccarat.Presentation.Rendering;

public class BankruptcyDialogRenderer
{
    private readonly BaccaratGameEngine _engine;
    private readonly TableLayout _layout;
    private readonly SpriteFont _balanceFont;
    private readonly SpriteFont _tableFont;
    private readonly Texture2D _bankruptcyDialogTexture;
    private readonly Texture2D _backgroundTexture;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly DealingAnimationQueue _animQueue;

    public Rectangle PlayAgainButtonRect { get; private set; }
    public Rectangle QuitButtonRect { get; private set; }

    public bool IsActive => _engine.Bankroll.Balance <= 0 && !_engine.Table.HasBets && !_animQueue.IsBusy;

    public BankruptcyDialogRenderer(
        BaccaratGameEngine engine,
        TableLayout layout,
        SpriteFont balanceFont,
        SpriteFont tableFont,
        Texture2D bankruptcyDialogTexture,
        Texture2D backgroundTexture,
        GraphicsDevice graphicsDevice,
        DealingAnimationQueue animQueue)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _balanceFont = balanceFont ?? throw new ArgumentNullException(nameof(balanceFont));
        _tableFont = tableFont ?? throw new ArgumentNullException(nameof(tableFont));
        _bankruptcyDialogTexture = bankruptcyDialogTexture ?? throw new ArgumentNullException(nameof(bankruptcyDialogTexture));
        _backgroundTexture = backgroundTexture ?? throw new ArgumentNullException(nameof(backgroundTexture));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _animQueue = animQueue ?? throw new ArgumentNullException(nameof(animQueue));
    }

    public void DrawBankruptcyPrompt(SpriteBatch spriteBatch)
    {
        if (!IsActive) return;

        Rectangle screenRect = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        spriteBatch.Draw(_backgroundTexture, screenRect, Color.Black * 0.65f);

        // Draw dialog background using layout
        Rectangle dialogRect = _layout.BankruptcyDialogArea;
        spriteBatch.Draw(_bankruptcyDialogTexture, dialogRect, Color.White);

        // Text
        Color darkBronze = new Color(45, 25, 10);
        string title = "OUT OF CHIPS";
        Vector2 titleSize = _balanceFont.MeasureString(title);
        Vector2 titlePos = new Vector2(
            dialogRect.Center.X - (titleSize.X / 2),
            dialogRect.Y + (int)(dialogRect.Height * 0.30f)
        );
        spriteBatch.DrawString(_balanceFont, title, titlePos, darkBronze);

        string message = "Your bankroll has reached $0.\nWould you like to refill $1,000 to keep playing?";
        Vector2 msgPos = new Vector2(
            dialogRect.Center.X - (_tableFont.MeasureString(message).X / 2),
            titlePos.Y + titleSize.Y + 12
        );
        spriteBatch.DrawString(_tableFont, message, msgPos, darkBronze * 0.90f);

        // Button Labels using layout rectangles
        Color goldText = new Color(230, 195, 95);
        Color goldShadow = new Color(20, 15, 5, 220);

        // Play Again Button Label
        string playText = "PLAY AGAIN";
        Vector2 playSize = _tableFont.MeasureString(playText);
        Vector2 playPos = new Vector2(
            _layout.PlayAgainButtonArea.Center.X - (playSize.X / 2),
            _layout.PlayAgainButtonArea.Center.Y - (playSize.Y / 2) + 10
        );
        spriteBatch.DrawString(_tableFont, playText, playPos + Vector2.One, goldShadow);
        spriteBatch.DrawString(_tableFont, playText, playPos, goldText);

        // Quit Button Label
        string quitText = "QUIT GAME";
        Vector2 quitSize = _tableFont.MeasureString(quitText);
        Vector2 quitPos = new Vector2(
            _layout.QuitButtonArea.Center.X - (quitSize.X / 2),
            _layout.QuitButtonArea.Center.Y - (quitSize.Y / 2) + 10
        );
        spriteBatch.DrawString(_tableFont, quitText, quitPos + Vector2.One, goldShadow);
        spriteBatch.DrawString(_tableFont, quitText, quitPos, goldText);
    }
}

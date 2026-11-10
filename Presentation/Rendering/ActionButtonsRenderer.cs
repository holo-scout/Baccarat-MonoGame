using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Baccarat.Logic.Engine;
using Baccarat.Presentation.Layout;

namespace Baccarat.Presentation.Rendering;

public enum ActionButtonType
{
    None,
    Rebet,
    Clear,
    Deal
}

public class ActionButtonsRenderer
{
    private readonly BaccaratGameEngine _engine;
    private readonly TableLayout _layout;
    private readonly SpriteFont _tableFont;
    private readonly Texture2D _goldButtonTexture;

    public ActionButtonType PressedActionButton { get; set; } = ActionButtonType.None;

    public ActionButtonsRenderer(
        BaccaratGameEngine engine,
        TableLayout layout,
        SpriteFont tableFont,
        Texture2D goldButtonTexture)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _tableFont = tableFont ?? throw new ArgumentNullException(nameof(tableFont));
        _goldButtonTexture = goldButtonTexture ?? throw new ArgumentNullException(nameof(goldButtonTexture));
    }

    public void DrawActionButtons(SpriteBatch spriteBatch, Point mousePos, bool isMouseDown)
    {
        bool canRebet = _engine.Table.CanRebet
            && _engine.Phase == RoundPhase.Betting
            && _engine.Bankroll.CanAfford(_engine.Table.LastRoundTotalBetAmount);
        DrawButton(spriteBatch, _layout.RebetButtonArea, "REBET", canRebet, ActionButtonType.Rebet, mousePos, isMouseDown);

        bool canClear = _engine.Table.HasBets && _engine.Phase == RoundPhase.Betting;
        DrawButton(spriteBatch, _layout.ClearButtonArea, "CLEAR", canClear, ActionButtonType.Clear, mousePos, isMouseDown);

        bool canDeal = _engine.Table.HasBets
            && _engine.Phase == RoundPhase.Betting
            && _engine.Bankroll.CanAfford(_engine.Table.TotalBetAmount);
        DrawButton(spriteBatch, _layout.DealButtonArea, "DEAL", canDeal, ActionButtonType.Deal, mousePos, isMouseDown);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text, bool isEnabled, ActionButtonType btnType, Point mousePos, bool isMouseDown)
    {
        bool isHovered = isEnabled && rect.Contains(mousePos);
        bool isPressed = isHovered && isMouseDown && (PressedActionButton == btnType);

        Rectangle drawRect = rect;
        if (isPressed)
        {
            drawRect.Y += 2;
        }

        Color tint;
        if (!isEnabled)
            tint = Color.Gray * 0.45f;
        else if (isPressed)
            tint = new Color(205, 195, 175);
        else if (isHovered)
            tint = new Color(255, 245, 210);
        else
            tint = Color.White;

        spriteBatch.Draw(_goldButtonTexture, drawRect, tint);

        Vector2 textSize = _tableFont.MeasureString(text);
        Vector2 textPos = new Vector2(
            drawRect.Center.X - (textSize.X / 2),
            drawRect.Center.Y - (textSize.Y / 2) + 5
        );

        if (isEnabled)
        {
            spriteBatch.DrawString(_tableFont, text, textPos + new Vector2(0, 1), new Color(255, 235, 170, 160));
            spriteBatch.DrawString(_tableFont, text, textPos, new Color(35, 20, 5));
        }
        else
        {
            spriteBatch.DrawString(_tableFont, text, textPos, Color.Gray * 0.7f);
        }
    }
}

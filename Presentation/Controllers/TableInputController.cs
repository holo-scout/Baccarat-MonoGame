using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Shuffle.UI;
using Baccarat.Models.Betting;
using Baccarat.Logic.Engine;
using Baccarat.Presentation.Animations;
using Baccarat.Presentation.Layout;
using Baccarat.Presentation.Rendering;

namespace Baccarat.Presentation.Controllers;

public class TableInputController
{
    private readonly BaccaratGameEngine _engine;
    private readonly TableLayout _layout;
    private readonly ChipRackRenderer _chipRack;
    private readonly DealingAnimationQueue _animQueue;
    private readonly ActionButtonsRenderer _buttonsRenderer;
    private readonly BankruptcyDialogRenderer _bankruptcyDialog;
    private readonly InputHelper _input;
    private readonly Action _onExit;

    public TableInputController(
        BaccaratGameEngine engine,
        TableLayout layout,
        ChipRackRenderer chipRack,
        DealingAnimationQueue animQueue,
        ActionButtonsRenderer buttonsRenderer,
        BankruptcyDialogRenderer bankruptcyDialog,
        InputHelper input,
        Action onExit)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _chipRack = chipRack ?? throw new ArgumentNullException(nameof(chipRack));
        _animQueue = animQueue ?? throw new ArgumentNullException(nameof(animQueue));
        _buttonsRenderer = buttonsRenderer ?? throw new ArgumentNullException(nameof(buttonsRenderer));
        _bankruptcyDialog = bankruptcyDialog ?? throw new ArgumentNullException(nameof(bankruptcyDialog));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _onExit = onExit ?? throw new ArgumentNullException(nameof(onExit));
    }

    public void Update()
    {
        _input.Update();

        // Exit on Escape or GamePad Back
        if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
        {
            _onExit();
            return;
        }

        // Button Visual Depression State
        if (_input.IsLeftJustPressed())
        {
            Point mousePos = _input.MousePosition;
            if (_layout.DealButtonArea.Contains(mousePos))
                _buttonsRenderer.PressedActionButton = ActionButtonType.Deal;
            else if (_layout.ClearButtonArea.Contains(mousePos))
                _buttonsRenderer.PressedActionButton = ActionButtonType.Clear;
            else if (_layout.RebetButtonArea.Contains(mousePos))
                _buttonsRenderer.PressedActionButton = ActionButtonType.Rebet;
        }
        else if (_input.IsLeftJustReleased())
        {
            _buttonsRenderer.PressedActionButton = ActionButtonType.None;
        }

        // Bankruptcy Dialog
        if (_bankruptcyDialog.IsActive)
        {
            if (_input.IsLeftJustReleased())
            {
                Point pos = _input.MousePosition;
                if (_layout.PlayAgainButtonArea.Contains(pos))
                    _engine.Bankroll.Add(1000m);
                else if (_layout.QuitButtonArea.Contains(pos))
                    _onExit();
            }
            return;
        }

        // Round Over Phase (Click anywhere to sweep and proceed)
        if (_engine.Phase == RoundPhase.RoundOver && !_animQueue.IsBusy)
        {
            if (_input.IsLeftJustReleased())
            {
                if (_engine.Shoe.HasCutCardAppeared)
                {
                    // Cut card reached: sweep cards and transition to Reshuffle banner
                    _animQueue.DiscardAllTo(_layout.DiscardPosition, () => _engine.PrepareReshuffle());
                }
                else
                {
                    // Normal round: sweep cards and return to Betting
                    _animQueue.DiscardAllTo(_layout.DiscardPosition, () => _engine.PrepareNextRound());
                }
            }

            return;
        }

        // Reshuffle Phase (Click anywhere to reshuffle shoe and start fresh)
        if (_engine.Phase == RoundPhase.Reshuffle && !_animQueue.IsBusy)
        {
            if (_input.IsLeftJustReleased())
            {
                _engine.ReshuffleShoe();
            }
            return;
        }

        // Chip Rack Selection
        if (_input.IsLeftJustPressed())
            _chipRack.HandleMouseDown(_input.MousePosition);

        if (_input.IsLeftJustReleased())
            _chipRack.HandleMouseUp(_input.MousePosition);

        // Betting Phase (Placing bets & Action buttons)
        if (_engine.Phase == RoundPhase.Betting && !_animQueue.IsBusy)
        {
            // Action button triggers on release
            if (_input.IsLeftJustReleased())
            {
                Point pos = _input.MousePosition;

                if (_layout.DealButtonArea.Contains(pos) && _engine.Table.HasBets && _engine.Bankroll.CanAfford(_engine.Table.TotalBetAmount))
                    _engine.PlayRound();
                else if (_layout.ClearButtonArea.Contains(pos) && _engine.Table.HasBets)
                    _engine.Table.Clear();
                else if (_layout.RebetButtonArea.Contains(pos) && _engine.Table.CanRebet && _engine.Bankroll.CanAfford(_engine.Table.LastRoundTotalBetAmount))
                    _engine.Table.RestoreLastRoundBets();
            }

            // Placing chips triggers on press down for instant responsiveness
            if (_input.IsLeftJustPressed())
            {
                int denom = _chipRack.SelectedDenomination;
                Point pos = _input.MousePosition;

                if (_engine.Bankroll.Balance >= _engine.Table.TotalBetAmount + denom)
                {
                    if (_layout.PlayerBetBox.Contains(pos))
                        _engine.Table.PlaceChip(BetType.Player, denom);
                    else if (_layout.BankerBetBox.Contains(pos))
                        _engine.Table.PlaceChip(BetType.Banker, denom);
                    else if (_layout.TieBetBox.Contains(pos))
                        _engine.Table.PlaceChip(BetType.Tie, denom);
                    else if (_layout.PlayerPairBetBox.Contains(pos))
                        _engine.Table.PlaceChip(BetType.PlayerPair, denom);
                    else if (_layout.BankerPairBetBox.Contains(pos))
                        _engine.Table.PlaceChip(BetType.BankerPair, denom);
                }
            }
        }
    }
}

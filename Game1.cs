using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shuffle.UI;
using Baccarat.Logic.Engine;
using Baccarat.Models.Cards;
using Baccarat.Models.Events;
using Baccarat.Presentation.Animations;
using Shuffle.UI.Assets;
using Shuffle.UI.Graphics;
using Baccarat.Presentation.Controllers;
using Baccarat.Presentation.Layout;
using Baccarat.Presentation.Rendering;

namespace Baccarat;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // Fonts
    private SpriteFont _tableFont = null!;
    private SpriteFont _balanceFont = null!;

    // Textures
    private Texture2D _backgroundTexture = null!;
    private Texture2D _goldButtonTexture = null!;
    private Texture2D _roundBannerTexture = null!;
    private Texture2D _bankruptcyDialogTexture = null!;
    private Texture2D _playerBadgeTexture = null!;
    private Texture2D _bankerBadgeTexture = null!;

    // Atlases
    private CardTextureAtlas _cardAtlas = null!;
    private ChipTextureAtlas _chipAtlas = null!;

    // Layout & Primitive Renderers
    private TableLayout _layout = null!;
    private CardQuadRenderer _quadRenderer = null!;

    // Logic, Animations & Input
    private BaccaratGameEngine _engine = null!;
    private readonly DealingAnimationQueue _animQueue = new();
    private readonly InputHelper _input = new();
    private bool _payoutApplied = false;

    // Renderers
    private ChipRackRenderer _chipRackRenderer = null!;
    private ActionButtonsRenderer _actionButtonsRenderer = null!;
    private BankruptcyDialogRenderer _bankruptcyDialogRenderer = null!;
    private HandScoreBadgeRenderer _handScoreBadgeRenderer = null!;
    private TableBannersRenderer _tableBannersRenderer = null!;
    private TableBetsRenderer _tableBetsRenderer = null!;
    private RoadmapRenderer _roadmapRenderer = null!;

    // Controller
    private TableInputController _inputController = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _engine = new BaccaratGameEngine(startingBalance: 1000m);
        _engine.CardDealt += OnCardDealt;

        _engine.RoundStarted += (sender, e) =>
        {
            _payoutApplied = false;
        };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // 1. Fonts
        _tableFont = Content.Load<SpriteFont>("Fonts/TableFont");
        _balanceFont = Content.Load<SpriteFont>("Fonts/BalanceFont");

        // 2. Textures
        using (var stream = File.OpenRead(Path.Combine(Content.RootDirectory, "Background", "table_background.png")))
            _backgroundTexture = Texture2D.FromStream(GraphicsDevice, stream);

        using (var stream = File.OpenRead(Path.Combine(Content.RootDirectory, "Buttons", "gold_button.png")))
            _goldButtonTexture = Texture2D.FromStream(GraphicsDevice, stream);

        using (var stream = File.OpenRead(Path.Combine(Content.RootDirectory, "Banners", "round_banner.png")))
            _roundBannerTexture = Texture2D.FromStream(GraphicsDevice, stream);

        using (var stream = File.OpenRead(Path.Combine(Content.RootDirectory, "UI", "bankruptcy_dialog.png")))
            _bankruptcyDialogTexture = Texture2D.FromStream(GraphicsDevice, stream);

        using (var stream = File.OpenRead(Path.Combine(Content.RootDirectory, "Badges", "player_badge.png")))
            _playerBadgeTexture = Texture2D.FromStream(GraphicsDevice, stream);

        using (var stream = File.OpenRead(Path.Combine(Content.RootDirectory, "Badges", "banker_badge.png")))
            _bankerBadgeTexture = Texture2D.FromStream(GraphicsDevice, stream);

        // 3. Texture Atlases
        string cardsPath = Path.Combine(Content.RootDirectory, "Cards");
        _cardAtlas = new CardTextureAtlas(GraphicsDevice, cardsPath);

        string chipsPath = Path.Combine(Content.RootDirectory, "Chips");
        _chipAtlas = new ChipTextureAtlas(GraphicsDevice, chipsPath);

        // 4. Layout & Slanted Card Quad Renderer
        _layout = new TableLayout(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _quadRenderer = new CardQuadRenderer(GraphicsDevice);

        // 5. Renderers
        _chipRackRenderer = new ChipRackRenderer(_chipAtlas);
        _chipRackRenderer.UpdateLayout(_layout.ChipRackArea);

        _actionButtonsRenderer = new ActionButtonsRenderer(_engine, _layout, _tableFont, _goldButtonTexture);
        _bankruptcyDialogRenderer = new BankruptcyDialogRenderer(_engine, _layout, _balanceFont, _tableFont, _bankruptcyDialogTexture, _backgroundTexture, GraphicsDevice, _animQueue);
        _handScoreBadgeRenderer = new HandScoreBadgeRenderer(_engine, _layout,  _tableFont, _playerBadgeTexture, _bankerBadgeTexture);
        _tableBannersRenderer = new TableBannersRenderer(_engine, _layout, _balanceFont, _tableFont, _roundBannerTexture, _backgroundTexture, GraphicsDevice, _animQueue);
        _tableBetsRenderer = new TableBetsRenderer(_engine, _layout, _chipAtlas, _balanceFont, _tableFont);
        _roadmapRenderer = new RoadmapRenderer(GraphicsDevice, _tableFont);

        // 6. Input Controller
        _inputController = new TableInputController(
            _engine,
            _layout,
            _chipRackRenderer,
            _animQueue,
            _actionButtonsRenderer,
            _bankruptcyDialogRenderer,
            _input,
            Exit
        );
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _animQueue.Update(dt);

        // Cards have finished dealing, banner is now visible -> award payout!
        if (!_payoutApplied && !_animQueue.IsBusy && _engine.Phase == RoundPhase.RoundOver)
        {
            _engine.ApplyPayout();
            _payoutApplied = true;
        }

        _inputController.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // 1. Table Background
        Rectangle screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _spriteBatch.Draw(_backgroundTexture, screenRect, Color.White);
        _tableBannersRenderer.DrawShoeCounter(_spriteBatch);

        // 2. Bankroll Information
        _tableBetsRenderer.DrawBankrollInfo(_spriteBatch);

        // 3. Chip Rack
        Point mousePos = Mouse.GetState().Position;
        bool isMouseDown = Mouse.GetState().LeftButton == ButtonState.Pressed;
        _chipRackRenderer.DrawRack(_spriteBatch, mousePos, isMouseDown);

        // 4. Betting Boxes & Stacked Chips
        _tableBetsRenderer.DrawAllBettingBoxes(_spriteBatch);

        // 5. Dealt Cards & Score Badges
        DrawDealtCards();

        // 6. Round Outcome Announcement Banner
        _tableBannersRenderer.DrawRoundOverAnnouncement(_spriteBatch);

        // 7. Action Buttons (Deal / Clear / Rebet)
        _actionButtonsRenderer.DrawActionButtons(_spriteBatch, mousePos, isMouseDown);

        // 8. Roadmaps (Bead Plate & Big Road)
        _roadmapRenderer.DrawBeadPlate(_spriteBatch, _engine.BeadPlate, _layout.BeadPlateArea);
        _roadmapRenderer.DrawBigRoad(_spriteBatch, _engine.BigRoad, _layout.BigRoadArea);

        // 9. Status Banners, Modals & Headings
        _bankruptcyDialogRenderer.DrawBankruptcyPrompt(_spriteBatch);
        _tableBannersRenderer.DrawRoadmapHeadings(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawDealtCards()
    {
        if (_engine.Phase == RoundPhase.Betting && _engine.PlayerHand.Count == 0)
            return;

        int cardWidth = (int)(_layout.ScreenWidth * 0.075f);
        int cardHeight = (int)(cardWidth * 1.44f);

        _spriteBatch.End();

        // Draw animated/landed cards using 3D quad renderer
        _animQueue.Draw(_quadRenderer, _cardAtlas, cardWidth, cardHeight);

        _spriteBatch.Begin();

        // Reveal score badges once animation queue finishes
        if (!_animQueue.IsBusy)
        {
            _handScoreBadgeRenderer.DrawHandScoreBadges(_spriteBatch);
        }
    }

    private void OnCardDealt(object? sender, CardDealtEventArgs e)
    {
        Vector2 targetPos = _layout.GetCardSlotPosition(e.Target, e.CardIndex);
        float slant = _layout.GetCardSlotSlant(e.Target, e.CardIndex);

        CardAnimation anim = new CardAnimation(e.Card, _layout.ShoePosition, targetPos, durationSeconds: 0.55f, slant);
        _animQueue.Enqueue(anim);
    }

    protected override void UnloadContent()
    {
        _backgroundTexture?.Dispose();
        _goldButtonTexture?.Dispose();
        _roundBannerTexture?.Dispose();
        _bankruptcyDialogTexture?.Dispose();
        _playerBadgeTexture?.Dispose();
        _bankerBadgeTexture?.Dispose();
        _cardAtlas?.Dispose();
        _chipAtlas?.Dispose();
        _quadRenderer?.Dispose();

        base.UnloadContent();
    }
}

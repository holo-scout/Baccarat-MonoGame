using Microsoft.Xna.Framework;
using Baccarat.Models.Betting;
using Baccarat.Models.Events;

namespace Baccarat.Presentation.Layout;

public class TableLayout
{
    public int ScreenWidth { get; private set; }
    public int ScreenHeight { get; private set; }

    // Betting Hitboxes
    public Rectangle PlayerBetBox { get; private set; }
    public Rectangle BankerBetBox { get; private set; }
    public Rectangle TieBetBox { get; private set; }
    public Rectangle PlayerPairBetBox { get; private set; }
    public Rectangle BankerPairBetBox { get; private set; }

    // Card Positions
    public Vector2[] PlayerCardSlots { get; private set; } = new Vector2[3];
    public Vector2[] BankerCardSlots { get; private set; } = new Vector2[3];
    public Vector2 ShoePosition { get; private set; }
    public Rectangle ShoeCounter { get; private set; }
    public Vector2 DiscardPosition { get; private set; }

    // Card Slant Angles
    public float[] PlayerCardSlants { get; } = new float[] { 8f, 5f, 3f };
    public float[] BankerCardSlants { get; } = new float[] { -3f, -5f, -8f };

    // Bottom Dock
    public Rectangle BalanceArea { get; private set; }
    public Rectangle ChipRackArea { get; private set; }
    public Rectangle BeadPlateArea { get; private set; }
    public Rectangle BigRoadArea { get; private set; }
    public Rectangle RebetButtonArea { get; private set; }
    public Rectangle ClearButtonArea { get; private set; }
    public Rectangle DealButtonArea { get; private set; }

    // Bankruptcy Modal Hitboxes
    public Rectangle BankruptcyDialogArea { get; private set; }
    public Rectangle PlayAgainButtonArea { get; private set; }
    public Rectangle QuitButtonArea { get; private set; }

    public TableLayout(int screenWidth, int screenHeight)
    {
        UpdateLayout(screenWidth, screenHeight);
    }

    public void UpdateLayout(int w, int h)
    {
        ScreenWidth = w;
        ScreenHeight = h;

        // Button size
        int btnSize = (int)(h * 0.14f); 
        int btnY = (int)(h * 0.84f);

        // Betting zones
        PlayerPairBetBox = new Rectangle((int)(w * 0.02f), (int)(h * 0.28f), (int)(w * 0.17f), (int)(h * 0.26f));
        BankerPairBetBox = new Rectangle((int)(w * 0.81f), (int)(h * 0.28f), (int)(w * 0.17f), (int)(h * 0.26f));
        PlayerBetBox = new Rectangle((int)(w * 0.02f), (int)(h * 0.55f), (int)(w * 0.32f), (int)(h * 0.21f));
        TieBetBox = new Rectangle((int)(w * 0.35f), (int)(h * 0.55f), (int)(w * 0.29f), (int)(h * 0.21f));
        BankerBetBox = new Rectangle((int)(w * 0.65f), (int)(h * 0.55f), (int)(w * 0.33f), (int)(h * 0.21f));
        
        // Card slots
        PlayerCardSlots[0] = new Vector2(w * 0.260f, h * 0.37f);
        PlayerCardSlots[1] = new Vector2(w * 0.345f, h * 0.37f);
        PlayerCardSlots[2] = new Vector2(w * 0.430f, h * 0.37f);
        BankerCardSlots[0] = new Vector2(w * 0.575f, h * 0.37f);
        BankerCardSlots[1] = new Vector2(w * 0.660f, h * 0.37f);
        BankerCardSlots[2] = new Vector2(w * 0.745f, h * 0.37f);

        // Shoe and discard pile
        ShoePosition = new Vector2(w * 0.12f, h * 0.16f);
        ShoeCounter = new Rectangle((int)(w * 0.165f), (int)(h * 0.13f), 110, 52);
        DiscardPosition = new Vector2(w * 0.88f, h * 0.16f);

        // Bottom dock
        BalanceArea = new Rectangle((int)(w * 0.02f), (int)(h * 0.79f), (int)(w * 0.18f), (int)(h * 0.15f));
        ChipRackArea = new Rectangle((int)(w * 0.02f), (int)(h * 0.80f), (int)(w * 0.36f), (int)(h * 0.22f));
        BeadPlateArea = new Rectangle((int)(w * 0.42f), (int)(h * 0.86f), (int)(w * 0.12f), (int)(h * 0.13f));
        BigRoadArea = new Rectangle((int)(w * 0.55f), (int)(h * 0.86f), (int)(w * 0.18f), (int)(h * 0.13f));
        RebetButtonArea = new Rectangle((int)(w * 0.765f), btnY, btnSize, btnSize);
        ClearButtonArea = new Rectangle((int)(w * 0.840f), btnY, btnSize, btnSize);
        DealButtonArea = new Rectangle((int)(w * 0.915f), btnY, btnSize, btnSize);

        // Bankruptcy Modal Layout
        int dialogWidth = (int)(w * 0.46f);
        int dialogHeight = (int)(dialogWidth * (682f / 1024f));
        int dialogX = (w - dialogWidth) / 2;
        int dialogY = (h - dialogHeight) / 2 - 20;

        BankruptcyDialogArea = new Rectangle(dialogX, dialogY, dialogWidth, dialogHeight);

        int bankrupt_btnY = dialogY + (int)(dialogHeight * (461f / 682f));
        int btnH = (int)(dialogHeight * (138f / 682f));
        int btnW = (int)(dialogWidth * (491f / 1024f));

        PlayAgainButtonArea = new Rectangle(dialogX + (int)(dialogWidth * (19f / 1024f)), bankrupt_btnY, btnW, btnH);
        QuitButtonArea = new Rectangle(dialogX + (int)(dialogWidth * (513f / 1024f)), bankrupt_btnY, btnW, btnH);
    }

    // Checks if mouse click was inside a betting zone
    public BetType? GetBetTypeAt(Point mousePosition)
    {
        if (PlayerBetBox.Contains(mousePosition)) return BetType.Player;
        if (BankerBetBox.Contains(mousePosition)) return BetType.Banker;
        if (TieBetBox.Contains(mousePosition)) return BetType.Tie;
        if (PlayerPairBetBox.Contains(mousePosition)) return BetType.PlayerPair;
        if (BankerPairBetBox.Contains(mousePosition)) return BetType.BankerPair;

        return null;
    }

    // Determines where a dealt card should be placed
    public Vector2 GetCardSlotPosition(HandTarget target, int index)
    {
        if (index < 0 || index >= 3) 
            index = 0;

        return target == HandTarget.Player ? PlayerCardSlots[index] : BankerCardSlots[index];
    }

    // Gets card slant angles
    public float GetCardSlotSlant(HandTarget target, int index)
    {
        if (index < 0 || index >= 3) index = 0;
        return target == HandTarget.Player ? PlayerCardSlants[index] : BankerCardSlants[index];
    }
}


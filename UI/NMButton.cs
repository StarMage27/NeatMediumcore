using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using NeatMediumcore.Config;
using Terraria.ID;

namespace NeatMediumcore.UI;

public class NMButton : ModSystem
{
    internal UserInterface NMInterface;
    internal NMUI nMUI;
    private GameTime lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        if (NMInterface?.CurrentState == null) return;
        NMInterface.Update(gameTime);
            
        lastUpdateUiGameTime = gameTime;

        if (Main.playerInventory)
        {
            ShowMyUI();
        }
        else
        {
            HideMyUI();
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        bool showButtonsToggle = ModContent.GetInstance<NMConfig>().ShowButtonsToggle;

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        if (mouseTextIndex != -1)
        {
            bool drawMethod()
            {
                if (lastUpdateUiGameTime != null && NMInterface?.CurrentState != null)
                {
                    NMInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);
                }
                    
                return true;
            }

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("MyMod: MyInterface", drawMethod, InterfaceScaleType.UI));
        }
            
        if (showButtonsToggle && Main.playerInventory && Main.netMode == NetmodeID.MultiplayerClient && Main.LocalPlayer.chest == -1 && Main.LocalPlayer.TalkNPC == null)
        {
            ShowMyUI();
        }
        else
        {
            HideMyUI();
        }

    }

    internal void ShowMyUI() => NMInterface?.SetState(nMUI);

    internal void HideMyUI() => NMInterface?.SetState(null);

    public override void Load()
    {
        if (!Main.dedServ)
        {
            NMInterface = new UserInterface();

            nMUI = new NMUI();
            nMUI.Activate();
        }

        ShowMyUI();
    }

    public override void Unload()
    {
        nMUI?.Unload();
        nMUI = null;

        HideMyUI();
    }
}
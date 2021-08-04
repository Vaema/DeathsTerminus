using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;



namespace DeathsTerminus
{
	public class DeathsTerminus : Mod
	{
		public DeathsTerminus()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
        {
            On.Terraria.Main.DrawInterface_Resources_Life += PostDrawLifeBar;
        }

        //to make it not draw the vanilla hearts, just don't do orig.Invoke()
        private void PostDrawLifeBar(On.Terraria.Main.orig_DrawInterface_Resources_Life orig)
        {
            orig.Invoke();

            if (Main.LocalPlayer.ghost) return;

            if (Main.player[Main.myPlayer].statLifeMax2 == 1 && Main.player[Main.myPlayer].GetModPlayer<DTPlayer>().mysteriousPresence)
            {
                Texture2D heartTexture = GetTexture("Buffs/MysteriousHeart");

                //a bunch of nonsense adapted from DrawInterface_Resources_Life
                int num14 = 255;
                float num13 = 1f;
                bool flag = false;
                if ((float)Main.player[Main.myPlayer].statLife >= 1)
                {
                    num14 = 255;
                    if ((float)Main.player[Main.myPlayer].statLife == 1)
                    {
                        flag = true;
                    }
                }
                else
                {
                    float num11 = ((float)Main.player[Main.myPlayer].statLife);
                    num14 = (int)(30f + 225f * num11);
                    if (num14 < 30)
                    {
                        num14 = 30;
                    }
                    num13 = num11 / 4f + 0.75f;
                    if ((double)num13 < 0.75)
                    {
                        num13 = 0.75f;
                    }
                    if (num11 > 0f)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    num13 += Main.cursorScale - 1f;
                }

                int a = (int)((double)(float)num14 * 0.9);

                Main.spriteBatch.Draw(heartTexture, new Vector2((float)(500 + Main.screenWidth - 800 + heartTexture.Width / 2), 32f + ((float)heartTexture.Height - (float)heartTexture.Height * num13) / 2f + (float)(heartTexture.Height / 2)), (Rectangle?)new Rectangle(0, 0, heartTexture.Width, heartTexture.Height), new Color(num14, num14, num14, a), 0f, new Vector2((float)(heartTexture.Width / 2), (float)(heartTexture.Height / 2)), num13, (SpriteEffects)0, 0f);
            }
        }

    }
}

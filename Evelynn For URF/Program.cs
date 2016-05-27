using System;
using System.Drawing;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

// ReSharper disable InconsistentNaming

namespace Evelynn_For_URF
{
    internal static class Program
    {
        private static Spell Q, W, E, R;
        private static Menu Menu;

        private static Orbwalking.Orbwalker Orbwalker;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!HeroManager.Player.ChampionName.Equals("Evelynn", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //if (HeroManager.Player.PercentCooldownMod < -0.8f)
            if (!HeroManager.Player.HasBuff("AwesomeBuff"))
            {
                Game.PrintChat("xcsoft: it's not URF Mode !");
                return;
            }

            //Set Menu
            Menu = new Menu($"{HeroManager.Player.ChampionName} For URF", $"{HeroManager.Player.ChampionName}forurfxcsoft", true);
            Orbwalker = new Orbwalking.Orbwalker(Menu);
            Menu.AddItem(new MenuItem("txt1", "Just Press Combo and Laneclear Keys !"));
            Menu.AddItem(new MenuItem("txt2", "Have a Fun !!"));
            Menu.AddItem(new MenuItem("txt3", "-xcsoft"));
            Menu.AddToMainMenu();

            //Set Spells
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 335f);
            R = new Spell(SpellSlot.R, 650f);

            R.SetSkillshot(0.25f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            //Set Events
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat($"xcsoft: {HeroManager.Player.ChampionName} For URF Loaded!");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (HeroManager.Player.IsDead)
            {
                return;
            }

            //Do Modes
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Q.Cast();
                    W.Cast();
                    E.CastOnBestTarget();
                    R.CastOnBestTarget(0f, false, true);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Q.Cast();
                    E.Cast(MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault());
                    break;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (HeroManager.Player.IsDead)
            {
                return;
            }

            Render.Circle.DrawCircle(HeroManager.Player.Position, Q.Range, Color.Chartreuse);
            Render.Circle.DrawCircle(HeroManager.Player.Position, E.Range, Color.Chartreuse);
            Render.Circle.DrawCircle(HeroManager.Player.Position, R.Range, Color.Chartreuse);
        }
    }
}

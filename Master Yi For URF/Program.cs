using System;
using System.Drawing;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

// ReSharper disable InconsistentNaming

namespace Master_Yi_For_URF
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
            if (!HeroManager.Player.ChampionName.Equals("MasterYi", StringComparison.OrdinalIgnoreCase))
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
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            //Set Events
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

            Game.PrintChat($"xcsoft: {HeroManager.Player.ChampionName} For URF Loaded!");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (HeroManager.Player.IsDead)
            {
                return;
            }

            //Do Modes
            if (Orbwalking.CanMove(10))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Q.CastOnBestTarget();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Q.Cast(MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault());
                        break;
                }
            }

            //Do Fast KillSteal
            var qKillableTarget = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() && x.Health + x.PhysicalShield + x.HPRegenRate <= Q.GetDamage(x));
            if (qKillableTarget != null)
            {
                Q.Cast(qKillableTarget);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (HeroManager.Player.IsDead)
            {
                return;
            }

            Render.Circle.DrawCircle(HeroManager.Player.Position, Q.Range, Color.Chartreuse);
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!args.Unit.IsMe)
            {
                return;
            }

            if (!HeroManager.Player.IsTargetable)
            {
                args.Process = false;
            }

            if (args.Target is Obj_AI_Hero)
            {
                R.Cast();
            }

            E.Cast();
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (target.IsValidTarget() && HeroManager.Enemies.Any(Orbwalking.InAutoAttackRange))
            {
                W.Cast();
            }
        }

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Animation.Equals("Spell1", StringComparison.OrdinalIgnoreCase))
            {
                Orbwalking.ResetAutoAttackTimer();
            }

            if (args.Animation.Equals("Spell2", StringComparison.OrdinalIgnoreCase)
                && (Orbwalker.ActiveMode.HasFlag(Orbwalking.OrbwalkingMode.Combo) || Orbwalker.ActiveMode.HasFlag(Orbwalking.OrbwalkingMode.LaneClear))
                && HeroManager.Player.CountEnemiesInRange(1000f) > 0)
            {
                HeroManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                Orbwalking.ResetAutoAttackTimer();
            }
        }
    }
}

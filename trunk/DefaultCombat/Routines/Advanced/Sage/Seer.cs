﻿using Buddy.BehaviorTree;
using DefaultCombat.Core;
using DefaultCombat.Helpers;

namespace DefaultCombat.Routines
{
    class Seer : RotationBase
    {
        public override string Name { get { return "Sage Seer"; } }

        public override Composite Buffs
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Force Valor")
                    );
            }
        }

        public override Composite Cooldowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Force Potency", ret => Targeting.ShouldAOEHeal),
                    Spell.Buff("Noble Sacrifice", ret => NeedSacrifice()),
                    Spell.Buff("Force Mend", ret => Me.HealthPercent <= 75)
                    );
            }
        }

        public override Composite SingleTarget
        {
            get
            {
                return new LockSelector(
                    //Movement
                    CombatMovement.CloseDistance(Distance.Ranged),

                    Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !DefaultCombat.MovementDisabled),
                    Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !DefaultCombat.MovementDisabled),

                    Spell.Cast("Vanquish", ret => Me.BuffCount("Presence of Mind") == 4 && Me.Level >= 57),
                    Spell.Cast("Mind Crush", ret => Me.BuffCount("Presence of Mind") == 4 && Me.Level < 57),
                    Spell.DoT("Weaken Mind", "Weaken Mind"),
                    Spell.DoT("Sever Force", "Sever Force"),
                    Spell.CastOnGround("Force in Balance", ret => Me.CurrentTarget.HasDebuff("Weaken Mind") && Me.CurrentTarget.HasDebuff("Sever Force")),
                    Spell.Cast("Force Serenity", ret => Me.CurrentTarget.HasDebuff("Weaken Mind")),
                    Spell.Cast("Disturbance", ret => Me.BuffCount("Presence of Mind") == 4),
                    Spell.Cast("Telekinetic Throw", ret => Me.BuffCount("Presence of Mind") < 4)
                    );
            }
        }

        public override Composite AreaOfEffect
        {
            get
            {
                return new LockSelector(
                    //Cleanse if needed
                    Spell.Cleanse("Restoration"),

                    //Emergency Heal (Insta-cast)
                    Spell.Heal("Benevolence", 40, ret => Me.HasBuff("Altruism")),

                    //Aoe Heal
                    Spell.HealGround("Salvation", ret => Targeting.ShouldAOE),

                    //Single Target Healing
                    Spell.Heal("Healing Trance", 80),
                    Spell.Heal("Force Armor", 90,  ret => HealTarget != null && !HealTarget.HasDebuff("Force-imbalance") && !HealTarget.HasBuff("Force Armor")),     

                    //Buff Tank
                    Spell.Heal("Force Armor", on => Tank, 100, ret => Tank != null && Tank.InCombat && !Tank.HasDebuff("Force-imbalance") && !Tank.HasBuff("Force Armor")),
                    
                    //Use Force Bending
                    new Decorator(ret => Me.HasBuff("Conveyance"),
                        new LockSelector(
                            Spell.Heal("Healing Trance", 90),
                            Spell.Heal("Deliverance", 50)
                        )),
                        
                    //Build Force Bending
                    Spell.HoT("Rejuvenate", 80),
                    Spell.HoT("Rejuvenate", on => Tank, 100, ret => Tank != null && Tank.InCombat),

                    //Single Target Healing                  
                    Spell.Heal("Benevolence", 35),
                    Spell.Heal("Deliverance", 80)
                );
            }
        }

        private bool NeedSacrifice()
        {
            if (Me.ForcePercent <= 20 && Me.HealthPercent >= 16)
                return true;
            if (Me.HasBuff("Resplendence") && Me.HealthPercent >= 16 && Me.ForcePercent < 90)
                return true;
            return false;
        }
    }
}
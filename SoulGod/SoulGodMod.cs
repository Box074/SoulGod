global using UObject = UnityEngine.Object;

using FSMProxy;
using HKTool;
using HKTool.FSM;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using UnityEngine;

namespace SoulGod
{
    class SoulGodMod : ModBase<SoulGodMod>
    {
        [Preload("GG_Soul_Tyrant", "Dream Mage Lord/Orb Spinner")]
        public GameObject SuperOrbSpinner = null!;

        [Preload("GG_Soul_Master", "Mage Lord/Orb Spinner")]
        public GameObject OrbSpinner = null!;

        [PreloadSharedAssets(34, "Mage Orb")]
        public GameObject MageOrbPrefab = null!;

        bool hitBySM = false;

        public override void Initialize()
        {
            On.HeroController.TakeDamage += HeroController_TakeDamage;
            ModHooks.TakeHealthHook += ModHooks_TakeHealthHook;
        }

        private int ModHooks_TakeHealthHook(int damage)
        {
            if(damage > 0 && hitBySM)
            {
                HeroController.instance.TakeMP(33);
            }
            return damage;
        }

        private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, 
            HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, 
            int damageAmount, int hazardType)
        {
            try
            {
                var t = go.transform;
                while(t != null)
                {
                    if(t.gameObject.name == "Mage Lord"
                        || t.gameObject.name == "Dream Mage Lord"
                        || t.gameObject.name == "Mage Lord Phase2"
                        || t.gameObject.name == "Dream Mage Lord Phase2"
                        || t.gameObject.name.StartsWith("Mage Orb"))
                    {
                        hitBySM = true;
                        break;
                    }
                    t = t.parent;
                }
                orig(self, go, damageSide, damageAmount, hazardType);
            }
            finally
            {
                hitBySM = false;
            }
        }

        [FsmPatcher("", "Mage Lord", "Mage Lord")]
        [FsmPatcher("", "Dream Mage Lord", "Mage Lord")]
        private void PatchSoulMaster(PlayMakerFSM pm)
        {
            var fsm = SoulMasterFSM.Apply(pm);
            var proxy = new FSMProxy_SoulMaster(pm);
            var oldspinner = proxy.Variables.Orb_Spinner.Value;
            GameObject newspinner;
            if (pm.gameObject.name == "Dream Mage Lord")
            {
                fsm.BaseHP = 1000;
                SoulTyrantFSM.Apply(pm);

                newspinner = new GameObject("Spinner Group");

                FsmEventProxy.Attach(newspinner, out var p, "Init");

                void SpawnSpinner(float rotateZ)
                {
                    var sp1 = UObject.Instantiate(OrbSpinner, newspinner.transform);
                    p.proxy.targets.Add(sp1);

                    var sspm = sp1.LocateMyFSM("Spin Control");
                    sspm.Fsm.RestartOnEnable = true;
                    var s = sspm.Fsm.GetState("Idle");
                    s.GetFSMStateActionOnState<Rotate>().zAngle = rotateZ;
                    s.SaveActions();

                    s = sspm.Fsm.GetState("Spin");
                    s.GetFSMStateActionOnState<Rotate>().zAngle = rotateZ;
                    s.SaveActions();
                    
                    sp1.SetActive(true);
                }

                SpawnSpinner(270);
                SpawnSpinner(-270);
                
            }
            else
            {
                newspinner = UObject.Instantiate(SuperOrbSpinner, oldspinner.transform.parent);
            }
            
            newspinner.transform.parent = oldspinner.transform.parent;
            newspinner.transform.localPosition = oldspinner.transform.localPosition;
            newspinner.transform.localScale = oldspinner.transform.localScale;
            
            UObject.Destroy(oldspinner);
            proxy.Variables.Orb_Spinner.Value = newspinner;
            newspinner.SetActive(true);
        }

        [FsmPatcher("", "Mage Lord Phase2", "Mage Lord 2")]
        [FsmPatcher("", "Dream Mage Lord Phase2", "Mage Lord 2")]
        private void PatchSoulMasterP2(PlayMakerFSM pm)
        {
            SoulMasterP2FSM.Apply(pm);
        }
    }
}

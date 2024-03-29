﻿global using UObject = UnityEngine.Object;

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

        [PreloadSharedAssets("Audio Player Actor")]
        public AudioSource audioPlayer = null!;

        [PreloadSharedAssets(102, "Soul_Master_Cast_03")]
        public AudioClip Soul_Master_Cast_03 = null!;

        [PreloadSharedAssets(102, "mage_lord_projectile_charge")]
        public AudioClip mage_lord_projectile_charge = null!;

        [PreloadSharedAssets(34, "Electro Zap")]
        public GameObject zapPrefab = null!;

        bool hitBySM = false;

        public static void SetSpinnerRotate(GameObject spinner, float z)
        {
            var sspm = spinner.LocateMyFSM("Spin Control");
            sspm.Fsm.RestartOnEnable = true;
            var s = sspm.Fsm.GetState("Idle");
            s.GetFSMStateActionOnState<Rotate>().zAngle = z;
            s.SaveActions();

            s = sspm.Fsm.GetState("Spin");
            s.GetFSMStateActionOnState<Rotate>().zAngle = z;
            s.SaveActions();
        }

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

        [FsmPatcher("GG_Soul_Master", "Mage Lord", "Mage Lord")]
        [FsmPatcher("GG_Soul_Tyrant", "Dream Mage Lord", "Mage Lord")]
        private void PatchSoulMaster(PlayMakerFSM pm)
        {
            GameObject.Find("GG_Arena_Prefab/Crowd")?.SetActive(false);
            GameObject.Find("GG_Arena_Prefab/BG/throne")?.transform.SetPositionY(0.79f);
            GameObject.Find("GG_Arena_Prefab/Godseeker Crowd")?.transform.SetPositionY(6.8f);

            var fsm = SoulMasterFSM.Apply(pm);
            var proxy = new FSMProxy_SoulMaster(pm);
            var oldspinner = proxy.Variables.Orb_Spinner.Value;
            GameObject newspinner;
            if (pm.gameObject.name == "Dream Mage Lord")
            {
                fsm.isTyrant = true;
                fsm.BaseHP = 850;
                SoulTyrantFSM.Apply(pm);

                newspinner = new GameObject("Spinner Group");

                FsmEventProxy.Attach(newspinner, out var p, "Init");

                void SpawnSpinner(float rotateZ)
                {
                    var sp1 = UObject.Instantiate(OrbSpinner, newspinner.transform);
                    p.proxy.targets.Add(sp1);

                    SetSpinnerRotate(sp1, rotateZ);

                    sp1.SetActive(true);
                }

                SpawnSpinner(240);
                SpawnSpinner(-240);
                
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

        [FsmPatcher("GG_Soul_Master", "Mage Lord Phase2", "Mage Lord 2")]
        [FsmPatcher("GG_Soul_Tyrant", "Dream Mage Lord Phase2", "Mage Lord 2")]
        private void PatchSoulMasterP2(PlayMakerFSM pm)
        {
            SoulMasterP2FSM.Apply(pm);
        }
    }
}

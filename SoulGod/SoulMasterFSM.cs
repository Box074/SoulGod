using FSMProxy;
using HKTool.FSM;
using HKTool.FSM.CSFsm;
using HKTool.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoulGod
{
    internal class SoulMasterFSM : CSFsm<SoulMasterFSM>
    {
        public int BaseHP = 500;
        public bool isTyrant = false;
        [FsmVar(FSMProxy_SoulMaster.VariableNames.Projectile)]
        public FsmGameObject MageOrbInst = new();

        [ComponentBinding]
        public tk2dSpriteAnimator anim = null!;
        [ComponentBinding]
        public AudioSource audio = null!;

        public GameObject orbL = null!;
        public GameObject orbR = null!;

        public FSMProxy_SoulMaster proxy = null!;

        [ComponentBinding]
        public HealthManager hm = null!;
        protected override void OnAfterBindPlayMakerFSM()
        {
            base.OnAfterBindPlayMakerFSM();

            proxy = new(FsmComponent);

            FsmComponent.SetState(nameof(SoulMasterInit));

            FsmComponent.Fsm
                .GetState(FSMProxy_SoulMaster.StateNames.Shot)
                .AppendFsmStateAction(new InvokeAction(() =>
                {
                    MageOrbInst.Value.AddComponent<MageOrbControl>().canTouchWall = false;
                }));
            FsmComponent.Fsm
                .GetState(FSMProxy_SoulMaster.StateNames.Quake_Waves)
                .RemoveAllFsmStateActions<Wait>();

            FsmComponent.Fsm
                .GetState(FSMProxy_SoulMaster.StateNames.Quake_Antic)
                .AppendFsmStateAction(new InvokeAction(() =>
                {
                    GameObject FollowOrb(float offset, GameObject old)
                    {
                        if (old != null)
                        {
                            old.transform.parent = null;
                        }
                        var orb = Instantiate(SoulGodMod.Instance.MageOrbPrefab, transform);
                        var ctrl = orb.AddComponent<MageOrbControl>();
                        ctrl.offset = new(offset, 1);
                        ctrl.canTouchWall = true;

                        FSMUtility.SendEventToGameObject(orb, "FIRE");
                        return orb;
                    }
                    orbL = FollowOrb(-2, orbL);
                    orbR = FollowOrb(2, orbR);
                }));
            FsmComponent.Fsm
                .GetState(FSMProxy_SoulMaster.StateNames.Quake_Land)
                .AppendFsmStateAction(new InvokeAction(() =>
                {
                    orbL.transform.parent = null;
                    orbR.transform.parent = null;
                    orbL = null;
                    orbR = null;
                }));

            var ac = FsmComponent.Fsm
                .GetState(FSMProxy_SoulMaster.StateNames.Attack_Choice)
                .GetFSMStateActionOnState<SendRandomEventV3>();

            ac.events = ac.events
                .Append(FsmEvent.GetFsmEvent("SMG SUPER ORB SHOT"))
                .ToArray();
            ac.weights = ac.weights
                .Append(0.35f)
                .ToArray();
            ac.eventMax = ac.eventMax
                .Append(1)
                .ToArray();
            ac.missedMax = ac.missedMax
                .Append(6)
                .ToArray();

            ac.trackingInts = ac.trackingInts
                .Append(0)
                .ToArray();
            ac.trackingIntsMissed = ac.trackingIntsMissed
                .Append(0)
                .ToArray();
        }

        [FsmState]
        private IEnumerator SoulMasterInit()
        {
            DefineEvent(FsmEvent.Finished, "Pause");
            yield return StartActionContent;
            yield return new WaitForSeconds(0.1f);

            hm.hp = BaseHP + 500;
        }

        [FsmState]
        private IEnumerator SuperOrbShot()
        {
            DefineGlobalEvent("SMG SUPER ORB SHOT");
            DefineEvent("TELE", FSMProxy_SoulMaster.StateNames.Teleport);
            DefineEvent(FsmEvent.Finished, FSMProxy_SoulMaster.StateNames.Reactivate);

            AudioEvent AE_summon = new()
            {
                Clip = SoulGodMod.Instance.Soul_Master_Cast_03,
                PitchMax = 1f,
                PitchMin = 1f,
                Volume = 1
            };

            yield return StartActionContent;
            bool useSecond = isTyrant && UnityEngine.Random.value < 0.6f;
            if (transform.position.y != proxy.Variables.Top_Y.Value)
            {
                var pos = new Vector2(20, proxy.Variables.Top_Y.Value);
                proxy.Variables.Tele_X.Value = pos.x;
                proxy.Variables.Tele_Y.Value = pos.y;
                proxy.Variables.Teleport_Point.Value = pos;
                proxy.Variables.Next_Event.Value = "SMG SUPER ORB SHOT";
                yield return "TELE";
            }
            anim.Play("Summon");
            AE_summon.SpawnAndPlayOneShot(SoulGodMod.Instance.audioPlayer, transform.position);

            GameObject SpawnSpinner(float z)
            {
                var s = Instantiate((isTyrant && !useSecond) ? SoulGodMod.Instance.SuperOrbSpinner :
                    SoulGodMod.Instance.OrbSpinner, transform);
                s.SetActive(true);
                FSMUtility.SendEventToGameObject(s, "SPINNER SUMMON");
                SoulGodMod.SetSpinnerRotate(s, z);
                return s;
            }
            var group = new List<GameObject>();
            for (int i = 0; i < (useSecond ? 2 : 4); i++)
            {
                if (i % 2 == 0)
                {
                    group.Add(SpawnSpinner(-220 - (i / 2 * 10)));
                }
                else
                {
                    group.Add(SpawnSpinner(220 + ((i + 1) / 2 * 10)));
                }
                yield return new WaitForSeconds(0.75f);
            }


            yield return new WaitForSeconds(useSecond ? 1.25f : 3.45f);

            var orbs = group.SelectMany(x => x.transform.Cast<Transform>())
                .Select(x => x.gameObject).ToArray();
            SoulGodMod.Instance.Log("Total Mage Orb Count: " + orbs.Length);
            int count = orbs.Length;
            foreach (var orb in orbs)
            {
                count--;
                var o = Instantiate(SoulGodMod.Instance.MageOrbPrefab, orb.transform.position,
                    orb.transform.localRotation);
                orb.Recycle();
                FSMUtility.SendEventToGameObject(o, "FIRE");

                if (!useSecond)
                {
                    yield return new WaitForSeconds(1.35f 
                        * UnityEngine.Random.value
                        * (count < 5 ? (5 - count) * 0.2f : 1));
                }
            }
            foreach (var g in group)
            {
                Destroy(g);
            }
            if (useSecond)
            {
                var pos = new Vector2(20, proxy.Variables.Top_Y.Value + 30);
                proxy.Variables.Tele_X.Value = pos.x;
                proxy.Variables.Tele_Y.Value = pos.y;
                proxy.Variables.Teleport_Point.Value = pos;
                proxy.Variables.Next_Event.Value = "SMG SUPER ORB SHOT 2";
                yield return "TELE";
            }
            anim.Play("SummonToIdle");
            yield return new WaitForSeconds(1.25f);

        }
        [FsmState]
        private IEnumerator SuperOrbShot2()
        {
            DefineGlobalEvent("SMG SUPER ORB SHOT 2");
            DefineEvent(FsmEvent.Finished, FSMProxy_SoulMaster.StateNames.Tele_Away);
            yield return StartActionContent;
            yield return new WaitForSeconds(2.5f);

        }
    }
}

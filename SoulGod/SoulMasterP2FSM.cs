﻿using FSMProxy;
using HKTool.FSM;
using HKTool.FSM.CSFsm;
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SoulGod
{
    internal class SoulMasterP2FSM : CSFsm<SoulMasterP2FSM>
    {
        public GameObject orbL;
        public GameObject orbR;

        [ComponentBinding]
        public tk2dSpriteAnimator anim;
        protected override void OnAfterBindPlayMakerFSM()
        {

            FsmComponent.Fsm
                .GetState(FSMProxy_SoulMasterP2.StateNames.Quake_Antic)
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
                        ctrl.canTouchWall = false;

                        FSMUtility.SendEventToGameObject(orb, "FIRE");
                        return orb;
                    }
                    orbL = FollowOrb(-3, orbL);
                    orbR = FollowOrb(3, orbR);
                }));
            FsmComponent.Fsm
                .GetState(FSMProxy_SoulMasterP2.StateNames.Quake_Land)
                .AppendFsmStateAction(new InvokeAction(() =>
                {
                    orbL.transform.parent = null;
                    orbR.transform.parent = null;
                    orbL = null;
                    orbR = null;
                }));
        }

        [FsmState(FSMProxy_SoulMasterP2.StateNames.Fireball_Pos)]
        private IEnumerator ClockAttack()
        {
            DefineEvent(FsmEvent.Finished, FSMProxy_SoulMasterP2.StateNames.Shot_CD);
            yield return StartActionContent;
            var sp = new GameObject();
            sp.transform.position = transform.position;
            sp.AddComponent<SpinnerControl>();
            var sp2 = Instantiate(sp);
            sp2.transform.SetScaleX(-sp2.transform.GetScaleX());

            yield return new WaitForSeconds(0.4f);
            anim.Play("SummonToIdle");

            sp.GetComponent<SpinnerControl>().StartCoroutine(nameof(SpinnerControl.StartRun));
            sp2.GetComponent<SpinnerControl>().StartCoroutine(nameof(SpinnerControl.StartRun));
            yield return new WaitForSeconds(5.5f * UnityEngine.Random.value);
        }
    }
}
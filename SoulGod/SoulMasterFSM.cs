using FSMProxy;
using HKTool.FSM;
using HKTool.FSM.CSFsm;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SoulGod
{
    internal class SoulMasterFSM : CSFsm<SoulMasterFSM>
    {
        public int BaseHP = 500;

        [FsmVar(FSMProxy_SoulMaster.VariableNames.Projectile)]
        public FsmGameObject MageOrbInst = new();

        [ComponentBinding]
        public tk2dSpriteAnimator anim = null!;

        public GameObject orbL;
        public GameObject orbR;

        [ComponentBinding]
        public HealthManager hm;
        protected override void OnAfterBindPlayMakerFSM()
        {
            base.OnAfterBindPlayMakerFSM();

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
                        if(old != null)
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
        }

        [FsmState]
        private IEnumerator SoulMasterInit()
        {
            DefineEvent(FsmEvent.Finished, "Pause");
            yield return StartActionContent;
            yield return new WaitForSeconds(0.1f);

            hm.hp = BaseHP;
        }

        [FsmState]
        private IEnumerator SuperOrbShot()
        {
            yield return StartActionContent;

        }


    }
}

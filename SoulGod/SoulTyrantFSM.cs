using FSMProxy;
using HKTool.FSM;
using HKTool.FSM.CSFsm;
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
    internal class SoulTyrantFSM : CSFsm<SoulTyrantFSM>
    {
        [ComponentBinding]
        public tk2dSpriteAnimator anim = null!;
        [ComponentBinding]
        public AudioSource audio = null!;
        [ComponentBinding("Fire Effect")]
        public GameObject fireeffect = null!;

        [ComponentBinding("Shot Charge")]
        public ParticleSystem shotParticle = null!;

        public FSMProxy_SoulMaster proxy = null!;

        private GameObject rayOrb = null!;
        protected override void OnAfterBindPlayMakerFSM()
        {
            proxy = new(FsmComponent);

            var ac = FsmComponent.Fsm
                .GetState(FSMProxy_SoulMaster.StateNames.Attack_Choice)
                .GetFSMStateActionOnState<SendRandomEventV3>();

            ac.events = ac.events
                .Append(FsmEvent.GetFsmEvent("SMG ST RAY"))
                .ToArray();
            ac.weights = ac.weights
                .Append(0.35f)
                .ToArray();
            ac.eventMax = ac.eventMax
                .Append(1)
                .ToArray();
            ac.missedMax = ac.missedMax
                .Append(4)
                .ToArray();

            ac.trackingInts = ac.trackingInts
                .Append(0)
                .ToArray();
            ac.trackingIntsMissed = ac.trackingIntsMissed
                .Append(0)
                .ToArray();
        }

        [FsmState]
        private IEnumerator ST_Ray()
        {
            DefineGlobalEvent("SMG ST RAY");
            DefineEvent("TELE", FSMProxy_SoulMaster.StateNames.Teleport);
            DefineEvent(FsmEvent.Finished, FSMProxy_SoulMaster.StateNames.Reactivate);

            yield return StartActionContent;
            if (transform.position.y != proxy.Variables.Top_Y.Value)
            {
                var pos = new Vector2(20, proxy.Variables.Top_Y.Value);
                proxy.Variables.Tele_X.Value = pos.x;
                proxy.Variables.Tele_Y.Value = pos.y;
                proxy.Variables.Teleport_Point.Value = pos;
                proxy.Variables.Next_Event.Value = "SMG ST RAY";
                yield return "TELE";
            }
            shotParticle.Play();
            anim.Play("Summon");
            audio.PlayOneShot(SoulGodMod.Instance.mage_lord_projectile_charge);
            yield return new WaitForSeconds(0.35f);
            
            rayOrb = Instantiate(SoulGodMod.Instance.MageOrbPrefab, fireeffect.transform.position,
                Quaternion.identity);
            var oc = rayOrb.AddComponent<MageOrbControl>();
            oc.canTouchWall = false;
            oc.isTrigger = false;

            rayOrb.GetFSMState("Check Spinner").Actions = new FsmStateAction[0];

            FSMUtility.SendEventToGameObject(rayOrb, "ORBIT");
            iTween.ScaleAdd(rayOrb, new(1.5f, 1.5f, 0), 1.65f);
            yield return new WaitForSeconds(2);
            shotParticle.Stop();
            {
                var pos = new Vector2(20, proxy.Variables.Top_Y.Value + 40);
                proxy.Variables.Tele_X.Value = pos.x;
                proxy.Variables.Tele_Y.Value = pos.y;
                proxy.Variables.Teleport_Point.Value = pos;
                proxy.Variables.Next_Event.Value = "SMG ST RAY 2";
                yield return "TELE";
            }
        }

        [FsmState]
        private IEnumerator ST_Ray2()
        {
            DefineGlobalEvent("SMG ST RAY 2");
            DefineEvent("TELE", FSMProxy_SoulMaster.StateNames.Teleport);
            DefineEvent(FsmEvent.Finished, FSMProxy_SoulMaster.StateNames.Reactivate);

            yield return StartActionContent;
            
            FSMUtility.SendEventToGameObject(rayOrb, "FIRE");
            yield return new WaitForSeconds(0.05f);
            rayOrb.GetComponentInChildren<DamageHero>().damageDealt = 2;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.75f, 1.25f));
            if (rayOrb.LocateMyFSM("Orb Control").ActiveStateName != "Chase Hero")
            {
                yield break;
            }

            FSMUtility.SendEventToGameObject(rayOrb, "END");
            for(int i = 0; i < 10; i++)
            {
                var orb = Instantiate(SoulGodMod.Instance.MageOrbPrefab, 
                    rayOrb.transform.position, Quaternion.identity);
                var oc = orb.AddComponent<MageOrbControl>();
                oc.canTouchWall = false;
                oc.isTrigger = true;
                var rig = orb.GetComponent<Rigidbody2D>();
                rig.velocity = new(UnityEngine.Random.Range(-20, 20),
                    UnityEngine.Random.Range(-10, 10));
            }
            yield return new WaitForSeconds(2.5f);
        }
    }
}

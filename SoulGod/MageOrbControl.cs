using HKTool.FSM;
using HKTool.FSM.CSFsm;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SoulGod
{
    internal class MageOrbControl : MonoBehaviour
    {
        public Vector2? offset;
        public bool canTouchWall = false;
        public bool isTrigger = true;
        void Start()
        {
            gameObject.AddComponent<DestroyOnInactive>();

            IEnumerator Init()
            {
                yield return new WaitForSeconds(0.01f);
                var fsm = gameObject.LocateMyFSM("Orb Control");

                var s = fsm.Fsm.GetState("Chase Hero");
                s.GetFSMStateActionOnState<Wait>().time = 4;
                var sa = s.GetFSMStateActionOnState<Collision2dEventLayer>();
                sa.Enabled = false;
                sa.OnExit();
                s.ActiveActions.Remove(sa);
                s.RemoveAllFsmStateActions<Collision2dEventLayer>();
                GetComponent<Collider2D>().isTrigger = isTrigger;
            }
            if(!canTouchWall)
            {
                StartCoroutine(Init());
            }
            
        }
        void Update()
        {
            if(offset != null && transform.parent != null)
            {
                var o = offset.Value;
                transform.localPosition = o;
            }
        }
    }
}

using HKTool.FSM.CSFsm;
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SoulGod
{
    internal class FsmEventProxy : CSFsm<FsmEventProxy>
    {
        public ProxyAction proxy = new();
        public class ProxyAction: FsmStateAction
        {
            public List<GameObject> targets = new();
            public override bool Event(FsmEvent fsmEvent)
            {
                foreach(var v in targets)
                {
                    FSMUtility.SendEventToGameObject(v, fsmEvent, true);
                }
                return base.Event(fsmEvent);
            }
        }
        [FsmState]
        private IEnumerator Init()
        {
            yield return StartActionContent;
            InvokeAction(proxy);
        }
    }
}

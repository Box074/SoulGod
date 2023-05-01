using HKTool.FSM;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using UnityEngine;

namespace SoulGod
{
    public class SpinnerControl : MonoBehaviour
    {
        bool isRunning = false;
        public static float speed = 12;
        void Start()
        {
            var spinner = Instantiate(SoulGodMod.Instance.SuperOrbSpinner, transform);
            spinner.SetActive(true);
            FSMUtility.SendEventToGameObject(spinner, "SPINNER SUMMON");
            isRunning = false;

            if (Random.value < 0.5f)
            {
                SoulGodMod.SetSpinnerRotate(spinner, -200);
            }
        }

        void Update()
        {
            if(isRunning)
            {
                var pos = transform.position;
                pos.x += speed * Time.deltaTime * transform.localScale.x;
                transform.position = pos;
            }
        }

        public IEnumerator StartRun()
        {
            isRunning = true;
            yield return new WaitForSeconds(5);
            Destroy(gameObject);
        }
    }
}
using System.Collections;
using UnityEngine;

namespace SoulGod
{
    public class DestroyOnInactive : MonoBehaviour
    {

        void OnDisable()
        {
            Destroy(gameObject);
        }
    }
}
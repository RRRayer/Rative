using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct StatBlock
    {
        [Range(0, 10)] public int str;
        [Range(0, 10)] public int intel;
        [Range(0, 10)] public int luk;
        [Range(0, 10)] public int agi;
        [Range(0, 10)] public int vit;
        [Range(0, 10)] public int spi;

        public bool IsEmpty =>
            str == 0 && intel == 0 && luk == 0 && agi == 0 && vit == 0 && spi == 0;
    }
}

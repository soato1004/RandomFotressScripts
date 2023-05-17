using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public abstract class MonsterBase : UnitBase
    {
        public abstract void Init(int index, float buffHP);

        public abstract void Hit(int damage);
    }
}

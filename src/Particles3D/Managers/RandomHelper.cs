﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Particles3D;

public static class RandomHelper
{
    public static Vector3 OnUnitSphere(this FastRandom random)
    {
        Vector3 v = new Vector3(random.NextSingle(-0.25f, 0.25f), random.NextSingle(-1f, 1f), random.NextSingle(-0.25f, 0.25f));
        v.Normalize();
        return v;
    }
}
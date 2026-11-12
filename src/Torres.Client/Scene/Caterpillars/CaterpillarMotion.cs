using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct CaterpillarMotion(CaterpillarMotionKind Kind, BoardCaterpillar Caterpillar, Vector3 StartBodyOrigin, Vector3 EndBodyOrigin, TimeSpan StartTime);
}

using UnityEngine;
using System.Collections.Generic;

namespace ColbyDoan.Physics
{
    public static class WindManager
    {
        public static HashSet<IWindSource> windSources = new HashSet<IWindSource>();

        public static Vector3 GetWindAtPoint(Vector3 point)
        {
            Vector3 windSpeed = Vector3.zero;
            foreach (IWindSource source in windSources)
            {
                windSpeed += source.GetWindAtPoint(point);
            }
            return windSpeed;
        }
    }

    public interface IWindSource
    {
        public Vector3 GetWindAtPoint(Vector3 point);
    }
}

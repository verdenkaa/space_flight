using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace Space_Flight_Code
{
    using System;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media.Media3D;

    namespace Space_Flight_Code
    {
        public class Physics
        {
            private const double G = 6.67430e-20;
            const double G_SI = 6.67430e-11;

            public (Point3D NewPosition, Vector3D NewVelocity) GravityEvaluate(Satelite satelite, Earth earth, Moon moon)
            {
                Vector3D toEarth = (Vector3D)(earth.Center - satelite.Center);
                Vector3D toMoon = (Vector3D)(moon.GetWorldPosition() - satelite.Center);

                double distE = toEarth.Length;
                double distM = toMoon.Length;

                Vector3D accelTotal = Aceleration(distE, toEarth, earth.Mass) + Aceleration(distM, toMoon, moon.Mass);

                Vector3D newVelocity = satelite.Velocity + accelTotal;
                Point3D newPosition = satelite.Center + newVelocity;

                return (newPosition, newVelocity);
            }

            public Vector3D Aceleration(double dist, Vector3D toPlanet, double mass)
            {
                Vector3D accelEarth = new Vector3D(0, 0, 0);
                if (dist > 1e-6)
                {
                    Vector3D dirE = toPlanet;
                    dirE.Normalize();
                    double aE = G * mass / (dist * dist);
                    accelEarth = dirE * aE;
                }

                return accelEarth;
            }

            public bool Is_Hit(Satelite satelite, Earth earth, Moon moon)
            {
                Vector3D toEarth = (Vector3D)(earth.Center - satelite.Center);
                Vector3D toMoon = (Vector3D)(moon.GetWorldPosition() - satelite.Center);

                double distE = toEarth.Length;
                double distM = toMoon.Length;

                if (distE <= earth.Radius || distM <= moon.Radius)
                {
                    return true;
                }

                return false;
            }

            public (double KineticJ, double PotentialJ, double TotalJ) EvaluateEnergies(Satelite satelite, Earth earth, Moon moon)
            {
                double mSat = satelite.Mass;

                // Скорость в m/s
                double v_m_s = satelite.Velocity.Length * 1000.0;
                double kineticJ = 0.5 * mSat * v_m_s * v_m_s;

                double potentialJ = 0.0;

                Vector3D toEarth = (Vector3D)(earth.Center - satelite.Center);
                double distE_km = toEarth.Length;
                if (distE_km > 1e-9) // обработка слишком близкого расположения
                {
                    double rE_m = distE_km * 1000.0;
                    potentialJ += -G_SI * mSat * earth.Mass / rE_m;
                }

                Vector3D toMoon = (Vector3D)(moon.GetWorldPosition() - satelite.Center);
                double distM_km = toMoon.Length;
                if (distM_km > 1e-9)
                {
                    double rM_m = distM_km * 1000.0;
                    potentialJ += -G_SI * mSat * moon.Mass / rM_m;
                }

                double totalJ = kineticJ + potentialJ;
                return (kineticJ, potentialJ, totalJ);
            }
        }
    }


}
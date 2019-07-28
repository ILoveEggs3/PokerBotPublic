using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;

namespace MathHelper
{
    static public class C2DTransformationHelper
    {
        #region PointToVectorAdapter
        static private OpenTK.Vector2d PointToVector(object p)
        {
            if (p is OpenTK.Vector2d)
                return (OpenTK.Vector2d)p;
            else if (p is System.Drawing.PointF)
                return PointToVector((System.Drawing.PointF)p);
            else if (p is System.Drawing.Point)
                return PointToVector((System.Drawing.Point)p);
            else if (p is System.Windows.Point)
                return PointToVector((System.Windows.Point)p);
            else
                return new OpenTK.Vector2d();
        }
        static public OpenTK.Vector2d PointToVector(System.Drawing.PointF p)
        {
            return new OpenTK.Vector2d(p.X, p.Y);
        }
        static public OpenTK.Vector2d PointToVector(System.Drawing.Point p)
        {
            return new OpenTK.Vector2d(p.X, p.Y);
        }
        static public OpenTK.Vector2d PointToVector(System.Windows.Point p)
        {
            return new OpenTK.Vector2d(p.X, p.Y);
        }

        static private OpenTK.Vector2d PointsToVector(object _from, object _to)
        {
            return PointToVector(_to) - PointToVector(_from);
        }
        static public System.Drawing.PointF PointsToVector(System.Drawing.PointF _from, System.Drawing.PointF _to)
        {
            OpenTK.Vector2d V = PointToVector((object)_to) - PointToVector((object)_from);
            return new System.Drawing.PointF((float)V.X, (float)V.Y);
        }
        static public System.Drawing.Point PointsToVector(System.Drawing.Point _from, System.Drawing.Point _to)
        {
            OpenTK.Vector2d V = PointToVector((object)_to) - PointToVector((object)_from);
            return new System.Drawing.Point((int)V.X, (int)V.Y);
        }
        static public System.Windows.Point PointsToVector(System.Windows.Point _from, System.Windows.Point _to)
        {
            OpenTK.Vector2d V = PointToVector((object)_to) - PointToVector((object)_from);
            return new System.Windows.Point((double)V.X, (double)V.Y);
        }
        #endregion

        #region UtilsFunctions
        static private double GetVectorLength(object p)
        {
            return PointToVector(p).Length;
        }
        static public double GetVectorLength(System.Drawing.PointF p)
        {
            return GetVectorLength((object)p);
        }
        static public double GetVectorLength(System.Drawing.Point p)
        {
            return GetVectorLength((object)p);
        }
        static public double GetVectorLength(System.Windows.Point p)
        {
            return GetVectorLength((object)p);
        }

        static private double DotProduct(object u, object v)
        {
            return OpenTK.Vector2d.Dot(PointToVector(u), PointToVector(v));
        }
        static public double DotProduct(System.Drawing.PointF u, System.Drawing.PointF v)
        {
            return DotProduct((object)u, (object)v);
        }
        static public double DotProduct(System.Drawing.Point u, System.Drawing.Point v)
        {
            return DotProduct((object)u, (object)v);
        }
        static public double DotProduct(System.Windows.Point u, System.Windows.Point v)
        {
            return DotProduct((object)u, (object)v);
        }

        static private double GetAngleBetweenVectors(object u, object v)
        {
            return Math.Acos(DotProduct(u, v) / (GetVectorLength(u) * GetVectorLength(v)));
        }
        static public double GetAngleBetweenVectors(System.Drawing.PointF u, System.Drawing.PointF v)
        {
            return GetAngleBetweenVectors((object)u, (object)v);
        }
        static public double GetAngleBetweenVectors(System.Drawing.Point u, System.Drawing.Point v)
        {
            return GetAngleBetweenVectors((object)u, (object)v);
        }
        static public double GetAngleBetweenVectors(System.Windows.Point u, System.Windows.Point v)
        {
            return GetAngleBetweenVectors((object)u, (object)v);
        }
        #endregion

        #region Transformations
        static private object Scale(object u, double factor)
        {
            if (u is OpenTK.Vector2d)
            {
                OpenTK.Vector2d V = (OpenTK.Vector2d)u;
                V.X *= factor;
                V.Y *= factor;
                return V;
            }
            else if (u is System.Drawing.PointF)
            {
                System.Drawing.PointF V = (System.Drawing.PointF)u;
                V.X = (float)(V.X * factor);
                V.Y = (float)(V.Y * factor);
                return V;
            }
            else if (u is System.Drawing.Point)
            {
                System.Drawing.Point V = (System.Drawing.Point)u;
                V.X = (int)(V.X * factor);
                V.Y = (int)(V.X * factor);
                return V;
            }
            else if (u is System.Windows.Point)
            {
                System.Windows.Point V = (System.Windows.Point)u;
                V.X *= factor;
                V.Y *= factor;
                return V;
            }
            else
                return new OpenTK.Vector2d();
        }
        static public System.Drawing.PointF Scale(System.Drawing.PointF u, double factor)
        {
            return (System.Drawing.PointF)Scale((object)u, factor);
        }
        static public System.Drawing.Point Scale(System.Drawing.Point u, double factor)
        {
            return (System.Drawing.Point)Scale((object)u, factor);
        }
        static public System.Windows.Point Scale(System.Windows.Point u, double factor)
        {
            return (System.Windows.Point)Scale((object)u, factor);
        }

        static private object Translate(object u, object translation)
        {
            OpenTK.Vector2d T = PointToVector(translation);
            if (u is OpenTK.Vector2d)
            {
                OpenTK.Vector2d V = (OpenTK.Vector2d)u;
                V.X += T.X;
                V.Y += T.Y;
                return V;
            }
            else if (u is System.Drawing.PointF)
            {
                System.Drawing.PointF V = (System.Drawing.PointF)u;
                V.X += (float)T.X;
                V.Y += (float)T.Y;
                return V;
            }
            else if (u is System.Drawing.Point)
            {
                System.Drawing.Point V = (System.Drawing.Point)u;
                V.X += (int)T.X;
                V.Y += (int)T.Y;
                return V;
            }
            else if (u is System.Windows.Point)
            {
                System.Windows.Point V = (System.Windows.Point)u;
                V.X += T.X;
                V.Y += T.Y;
                return V;
            }
            else
                return new OpenTK.Vector2d();
        }
        static public System.Drawing.PointF Translate(System.Drawing.PointF u, System.Drawing.PointF translation)
        {
            return (System.Drawing.PointF)Translate((object)u, (object)translation);
        }
        static public System.Drawing.Point Translate(System.Drawing.Point u, System.Drawing.Point translation)
        {
            return (System.Drawing.Point)Translate((object)u, (object)translation);
        }
        static public System.Windows.Point Translate(System.Windows.Point u, System.Windows.Point translation)
        {
            return (System.Windows.Point)Translate((object)u, (object)translation);
        }

        static private object Rotate(object u, double angle)
        {
            OpenTK.Vector2d U = PointToVector(u);
            if (u is OpenTK.Vector2d)
            {
                OpenTK.Vector2d V = (OpenTK.Vector2d)u;
                return new OpenTK.Vector2d(V.X * Math.Cos(angle) - V.Y * Math.Sin(angle), V.X * Math.Sin(angle) + V.Y * Math.Cos(angle));
            }
            else if (u is System.Drawing.PointF)
            {
                System.Drawing.PointF V = (System.Drawing.PointF)u;
                return new System.Drawing.PointF((float)((double)V.X * Math.Cos(angle) - (double)V.Y * Math.Sin(angle)), (float)((double)V.X * Math.Sin(angle) + (double)V.Y * Math.Cos(angle)));
            }
            else if (u is System.Drawing.Point)
            {
                System.Drawing.Point V = (System.Drawing.Point)u;
                return new System.Drawing.Point((int)((double)V.X * Math.Cos(angle) - (double)V.Y * Math.Sin(angle)), (int)((double)V.X * Math.Sin(angle) + (double)V.Y * Math.Cos(angle)));
            }
            else if (u is System.Windows.Point)
            {
                System.Windows.Point V = (System.Windows.Point)u;
                return new System.Windows.Point(V.X * Math.Cos(angle) - V.Y * Math.Sin(angle), V.X * Math.Sin(angle) + V.Y * Math.Cos(angle));
            }
            else
                return new OpenTK.Vector2d();
        }
        static public System.Drawing.PointF Rotate(System.Drawing.PointF u, double angle)
        {
            return (System.Drawing.PointF)Rotate((object)u, angle);
        }
        static public System.Drawing.Point Rotate(System.Drawing.Point u, double angle)
        {
            return (System.Drawing.Point)Rotate((object)u, angle);
        }
        static public System.Windows.Point Rotate(System.Windows.Point u, double angle)
        {
            return (System.Windows.Point)Rotate((object)u, angle);
        }
        #endregion
    }
}

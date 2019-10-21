using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(Curve axis, int polygon, double level_offset, int num_levels, List<double> h, ref object a, ref object b, ref object c, ref object d, ref object e)
    {
        List<LineCurve> Lines = new List<LineCurve>();
        List<Surface> Srf = new List<Surface>();
        List<Point3d> Points = new List<Point3d>();
        List<Surface> ArcFaces = new List<Surface>();
        List<Surface> PlanarFaces = new List<Surface>();

        Point3d A = CurveStartPoint(axis);
        Point3d B = CurveEndPoint(axis);
        //Vector3d A_vec = VectorUnitize(VectorCreate(A, B));
        Vector3d A_vec = VectorUnitize(new Vector3d(A, B));
        //Vector3d B_vec = VectorUnitize(VectorCreate(B, A));
        Vector3d B_vec = VectorUnitize(new Vector3d (B, A));

        Pointd3d Origin = CurveMidPoint(axis);
        int angle = 180 / polygon;
        double d = Distance(A, B) / 2;
        Vector3d dir = VectorScale(A_vec, d);

        List<Point3d> Pts = new List<Point3d>();
        //Pts = [];

        for (int i = 0; i <= polygon + 1; i++)
        {
            Point3d p = new Point3d(Origin, dir);
            //p = PointAdd(Origin, dir);

            X = Pt(p, rs.VectorUnitize(dir), Origin);

            Pts.append(X);
            dir = rs.VectorRotate(dir, angle, new Vector3d[0, 0, 1]);
        }

        // for x in range(0, polygon + 1);
        //p = PointAdd(Origin, dir);
        // X = Pt(p, rs.VectorUnitize(dir), Origin);
        // Pts.append(X);
        // dir = rs.VectorRotate(dir, angle, [0, 0, 1]);

        // for x in range(1, polygon + 1);
        //Break(Pts[x], Pts[x - 1]);

        for (int i = 1; i <= polygon + 1; i++)
        {
            Break(Pts[i], Pts[i - 1]);
        }

        a = Lines;
        b = Srf;
        c = Points;
        d = ArcFaces;
        e = PlanarFaces;

    }

    // <Custom additional code> 
    void Break(LineCurve A, LineCurve B)
    {
        Point3d pt = A.interpTo(B, 0.5);//?
        Vector3d V = new Vector3d(pt, A.o);
        V = VectorScale(V, 0.8);
        //new_pos = Rhino.PointAdd(A.o, V);
        Point3d new_pos = new Point3d(A.o, V);
        //newPt = Pt(new_pos, VectorUnitize(V), A.o);
        Point3d newPt = new Point3d(new_pos, VectorUnitize(V), A.o);
        //Grow(newPt, A, num_levels)
        //Grow(newPt, B, num_levels)

        MoveUp(newPt, A, num_levels);
        MoveUp(newPt, B, num_levels);

        //DrawLine(newPt, A)
        //DrawLine(newPt, B)
    }

    void MoveUp(LineCurve A, LineCurve B, int num)
    {
        A_2 = A.getNewLevel(h[(num)]);
        B_2 = B.getNewLevel(h[(num)]);
        //DrawLine(A_2, A)
        //DrawLine(B_2, B)
        //PlanarFaces.append(AddSrf([A.p, B.p, B_2.p, A_2.p, A.p])[0])
        Surface PlanarFaces = new PlanarFaces();
        PlanarFaces.Add(new Surface([A, B, B_2, A_2, A][0]);// .p lere dikkat
        //PlanarFaces.append(AddSrf([A.p, B.p, B_2.p, A_2.p, A.p])[0])
        Grow(A_2, B_2, num);
    }

    Surface AddSrf(List<Point> PtArray)
    {
        Polyline poly = new Polyline(PtArray);
        //poly = rs.AddPolyline(PtArray);
        PlanarSrf s = new PlanarSrf(poly);
        //s = rs.AddPlanarSrf(poly);
        return s;
    }

    void DrawArcSrf(LineCurve A, LineCurve B, int num)
    {
        //move B down
        B_2 = B.getNewLevel(-h[(num)]);

        //Curve c1 = rs.AddCurve([A, B, B_2], 3);
        LineCurve c1 = new LineCurve(new List<LineCurve>[A.p, B.p, B_2.p], 3);//?
        Line c2 = new Line(B_2.p, B.p);
        Line c3 = new Line(A.p, B.p);
        //crvs = rs.JoinCurves([c1, c2, c3], True);
        List<LineCurve> crvs = rs.JoinCurves(new List<LineCurve>[c1, c2, c3], True);
        //Srf.append(rs.AddPlanarSrf(crvs)[0]);
        Srf.append(rs.AddPlanarSrf(crvs)[0]);
        Lines.Add(c1);
    }

    def Grow(LineCurve A, LineCurve B, int g)
    {
        if (g > 0)
        {
            V = rs.VectorScale(A.v, rs.Distance(A.p, B.p) * level_offset);
            new_pos = rs.PointAdd(A.p, V);
            newPt = Pt(new_pos, A.v, A.p);

            MoveUp(B, newPt, g - 1);
            //Grow(B, newPt, g-1)
            DrawLine(newPt, A);
            DrawLine(newPt, B);
            DrawArcSrf(newPt, B, g);
            Points.append(newPt.interpTo(A, 0.5));

            ArcFaces.append(AddSrf([newPt.p, A.p, B.p, newPt.p])[0]);
        }

        else
        {
            DrawLine(A, B);
        }

        void DrawLine(LineCurve A, LineCurve B)
        {
            //Lines.append(rs.AddLine(A.p, B.p));
            //Lines
        }

        List<Point3d> interpToPt(Point3d ptA, Point3d ptB, int par)
        {
            V = rs.VectorCreate(ptA, ptB);
            V = rs.VectorScale(V, par);
            return new Point3d(ptA, V);
        }
    }

        /*
        class Pt:
          def __init__(self, pos, vec, ori):
        self.p = pos
        self.v = vec
        self.o = ori

        def getNewLevel(self, height):
        return Pt(rs.PointAdd(self.p, [0, 0, height]), self.v, rs.PointAdd(self.o, [0, 0, height]))
        
        def getDistO(self):
        return rs.Distance(self.p, self.o)

        def interpTo(self, to, par):
        V = rs.VectorCreate(to.p, self.p)
        V = rs.VectorScale(V, par)
        return rs.PointAdd(self.p, V)

       */

        public class Pt
        {
            public Pt( Point3d pos,Vector3d vec,Direction ori)
            {

            }

            public Point3d getNewLevel(double height)
            {
                 return Pt(new Point3d(self.p, [0, 0, height]), self.v, Point3d(self.o, [0, 0, height]));
            }


            public double getDistO()
            {
               return Distance(this.p, this.o);
            }

            public void interpTo( Point3d to ,double par)
            {
                //V = VectorCreate(to.p, self.p);
                Vector3d V = new Vector3d(to.p, this.p);
                V = VectorScale(V, par);
                //return PointAdd(self.p, V);
                return new Point3d(this.p, V);
            }

         }

        // </Custom additional code> 
    }
}
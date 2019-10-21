using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry ;

using Rhino;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel.Data;


using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;

using System.Runtime.InteropServices;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Muqarnate_1
{
    public class Generator : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Generator()
          : base("Recursive", "Rec",
              "Generates a muqarnas from a single line within a recursive function.",
              "Muqarnate", "Primitive")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
           
            pManager.AddLineParameter("axis", "A", "Base curve to start a form", GH_ParamAccess.item, Line.Unset);
            pManager.AddIntegerParameter("polygon", "P", "Number of the Sides of a Polygon", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("levelOffset", "L_O", "Difference between each level", GH_ParamAccess.item, 10.0);
            pManager.AddIntegerParameter("numLevels", "L_N", "Number of levels", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("h", "h", "h", GH_ParamAccess.list, new List<double>() { });

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            //pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);
            pManager.AddLineParameter("A_Lines", "a", "Lines", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("B_Surfaces", "b", "Surfaces", GH_ParamAccess.list);
            pManager.AddPointParameter("C_Points", "c", "Points", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("D_ArcFaces", "d", "Trimmed Surfaces", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("E_PlanarFaces", "e", "Trimmed Surfaces", GH_ParamAccess.list);

            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            // pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            //Plane plane = Plane.WorldXY;

            Line axis = new Line();
            int polygon = 0;
            double levelOffset = 10.0;
            int numLevels = 0;
            List<double> h = new List<double>();

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref axis)) return;
            if (!DA.GetData(1, ref polygon)) return;
            if (!DA.GetData(2, ref levelOffset)) return;
            if (!DA.GetData(3, ref numLevels)) return;
            //if (!DA.GetData(4, ref h)) return;
            if (!DA.GetDataList<double> (4, h)) return;///in case of trouble check here!

            // We should now validate the data and warn the user if invalid data is supplied.
            /* if (radius0 < 0.0)
             {
                 AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Inner radius must be bigger than or equal to zero");
                 return;
             }
             if (radius1 <= radius0)
             {
                 AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Outer radius must be bigger than the inner radius");
                 return;
             }*/
            if (polygon <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Polygon must be bigger than or equal to one");
                return;
            }

            if (numLevels <= 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "To get more accurete form give more than 3");
                return;
            }

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:

            //Curve spiral = CreateSpiral(plane, radius0, radius1, turns);
            //Curve spiral = CreateSpiral(plane, radius0, radius1, turns);

            // Finally assign the spiral to the output parameter.
            //DA.SetData(0, A_Lines);

           // void shellMuq = CreateMuqarnas(List<double> h, Line axis, double levelOffset, int polygon, int numLevels);
           // DA.SetData(0,shellMuq);
        }

        public static double DistanceCalc(Point3d A, Point3d B)
        {
            return Math.Sqrt(Math.Pow((A.X - B.X), 2) + Math.Pow((A.Y - B.Y), 2) + Math.Pow((A.Z - B.Z), 2));
        }

        public class Pt
        {
            public Point3d P;
            public Vector3d V;
            public Point3d O;

            public Pt(Point3d pos, Vector3d vec, Point3d ori)
            {
                this.P = pos;
                this.V = vec;
                this.O = ori;
            }

            public Pt GetNewLevel(double height)
            {
                return new Pt(new Point3d(this.P + new Vector3d(0, 0, height)),
                this.V, new Point3d(this.O + new Vector3d(0, 0, height)));
            }

            public double GetDist()
            {
                return DistanceCalc(this.P, this.O);
            }

            public Point3d InterpTo(Pt to, double par)
            {
                //V = VectorCreate(to.P, self.P);
                Vector3d V = new Vector3d(to.P - this.P);
                //V = VectorScale(V, par);
                V *= par;
                //return PointAdd(self.P, V);
                return new Point3d(this.P + V);
            }

        }

        private void CreateMuqarnas(List<double> h, Line axis, double levelOffset, int polygon, Int32 numLevels)
        {
            //Point3d A = rs.CurveStartPoint(axis);
            //Point3d A = axis.PointAtStart ;
            Point3d A = axis.PointAt(0);
            Point3d B = axis.PointAt(1);

            // B = rs.CurveEndPoint(axis);
            //Point3d B = axis.PointAtEnd;
            //Vector3d A_vec = VectorUnitize(VectorCreate(A, B));

            //Vector3d A_vec = (new Vector3d(new Point3d ((A.X - B.X),(A.Y - B.Y),(A.Z - B.Z))).Unitize();
            Vector3d A_vec = new Vector3d(axis.PointAt(1.0) - axis.PointAt(0.0));
            A_vec.Unitize();

            // B_vec = VectorUnitize(VectorCreate(B, A));
            Vector3d B_vec = new Vector3d(axis.PointAt(0.0) - axis.PointAt(1.0));
            B_vec.Unitize();

            //Origin = rs.CurveMidPoint(axis);
            Point3d origin = axis.PointAt(0.5);

            double angle = 180 / polygon;
            // d = Distance(A, B) / 2;
            double d = DistanceCalc(A, B) / 2;
            // dir = VectorScale(A_vec, d);
            Vector3d dir = A_vec * d; //Scale

            List<Pt> Pts = new List<Pt>();
            //Pts = [];

            for (int i = 0; i <= polygon + 1; i++)
            {
                //p = PointAdd(Origin, dir);
                Point3d p = new Point3d(origin + dir);

                //X = Pt(p, rs.VectorUnitize(dir), Origin);
                //Pt  X = new Pt(p, dir.Unitize() , origin);  
                Pt X = new Pt(p, VectorUnitize(dir), origin);

                Pts.Add(X);
                //Pts.append(X);
                //dir = VectorRotate(dir, angle, new Vector3d[0, 0, 1]);
                //dir = VectorRotate(dir, angle, new Vector3d[0, 0, 1]);
                Transform.Rotation(angle, new Vector3d(0, 0, 1), X.P); //  hatali olabilur
            }

            Vector3d VectorUnitize(Vector3d v)
            {
                return v / (Math.Pow(v.X, 2) + Math.Pow(v.Y, 2) + Math.Pow(v.Z, 2));
            }

            for (int i = 1; i <= polygon + 1; i++)
            {
                Break(Pts[i], Pts[i - 1]);
            }

            void Break(Pt A3, Pt B3)
            {
                Point3d pt = A3.InterpTo(B3, 0.5); //?
                Vector3d V = new Vector3d(pt - A3.O);

                V *= 0.8;
                //new_pos = Rhino.PointAdd(A.o, V);
                Point3d new_pos = new Point3d(A3.O + V);
                //newPt = Pt(new_pos, VectorUnitize(V), A.o);
                var newPt = new Pt(new_pos, VectorUnitize(V), A3.O);
                //Grow(newPt, A, num_levels)
                //Grow(newPt, B, num_levels)

                MoveUp(newPt, A3, numLevels);
                MoveUp(newPt, B3, numLevels);

                //DrawLine(newPt, A)
                //DrawLine(newPt, B)
            }

            void MoveUp(Pt A4, Pt B4, int num)
            {
                Pt A_4 = A4.GetNewLevel(h[num]);
                Pt B_4 = B4.GetNewLevel(h[num]);
                //DrawLine(A_2, A)
                //DrawLine(B_2, B)
                //PlanarFaces.append(AddSrf([A.p, B.p, B_2.p, A_2.p, A.p])[0])
                List<Surface> PlanarFaces = new List<Surface>();
                PlanarFaces.Add(AddSrf(new List<Point3d>() { A4.P, B4.P, B_4.P, A_4.P, A4.P }));

                //PlanarFaces.append(AddSrf([A.p, B.p, B_2.p, A_2.p, A.p])[0])
                Grow(A_4, B_4, num);
            }

            Surface AddSrf(List<Point3d> PtArray)
            {
                //Polyline poly = new Polyline(PtArray);
                //poly = rs.AddPolyline(PtArray);
                //Surface s = new Surface(poly);
                Surface s = AddSrf(PtArray);

                //s = rs.AddPlanarSrf(poly);
                return s;
            }

            List<Curve> Lines = new List<Curve>();
            List<Surface> Srf = new List<Surface>();

            void DrawArcSrf(Pt A1, Pt B1, int num)
            {
                //move B down

                Pt B_2 = B1.GetNewLevel(-h[num]);
                Curve c1 = Curve.CreateInterpolatedCurve(new List<Point3d>() { A1.P, B1.P, B_2.P }, 3);

                //Curve c1 = rs.AddCurve([A, B, B_2], 3);
                //LineCurve c1 = new LineCurve(new List<Line>([A1.P, B1.P, B_2.P], 3);//?
                //LineCurve c1a = new LineCurve(, 3);//?
                var c2 =Curve.CreateControlPointCurve(new List<Point3d>() { B_2.P, B1.P }, 1);
                var c3=Curve.CreateControlPointCurve(new List<Point3d>() { A1.P, B1.P }, 1);
                //Curve c2 = new Curve(B_2.P, B1.P);
                //Curve c3 = new Curve(A1.P, B1.P);
                //crvs = rs.JoinCurves([c1, c2, c3], True);
                //List<LineCurve> crvs = rs.JoinCurves(new List<LineCurve>[c1, c2, c3], True);
                Curve[] crvs = Curve.JoinCurves(new List<Curve>() { c1, c2, c3 }, 4 , true);
                
                //Srf.append(rs.AddPlanarSrf(crvs)[0]);
                Srf.Add(AddSrf(new List<Point3d>() { A1.P, B1.P, B_2.P }));
                Lines.Add(c1);
            }

            List<Surface> ArcFaces = new List<Surface>();
            List<Point3d> Points = new List<Point3d>();

            //double levelOffset = 10.0; // declared additionally please ERASE if conflicts bewtween input 
            void Grow(Pt A2, Pt B2, int g)
            {
                if (g > 0)
                {
                    Vector3d V = A2.V * (DistanceCalc(A2.P, B2.P) * levelOffset);
                    Point3d new_pos = A2.P + V;
                    Pt newPt = new Pt(new_pos, A2.V, A2.P);

                    MoveUp(B2, newPt, g - 1);
                    //Grow(B, newPt, g-1) 
                    DrawLine(newPt, A2);

                    DrawLine(newPt, B2);
                    DrawArcSrf(newPt, B2, g);
                    Points.Add(newPt.InterpTo(A2, 0.5));

                    ArcFaces.Add(AddSrf(new List<Point3d>() { newPt.P, A2.P, B2.P, newPt.P }));
                }

                else
                {
                    //DrawLine(A, B);
                    new Line(A2.P, B2.P);
                }

                void DrawLine(Pt A5, Pt B5)
                {
                    //Lines.append(rs.AddLine(A.p, B.p));

                    Lines.Add(new LineCurve(A5.P, B5.P));

                }
                /*
                Point3d InterpToPt(Point3d ptA, Point3d ptB, int par)
                {
                    //V = rs.VectorCreate(ptA, ptB);

                    var V = new Vector3d((ptB - ptA));

                    // V = rs.VectorScale(V, par);
                    V = V * par;
                    return new Point3d(ptA + V);
                }
                */
            }
        }
    
        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.Obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("082a4ad6-2e84-410d-be2f-6b5858160a43"); }
        }
    }
}

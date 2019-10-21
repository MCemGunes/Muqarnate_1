using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Muqarnate_1
{
    public class Muqarnate_1Info : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Muqarnate";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "This Plug-in Creates a center based recursive muqarnas generator";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("5c5ac8e0-t980-4361-9417-3262e2213c57");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "CEM GUNES";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "ITU_FACULTY_OF_ARCHITECTURE";
            }
        }
    }
}

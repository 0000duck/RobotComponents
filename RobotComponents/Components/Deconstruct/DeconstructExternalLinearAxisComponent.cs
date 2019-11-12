﻿using System;

using Rhino.Geometry;
using Grasshopper.Kernel;

using RobotComponents.Parameters;
using RobotComponents.Goos;

namespace RobotComponents.Components
{
    public class DeconstructExternalLinearAxisComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructRobotTool class.
        /// </summary>
        public DeconstructExternalLinearAxisComponent()
          : base("Deconstruct External Linear Axis", "DeConELA",
              "Deconstructs an External Linear Axis into its parameters."
                + System.Environment.NewLine +
                "RobotComponent V : " + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("External Linear Axis", "ELA", "External Linear Axis as Input", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PlaneParam("Attachment Plane", "AP", "Attachment Plane as Plane");
            pManager.Register_VectorParam("Axis", "A", "Axis as Vector");
            pManager.Register_IntervalParam("Axis Limits", "AL", "Axis Limits as Domain");
            pManager.Register_MeshParam("Base Mesh", "BM", "Base Mesh as Mesh");
            pManager.Register_MeshParam("Link Mesh", "LM", "Link Mesh as Mesh");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Variables input
            ExternalLinearAxisGoo externalLinearAxisGoo = null;

            //Get the data from the input
            if (!DA.GetData(0, ref externalLinearAxisGoo)) { return; }

            //Output
            DA.SetData(0, externalLinearAxisGoo.Value.AttachmentPlane);
            DA.SetData(1, externalLinearAxisGoo.Value.AxisPlane.ZAxis);
            DA.SetData(2, externalLinearAxisGoo.Value.AxisLimits);
            DA.SetData(3, externalLinearAxisGoo.Value.BaseMesh);
            DA.SetData(3, externalLinearAxisGoo.Value.LinkMesh);
        }

        /// <summary>
        /// Provides an Icon for the component
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.DeconstructExternalLinearAxis_Icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4E61CB2C-A7FE-43F8-9C61-616830FF57A1"); }
        }
    }

}
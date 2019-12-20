﻿using System;

using Grasshopper.Kernel;

using RobotComponents.BaseClasses;

using RobotComponentsABB.Goos;
using RobotComponentsABB.Parameters;

namespace RobotComponentsABB.Components
{
    public class OverrideRobotToolComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public OverrideRobotToolComponent()
          : base("Action: Override Robot Tool", "Overrides cuurent RobotTool",
              "Defines an instruction to override the current robot tool of the ABB robot for RAPID base code generation."
                + System.Environment.NewLine +
                "RobotComponent V : " + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Code Generation")
        {
        }

        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RobotToolParameter(), "Robot Tool", "RT", "Robot Tool as Robot Tool, uses tool0 as default Robot Tool", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new OverrideRobotToolParameter(), "Override Robot Tool", "ORT", "Resulting Override Robot Tool");  //Todo: beef this up to be more informative.
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Gets Document ID
            Guid documentGUID = this.OnPingDocument().DocumentID;

            // Input variables
            RobotToolGoo _toolGoo = new RobotToolGoo();

            // Catch input data
            if (!DA.GetData(0, ref _toolGoo)) { _toolGoo.Value.Name = "tool0"; ; }

            // Create action
            OverrideRobotTool OverrideRobotTool = new OverrideRobotTool(_toolGoo.Value, documentGUID);

            // Output
            DA.SetData(0, OverrideRobotTool);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ChangeTool_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FEDDD15A-F7F9-4575-A7E8-6FC3C3B3E7AF"); }
        }

    }
}
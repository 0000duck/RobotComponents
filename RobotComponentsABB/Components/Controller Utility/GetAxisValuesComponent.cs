﻿// This file is part of RobotComponents. RobotComponents is licensed 
// under the terms of GNU General Public License as published by the 
// Free Software Foundation. For more information and the LICENSE file, 
// see <https://github.com/EDEK-UniKassel/RobotComponents>.

// System Libs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
// RobotComponents Libs
using RobotComponentsABB.Goos;
using RobotComponentsABB.Utils;
// ABB Libs
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.MotionDomain;

namespace RobotComponentsABB.Components.ControllerUtility
{
    /// <summary>
    /// RobotComponents Controller Utility : Get the Axis Values from a defined controller. An inherent from the GH_Component Class.
    /// </summary>
    public class GetAxisValuesComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetAxisValues class.
        /// </summary>
        public GetAxisValuesComponent()
          : base("Get Axis Values", "GA",
              "Gets the current robot axis values from a defined ABB robot controller."
                + System.Environment.NewLine +
                "RobotComponents : v" + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Controller Utility")
        {
        }

        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // To do: replace generic parameter with an RobotComponents Parameter
            pManager.AddGenericParameter("Robot Controller", "RC", "Controller to extract the axis values from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Internal Axis Values", "IAV", "Extracted internal Axis Values", GH_ParamAccess.tree);
            pManager.AddNumberParameter("External Axis Values", "EAV", "Extracted external Axis Values", GH_ParamAccess.tree);
        }

        // Fields
        private readonly GH_Structure<GH_Number> _internalAxisValues = new GH_Structure<GH_Number>();
        private readonly GH_Structure<GH_Number> _externalAxisValues = new GH_Structure<GH_Number>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            GH_Controller controllerGoo = null;

            // Catch input data
            if (!DA.GetData(0, ref controllerGoo)) { return; }

            // Clear output variables 
            _internalAxisValues.Clear();
            _externalAxisValues.Clear();

            // Data needed for making the datatree with axis values
            MechanicalUnitCollection mechanicalUnits = controllerGoo.Value.MotionSystem.MechanicalUnits;
            int internalAxisValuesPath = 0;
            int externalAxisValuesPath = 0;
            List<double> values;
            GH_Path path;

            // Make the output datatree with names with a branch for each mechanical unit
            for (int i = 0; i < mechanicalUnits.Count; i++)
            {
                // Get the ABB joint target of the mechanical unit
                MechanicalUnit mechanicalUnit = mechanicalUnits[i];
                JointTarget jointTarget = mechanicalUnit.GetPosition();

                // For internal axis values
                if (mechanicalUnit.Type == MechanicalUnitType.TcpRobot)
                {
                    values = GetInternalAxisValuesAsList(jointTarget);
                    path = new GH_Path(internalAxisValuesPath);
                    _internalAxisValues.AppendRange(values.ConvertAll(val => new GH_Number(val)), path);
                    internalAxisValuesPath += 1;
                }

                // For external axis values
                else
                {
                    values = GetExternalAxisValuesAsList(jointTarget);
                    path = new GH_Path(externalAxisValuesPath);
                    _externalAxisValues.AppendRange(values.GetRange(0, mechanicalUnit.NumberOfAxes).ConvertAll(val => new GH_Number(val)), path);
                    externalAxisValuesPath += 1;
                }
            }

            // Output
            DA.SetDataTree(0, _internalAxisValues);
            DA.SetDataTree(1, _externalAxisValues);
        }

        // Additional methods
        #region additional methods
        /// <summary>
        /// Get the internal axis values from a defined joint target
        /// </summary>
        /// <param name="jointTarget"> The joint target to get the internal axis values from. </param>
        /// <returns></returns>
        private List<double> GetInternalAxisValuesAsList(JointTarget jointTarget)
        {
            // Initiate the list with internal axis values
            List<double> result = new List<double>() { };

            // Get the axis values from the joint target
            result.Add(jointTarget.RobAx.Rax_1);
            result.Add(jointTarget.RobAx.Rax_2);
            result.Add(jointTarget.RobAx.Rax_3);
            result.Add(jointTarget.RobAx.Rax_4);
            result.Add(jointTarget.RobAx.Rax_5);
            result.Add(jointTarget.RobAx.Rax_6);

            // Replace large numbers (the not connected axes) with an axis value equal to zero 
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] > 9.0e+8)
                {
                    result[i] = 0;
                }
            }

            // Return the list with axis values
            return result;
        }

        /// <summary>
        /// Get the external axis values from a defined joint target
        /// </summary>
        /// <param name="jointTarget"> The joint target to get the external axis values from. </param>
        /// <returns></returns>
        private List<double> GetExternalAxisValuesAsList(JointTarget jointTarget)
        {
            // Initiate the list with external axis values
            List<double> result = new List<double>() { };

            // Get the axis values from the joint target
            result.Add(jointTarget.ExtAx.Eax_a);
            result.Add(jointTarget.ExtAx.Eax_b);
            result.Add(jointTarget.ExtAx.Eax_c);
            result.Add(jointTarget.ExtAx.Eax_d);
            result.Add(jointTarget.ExtAx.Eax_e);
            result.Add(jointTarget.ExtAx.Eax_f);

            // Replace large numbers (the not connected axes) with an axis value equal to zero 
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] > 9.0e+8)
                {
                    result[i] = 0;
                }
            }

            // Return the list with axis values
            return result;
        }
        #endregion

        #region menu item
        /// <summary>
        /// Adds the additional items to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Documentation", MenuItemClickComponentDoc, Properties.Resources.WikiPage_MenuItem_Icon);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Documentation" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickComponentDoc(object sender, EventArgs e)
        {
            string url = Documentation.ComponentWeblinks[this.GetType()];
            System.Diagnostics.Process.Start(url);
        }
        #endregion

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return RobotComponentsABB.Properties.Resources.GetAxisValues_Icon; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("691a3c83-114a-4c80-81b9-2e1407004a24"); }
        }
    }
}
﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;
// Rhino Libs
using Rhino.Geometry;
// RobotComponents Libs
using RobotComponents.BaseClasses.Kinematics;
using RobotComponentsGoos.Definitions;
using RobotComponentsABB.Parameters.Definitions;

namespace RobotComponentsABB.Components.Simulation
{
    /// <summary>
    /// RobotComponents Forward Kinematics component. An inherent from the GH_Component Class.
    /// </summary>
    public class ForwardKinematicsComponent : GH_Component 
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public ForwardKinematicsComponent()
          : base("Forward Kinematics", "FK",
              "Computes the position of the end-effector of a defined ABB robot based on a set of given axis values."
                + System.Environment.NewLine +
                "RobotComponents : v" + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Simulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RobotInfoParameter(), "Robot Info", "RI", "Robot Info as Robot Info", GH_ParamAccess.item);
            pManager.AddNumberParameter("Internal Axis Values", "IAV", "Internal Axis Values as List", GH_ParamAccess.list, new List<double> { 0, 0, 0, 0, 0, 0 } );
            pManager.AddNumberParameter("External Axis Values", "EAV", "External Axis Values as List", GH_ParamAccess.list, new List<double> { 0, 0, 0, 0, 0, 0 });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_MeshParam("Posed Meshes", "PM", "Posed Robot and External Axis meshes");  //Todo: beef this up to be more informative.
            pManager.Register_PlaneParam("End Plane", "EP", "Robot TCP plane placed on Target");
            pManager.Register_PlaneParam("External Axis Planes", "EAP", "Exernal Axis Planes as list of Planes");
        }

        // Fields
        private ForwardKinematics _fk = new ForwardKinematics();
        private bool _hideMesh = false;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            GH_RobotInfo robotInfoGoo = null;
            List<double> internalAxisValues = new List<double>();
            List<double> externalAxisValues = new List<double>();

            // Catch input data
            if (!DA.GetData(0, ref robotInfoGoo)) { return; }
            if (!DA.GetDataList(1, internalAxisValues)) { return; }
            if (!DA.GetDataList(2, externalAxisValues)) { return; }

            // Add up missing internal axisValues
            for (int i = internalAxisValues.Count; i <6; i++)
            {
                internalAxisValues.Add(0);
            }

            // Add up missing external axisValues
            for (int i = externalAxisValues.Count; i < 6; i++)
            {
                externalAxisValues.Add(0);
            }

            // Calcuate the robot pose
            ForwardKinematics forwardKinematics = new ForwardKinematics(robotInfoGoo.Value, internalAxisValues, externalAxisValues, _hideMesh);
            forwardKinematics.Calculate();

            // Check the values
            for (int i = 0; i < forwardKinematics.ErrorText.Count; i++)
            {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, forwardKinematics.ErrorText[i]);
            }

            // Create data tree for output of all posed meshes
            GH_Structure<GH_Mesh> meshes = new GH_Structure<GH_Mesh>();

            // Fill data tree with meshes
            if (_hideMesh == false)
            {
                meshes = GetPosedMeshesDataTree(forwardKinematics);
            }

            // Output
            _fk = forwardKinematics;
            DA.SetDataTree(0, meshes); 
            DA.SetData(1, forwardKinematics.TCPPlane); // Outputs the TCP as a plane
            DA.SetDataList(2, forwardKinematics.ExternalAxisPlanes); // Outputs the External Axis Planes
        }

        /// <summary>
        /// Transform the posed meshes rom the forward kinematics to a datatree
        /// </summary>
        /// <param name="forwardKinematics"> The forward kinematics the posed meshes will be extracted from. </param>
        /// <returns> The data tree structure with all the posed meshes. </returns>
        public GH_Structure<GH_Mesh> GetPosedMeshesDataTree(ForwardKinematics forwardKinematics)
        {
            // Create data tree for output of alle posed meshes
            GH_Structure<GH_Mesh> meshes = new GH_Structure<GH_Mesh>();

            {
                // Robot pose meshes
                List<Mesh> posedInternalAxisMeshes = forwardKinematics.PosedInternalAxisMeshes;

                // Data tree path
                GH_Path path = new GH_Path(0);

                // Save the posed meshes
                for (int i = 0; i < posedInternalAxisMeshes.Count; i++)
                {
                    meshes.Append(new GH_Mesh(posedInternalAxisMeshes[i]), path);
                }

                // Extenal axis meshes
                List<List<Mesh>> posedExternalAxisMeshes = forwardKinematics.PosedExternalAxisMeshes;

                // Loop over all the external axes
                for (int i = 0; i < posedExternalAxisMeshes.Count; i++)
                {
                    // Data tree path
                    path = new GH_Path(i + 1);

                    // Save the posed meshes
                    for (int j = 0; j < posedExternalAxisMeshes[i].Count; j++)
                    {
                        meshes.Append(new GH_Mesh(posedExternalAxisMeshes[i][j]), path);
                    }
                }
            }
            
            // Return the data tree stucture
            return meshes;
        }

        /// <summary>
        /// This method displays the robot pose for the given axis values. 
        /// </summary>
        /// <param name="args"> Preview display arguments for IGH_PreviewObjects. </param>
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // Check if there is a mesh available to display and the onlyTCP function not active
            if (_fk.PosedInternalAxisMeshes != null && !_hideMesh)
            {
                // A boolean that defines if the axis values are valid.
                bool AxisAreValid = true;

                // Chekc if the internal axis values are valid
                for (int i = 0; i < _fk.InternalAxisInLimit.Count; i++)
                {
                    if (_fk.InternalAxisInLimit[i] == false)
                    {
                        AxisAreValid = false;
                        break;
                    }
                }

                // Check if the external axis values are valid
                if (AxisAreValid == true)
                {
                    for (int i = 0; i < _fk.ExternalAxisInLimit.Count; i++)
                    {
                        if (_fk.ExternalAxisInLimit[i] == false)
                        {
                            AxisAreValid = false;
                            break;
                        }
                    }
                }

                // Initiate the display color and transparancy of the robot mesh
                Color color;
                double trans;

                // Set the display color and transparancy of the robot mesh
                if (AxisAreValid == true)
                {
                    color = Color.FromArgb(225, 225, 225);
                    trans = 0.0;
                }
                else
                {
                    color = Color.FromArgb(150, 0, 0);
                    trans = 0.5;
                }

                // Display the internal axes of the robot
                for (int i = 0; i != _fk.PosedInternalAxisMeshes.Count; i++)
                {
                    args.Display.DrawMeshShaded(_fk.PosedInternalAxisMeshes[i], new Rhino.Display.DisplayMaterial(color, trans));
                }

                // Display the external axes
                for (int i = 0; i != _fk.PosedExternalAxisMeshes.Count; i++)
                {
                    for (int j = 0; j != _fk.PosedExternalAxisMeshes[i].Count; j++)
                    {
                        args.Display.DrawMeshShaded(_fk.PosedExternalAxisMeshes[i][j], new Rhino.Display.DisplayMaterial(color, trans));
                    }
                }
            }
        }

        // Methods for creating custom menu items and event handlers when the custom menu items are clicked
        #region menu items
        /// <summary>
        /// Boolean that indicates if the custom menu item for hding the robot mesh is checked
        /// </summary>
        public bool SetHideMesh
        {
            get { return _hideMesh; }
            set { _hideMesh = value; }
        }

        /// <summary>
        /// Add our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="writer"> Provides access to a subset of GH_Chunk methods used for writing archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            // Add our own fields
            writer.SetBoolean("Set Hide Mesh", SetHideMesh);

            // Call the base class implementation.
            return base.Write(writer);
        }

        /// <summary>
        /// Read our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="reader"> Provides access to a subset of GH_Chunk methods used for reading archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            // Read our own fields
            SetHideMesh = reader.GetBoolean("Set Hide Mesh");

            // Call the base class implementation.
            return base.Read(reader);
        }

        /// <summary>
        /// Adds the additional items to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            // Add menu separator
            Menu_AppendSeparator(menu);

            // Add custom menu items
            Menu_AppendItem(menu, "Hide Mesh", MenuItemClickHideMesh, true, SetHideMesh);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Hide Mesh" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        public void MenuItemClickHideMesh(object sender, EventArgs e)
        {
            RecordUndoEvent("Set Hide Mesh");
            _hideMesh = !_hideMesh;

            // Expire solution
            ExpireSolution(true);
        }
        #endregion

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ForwardKinematics_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5D6207E3-2051-4DAD-A1D9-C4A9EAD371FB"); }
        }

    }
}

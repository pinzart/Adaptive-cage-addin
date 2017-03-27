using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

[TransactionAttribute(TransactionMode.Manual)]
[RegenerationAttribute(RegenerationOption.Manual)]
public class SolidMatch : IExternalCommand
{
   public Result Execute(
     ExternalCommandData commandData,
     ref string message,
     ElementSet elements)
   {
      //Get application and document objects
      UIApplication uiApp = commandData.Application;
      Document doc = uiApp.ActiveUIDocument.Document;

      //Define a Reference object to accept the pick result.
      Reference pickedRef = null;

      //Pick a group
      Selection sel = uiApp.ActiveUIDocument.Selection;
      pickedRef = sel.PickObject(ObjectType.Element, "Please select the first solid you want to match");
      Element firstElem = doc.GetElement(pickedRef);

      pickedRef = sel.PickObject(ObjectType.Element, "Please select the second solid you want to match");
      Element secondElem = doc.GetElement(pickedRef);

      Options geomOpt = new Options();
      geomOpt.ComputeReferences = true;
      var firstGeom = firstElem.get_Geometry(geomOpt);
      var secondGeom = secondElem.get_Geometry(geomOpt);

      Result result = Result.Succeeded;
      result = ValidateGeometries(firstGeom, secondGeom);
      if (result != Result.Succeeded)
         return result;

      // FOR THE SAKE OF SIMPLICITY, AND SINCE THIS IS A PROOF OF CONCEPT WE WILL ONLY WORK
      // WITH GEOMETRIES THAT HAVE A SINGLE SOLID!!!
      // FURTHER IMPLEMENTATION IS NEEDED TO COVER LAYERED GEOMETRY, ETC.

      //Transaction trans = new Transaction(doc);
      return determineMatch(firstGeom, secondGeom);
      //trans.Commit();

      //return Result.Succeeded;
   }

   private Result ValidateGeometries(GeometryElement first, GeometryElement second)
   {
      Result result = Result.Succeeded;

      //// We only support solids with single geometries // no layered geometries
      //if (firstGeom.)
      //   return stop("We only support solids with single geometries");
      int firstFaceCount = 0;
      int secondFaceCount = 0;

      foreach (GeometryObject geomObj in first)
      {
         Solid geomSolid = geomObj as Solid;
         firstFaceCount = geomSolid.Faces.Size;
      }

      foreach (GeometryObject geomObj in second)
      {
         Solid geomSolid = geomObj as Solid;
         secondFaceCount = geomSolid.Faces.Size;
      }

      if (firstFaceCount != secondFaceCount)
         return stop("Failed. Diffrent number of faces.");
      return result;
   }

   private Result stop(string err)
   {
      TaskDialog.Show("Error", err);
      return Result.Failed;
   }

   private Result determineParallelCount(XYZ faceNorm, Solid secondSolid, ref int parallelCount)
   {
      parallelCount = 0;

      // rotate element to norm direction
      // check how many faces are parallel

      foreach (Face geomFace in secondSolid.Faces)
      {
         var surf = geomFace.GetSurface();
         Plane plane = surf as Plane;
         if (plane == null)
            return stop("Non planar faces!");

         XYZ norm = plane.Normal;
         //Transform trf = Transform.;

      }


      //return parallelCount;
      return Result.Succeeded;
   }

   private Result GetCandidateList(ref List<XYZ> norms, ref int highestCount, Solid firstSolid, Solid secondSolid)
   {
      // build a list of norms that when the second solid is rotated 
      // in its direction we get the highest parallelism count
      foreach (Face geomFace in firstSolid.Faces)
      {
         var surf = geomFace.GetSurface();

         Plane plane = surf as Plane;
         if (plane == null)
            return stop("Non planar faces!");

         XYZ norm = plane.Normal;
         var count = 0;
         determineParallelCount(norm, secondSolid, ref count);

         // retain only the normals with the highest parallel count
         if (count >= highestCount)
         {
            if (count > highestCount)
               norms.Clear();

            highestCount = count;
            norms.Add(norm);
         }
      }

      return Result.Succeeded;
   }

   private Result determineMatch(GeometryElement first, GeometryElement second)
   {
      Result result = Result.Succeeded;

      int highestCount = 0;
      List<XYZ> norms = new List<XYZ>();

      Solid firstSolid = null, secondSolid = null;

      foreach (GeometryObject geomObj in first)
         firstSolid = geomObj as Solid;

      foreach (GeometryObject geomObj in second)
         secondSolid = geomObj as Solid;

      if (firstSolid == null || secondSolid == null)
         return stop("No solid!");

      result = GetCandidateList(ref norms, ref highestCount, firstSolid, secondSolid);

      // determine list of candidates

      // if more candidates use other criteria to distinguish them; area?

      return result;
   }
}
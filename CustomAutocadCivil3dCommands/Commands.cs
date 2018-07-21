using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.Runtime;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using CustomAutocadCivil3dCommands;


namespace CustomAutocadCivil3dCommands
{
    public class Commands
    {
        [CommandMethod("sectionlabelonbandset")]
        public void sectionlabelonbandset()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions dat = new Functions(acDoc, civDoc);
            dat.SectionLabelOnBandSet();
        }

        [CommandMethod("Civil2Autocad")]
        public void Civil2Autocad()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions fnc = new Functions(doc, civDoc);
            List<BaseC3dObject> oList = fnc.GetObjects("\nSelect the AECC objects:");
            if (oList.Count > 0)
            {
                Functions.ExplodeAecObj(oList);
            }
        }

        [CommandMethod("ALI2PRO")]
        public void Ali2Pro()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                    (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions fnc = new Functions(doc, civDoc);
            fnc.Ali2pro();
        }
        /*
        [CommandMethod("TraceAli")]
        public void TraceAli()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions fnc = new Functions(doc, civDoc);
            fnc.deneme(); 
        }
        [CommandMethod("sempil")]
        public void Sempil()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions fnc = new Functions(doc, civDoc);
            
            fnc.SampleLines();
        } */

        [CommandMethod("Point2Section")]
        public void Point2Section()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions fnc = new Functions(doc, civDoc);
            fnc.Point2Sec();

        }

        [CommandMethod("ExportGeoJSON")]
        public void ExportGeoJSON()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
               (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Functions fnc = new Functions(doc, civDoc);
            fnc.ExportGeoJSON();

        }

         [CommandMethod("LandXml")]
         public void LandXml()
         {
             Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
                (" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
             Document doc = Application.DocumentManager.MdiActiveDocument;
             CivilDocument civDoc = CivilApplication.ActiveDocument;
             Functions fnc = new Functions(doc, civDoc);
             fnc.LandXmlHandling();

         }
    }
}


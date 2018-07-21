using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotSpatial.Projections;
using Autodesk.AutoCAD.Windows;
using System.Xml;

namespace CustomAutocadCivil3dCommands
{
    public class Functions : BaseC3dObject
    {
        public Document Dwg { get; set; }
        public Database Db { get; set; }
        public Editor Ed { get; set; }
        public CivilDocument CivDwg { get; set; }

        public Functions(Document dwg, CivilDocument civDwg)
        {
            Dwg = dwg;
            Ed = dwg.Editor;
            Db = dwg.Database;
            CivDwg = civDwg;
        }

        public void Ali2pro()
        {

            Alignment selectedAli = GetAlignment();
            if (selectedAli != null)
            {
                ObjectIdCollection prfIds = selectedAli.GetProfileViewIds();

                try
                {
                    Boolean loopControl = true;
                    while (loopControl)
                    {
                        foreach (ObjectId item in prfIds)
                        {
                            using (Transaction ts = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                            {
                                PromptPointOptions pntOpt = new PromptPointOptions("\nSelect a point");
                                pntOpt.AllowNone = true;
                                pntOpt.Message = "dogruSec";
                                PromptPointResult pnt = Ed.GetPoint("\nSelect the point");
                                if (pnt.Status != PromptStatus.OK) { return; };
                                Double km = 0;
                                Double ofset = 0;
                                double x = 0;
                                double y = 0;
                                selectedAli.StationOffset(pnt.Value.X, pnt.Value.Y, ref km, ref ofset);
                                ProfileView pv = ts.GetObject(item, OpenMode.ForRead) as ProfileView;

                                pv.FindXYAtStationAndElevation(km, pv.Location.Y, ref x, ref y);

                                Point3d p1 = new Point3d(x, pv.Location.Y, 0);
                                Point3d p2 = new Point3d(x, pv.Location.Y + pv.ElevationMax, 0);
                                Xline xl = new Xline();
                                xl.BasePoint = p1;
                                xl.SecondPoint = p2;
                                BlockTable acBlkTbl;
                                acBlkTbl = ts.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec;
                                acBlkTblRec = ts.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                                acBlkTblRec.AppendEntity(xl);
                                ts.AddNewlyCreatedDBObject(xl, true);
                                ts.Commit();

                            }
                        }
                    }
                }
                catch (System.Exception)
                {

                    throw;
                }
            }
        }

        public void Point2Sec()
        {
            SampleLine smpLine = GetSampleLine();
            SampleLineVertexCollection smpVertices = smpLine.Vertices;
            List<Point3d> pntList = new List<Point3d>();
            Autodesk.AutoCAD.DatabaseServices.Polyline pl = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            foreach (SampleLineVertex item in smpVertices)
            {
                pntList.Add(item.Location);
            }
            Line l = new Line(pntList[0], pntList[2]);
            ObjectIdCollection sectionViewID = smpLine.GetSectionViewIds();
            Boolean loopControl = true;
            while (loopControl)
            {
                using (Transaction ts = Dwg.TransactionManager.StartTransaction())
                {
                    try
                    {
                        PromptPointResult pnt = Ed.GetPoint("\nSelect the point");
                        if (pnt.Status == PromptStatus.Cancel) return;
                        Point3d pntOnSampleLine = l.GetClosestPointTo(pnt.Value, false);
                        Double dist = Math.Sqrt(Math.Pow((pntList[0].X - pntOnSampleLine.X), 2) + Math.Pow(pntList[0].Y - pntOnSampleLine.Y, 2));
                        SectionView sectionView = ts.GetObject(sectionViewID[0], OpenMode.ForRead) as SectionView;
                        Point3d pntOnSec = sectionView.Location;
                        Double left = sectionView.OffsetLeft;
                        Point3d startPoint = new Point3d(sectionView.Location.X + left, sectionView.Location.Y, sectionView.Location.Z);
                        Point3d targetPoint = new Point3d(startPoint.X + dist, startPoint.Y, startPoint.Z);
                        Point3d targetPoint2 = new Point3d(targetPoint.X, targetPoint.Y - 5, targetPoint.Z);
                        Xline xl = new Xline();
                        xl.BasePoint = targetPoint;
                        xl.SecondPoint = targetPoint2;
                        AppendEntity(Db.BlockTableId, xl);
                        ts.Commit();

                    }
                    catch (System.Exception)
                    {
                        throw;
                    }
                }
            }

        }
        public void Deneme()
        {

            Ed.PointMonitor += OnMouseMove;

            PromptPointOptions pOpt = new PromptPointOptions("helleo");
            PromptPointResult pRes = Ed.GetPoint(pOpt);
            if (pRes.Status == PromptStatus.Cancel)
            {
                Ed.PointMonitor -= OnMouseMove;
            }

        }

        private void OnMouseMove(object sender, PointMonitorEventArgs e)
        {
            Point3d a = e.Context.RawPoint;
            DrawPoint(a);
        }

        private void DrawPoint(Point3d a)
        {
            DBPoint b = new DBPoint(a);
            Ed.WriteMessage("selamın aleyküm");
            AppendEntity(Db.BlockTableId, b);
        }
        public List<BaseC3dObject> GetAlignmentIds()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            ObjectIdCollection alignIds = civilDoc.GetAlignmentIds();
            List<BaseC3dObject> alignList = new List<BaseC3dObject>();
            if (alignIds.Count == 0)
            {
                return null;
            }
            else
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    try
                    {

                        foreach (ObjectId id in alignIds)
                        {
                            Alignment align = ts.GetObject(id, OpenMode.ForRead) as Alignment;
                            BaseC3dObject bObject = new BaseC3dObject();
                            bObject.Id = align.ObjectId;
                            bObject.Name = align.Name;
                            alignList.Add(bObject);
                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                }
                return alignList;
            }

        }
        public Alignment GetAlignment()
        {
            PromptEntityOptions opt = new PromptEntityOptions("\nSelect the Alignment:");
            opt.SetRejectMessage("\nOnly alignment");
            opt.AddAllowedClass(typeof(Alignment), true);
            PromptEntityResult res = Ed.GetEntity(opt);
            if (res.Status != PromptStatus.OK) { return null; };
            using (Transaction ts = Dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    Alignment al = ts.GetObject(res.ObjectId, OpenMode.ForRead) as Alignment;
                    return al;

                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }
        public ProfileView GetProfileView()
        {
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect the profile view");
            peo.SetRejectMessage("\nOnly profile view");
            peo.AddAllowedClass(typeof(ProfileView), true);
            PromptEntityResult res = Ed.GetEntity(peo);
            if (res.Status == PromptStatus.OK) { }
            using (Transaction ts = Dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    ProfileView pv = ts.GetObject(res.ObjectId, OpenMode.ForRead) as ProfileView;
                    return pv;
                }
                catch (System.Exception)
                {

                    throw;
                }
            }

        }
        public Profile GetProfile()
        {
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect the profile view");
            peo.SetRejectMessage("\nOnly profile view");
            peo.AddAllowedClass(typeof(Profile), true);
            PromptEntityResult res = Ed.GetEntity(peo);
            if (res.Status != PromptStatus.OK) { return null; }
            using (Transaction ts = Dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    Profile prf = ts.GetObject(res.ObjectId, OpenMode.ForRead) as Profile;
                    return prf;
                }
                catch (System.Exception)
                {

                    throw;
                }
            }

        }

        public SampleLine GetSampleLine()
        {
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect the SampleLine");
            peo.SetRejectMessage("\nOnly SampleLine");
            peo.AddAllowedClass(typeof(SampleLine), true);
            PromptEntityResult res = Ed.GetEntity(peo);
            if (res.Status != PromptStatus.OK) { return null; }
            using (Transaction ts = Dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    SampleLine smpLine = ts.GetObject(res.ObjectId, OpenMode.ForRead) as SampleLine;
                    return smpLine;

                }
                catch (System.Exception)
                {

                    throw;
                }
            }
        }
        public static List<BaseC3dObject> GetAlignmentLabelSets()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            Autodesk.Civil.DatabaseServices.Styles.AlignmentLabelSetStyleCollection aliLabelSetIds =
                civilDoc.Styles.LabelSetStyles.AlignmentLabelSetStyles;
            List<BaseC3dObject> aliLabelSetList = new List<BaseC3dObject>();
            if (aliLabelSetIds.Count == 0) return null;
            else
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        foreach (ObjectId id in aliLabelSetIds)
                        {
                            Autodesk.Civil.DatabaseServices.Styles.AlignmentLabelSetStyle aliLabelSet =
                                id.GetObject(OpenMode.ForRead) as Autodesk.Civil.DatabaseServices.Styles.AlignmentLabelSetStyle;
                            BaseC3dObject bObject = new BaseC3dObject();
                            bObject.Name = aliLabelSet.Name;
                            bObject.Id = aliLabelSet.ObjectId;
                            aliLabelSetList.Add(bObject);
                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                }
                return aliLabelSetList;
            }

        }
        public static List<BaseC3dObject> GetAlignmentStyles()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            Autodesk.Civil.DatabaseServices.Styles.AlignmentStyleCollection aliStyles = civilDoc.Styles.AlignmentStyles;
            List<BaseC3dObject> aliStylesList = new List<BaseC3dObject>();
            if (aliStyles.Count == 0) return null;
            else
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        foreach (ObjectId id in aliStyles)
                        {
                            Autodesk.Civil.DatabaseServices.Styles.AlignmentStyle aliStyle =
                                id.GetObject(OpenMode.ForRead) as Autodesk.Civil.DatabaseServices.Styles.AlignmentStyle;
                            BaseC3dObject bObject = new BaseC3dObject();
                            bObject.Name = aliStyle.Name;
                            bObject.Id = aliStyle.ObjectId;
                            aliStylesList.Add(bObject);
                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                }
                return aliStylesList;
            }
        }
        public static List<BaseC3dObject> GetSites()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            ObjectIdCollection sitesID = civilDoc.GetSiteIds();
            List<BaseC3dObject> sites = new List<BaseC3dObject>();
            if (sitesID.Count == 0) return null;
            else
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {

                    try
                    {
                        foreach (ObjectId item in sitesID)
                        {
                            Site site = item.GetObject(OpenMode.ForRead) as Site;
                            BaseC3dObject bObject = new BaseC3dObject();
                            bObject.Name = site.Name;
                            bObject.Id = site.ObjectId;
                            sites.Add(bObject);
                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                }
                return sites;
            }
        }
        public List<BaseC3dObject> GetObjects(string question)
        {
            Ed.WriteMessage(question);
            //Objeleri Seçmece ve Listeye Atmaca
            PromptSelectionOptions opt = new PromptSelectionOptions();
            PromptSelectionResult res = Ed.GetSelection(opt);
            SelectionSet SS = res.Value;
            List<BaseC3dObject> oList = new List<BaseC3dObject>();
            ////////////////////////////////////////----OBJE SEÇMECE ----- ///////////////////////////////
            // seçilmiş objeleri BASEC3D objesine çevir
            if (res.Status == PromptStatus.OK)
            {
                foreach (SelectedObject item in SS)
                {
                    BaseC3dObject bobject = new BaseC3dObject();
                    bobject.Id = item.ObjectId;
                    bobject.Name = item.ObjectId.ObjectClass.DxfName;
                    oList.Add(bobject);
                }

            }

            return oList;
        }
        public Point3d GetPoint()
        {

            PromptPointOptions opt = new PromptPointOptions("Noktayı seçiniz..");
            PromptPointResult res = Ed.GetPoint(opt);
            if (res.Status == PromptStatus.OK)
            {
                Point3d pnt = new Point3d(res.Value.X, res.Value.Y, res.Value.Z);
                return pnt;
            }
            else
            {
                Point3d pnt = new Point3d(0, 0, 0);
                return pnt;
            }

        }
        public static void ExplodeAecObj(List<BaseC3dObject> oList)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            Editor ed = doc.Editor;


            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                try
                {   //Patlatmaca
                    DBObjectCollection dbObjCol = new DBObjectCollection();
                    foreach (BaseC3dObject item in oList)
                    {
                        //Sadece C3D objelerini patlat
                        if (item.Name.Contains("AECC"))
                        {
                            Autodesk.Civil.DatabaseServices.Entity ent = item.Id.GetObject(OpenMode.ForRead)
                            as Autodesk.Civil.DatabaseServices.Entity;
                            ent.Explode(dbObjCol);
                        }

                    }
                    //open the block table
                    BlockTable acBlkTbl;
                    acBlkTbl = ts.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                    //create the blocktablerecord
                    BlockTableRecord btr = new BlockTableRecord();
                    int i = 0;
                    String g = string.Format("C3D-{0}", i.ToString());
                    //set its name
                    while (acBlkTbl.Has(g))
                    {
                        i++;
                        g = string.Format("C3D-{0}", i.ToString());

                    }

                    btr.Name = g;

                    acBlkTbl.UpgradeOpen();
                    ObjectId btrId = acBlkTbl.Add(btr);
                    ts.AddNewlyCreatedDBObject(btr, true);


                    foreach (Autodesk.AutoCAD.DatabaseServices.Entity obj in dbObjCol)
                    {
                        btr.AppendEntity(obj);
                        ts.AddNewlyCreatedDBObject(obj, true);
                    }
                    BlockTableRecord ms;
                    ms = ts.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br = new BlockReference(Point3d.Origin, btrId);
                    ms.AppendEntity(br);
                    ts.AddNewlyCreatedDBObject(br, true);
                    ts.Commit();
                    ed.WriteMessage("\nCreated a block named {0} from {1} selected C3D entities", g, dbObjCol.Count);

                }
                catch (System.Exception)
                {

                    throw;
                }
            }
        }


        public static BaseC3dObject CreateAlignment(
            string alName,
            string layerName,
            string stil,
            string labelSet,
            string desc)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            ObjectId al = Alignment.Create(civilDoc, alName, null, layerName, stil, labelSet);
            BaseC3dObject alObject = new BaseC3dObject();
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Alignment alignment = al.GetObject(OpenMode.ForWrite) as Alignment;
                    alignment.Layer = layerName;
                    alignment.Description = desc;
                    alObject.Name = alignment.Name;
                    alObject.Id = alignment.ObjectId;
                    ts.Commit();

                }
                catch (System.Exception)
                {


                    throw;
                }
            }
            return alObject;
        }
        public static List<BaseC3dObject> GetLayers()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            List<BaseC3dObject> layers = new List<BaseC3dObject>();
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable lyrTable = ts.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    foreach (ObjectId item in lyrTable)
                    {
                        LayerTableRecord lyrTabRec = item.GetObject(OpenMode.ForRead) as LayerTableRecord;
                        BaseC3dObject bObject = new BaseC3dObject();
                        bObject.Id = lyrTabRec.ObjectId;
                        bObject.Name = lyrTabRec.Name;
                        layers.Add(bObject);

                    }
                    ts.Commit();
                }
                catch (System.Exception)
                {
                    ts.Abort();
                    throw;
                }

            }
            return layers;
        }
        private void AppendEntity(ObjectId acBlkTblId, Autodesk.AutoCAD.DatabaseServices.Entity ent)
        {
            Database acCurDb = Dwg.Database;
            using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = ts.GetObject(acBlkTblId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = ts.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                acBlkTblRec.AppendEntity(ent);
                ts.AddNewlyCreatedDBObject(ent, true);
                ts.Commit();
            }
        }
        public void SectionLabelOnBandSet()
        {
            Database acCurDb = Dwg.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            //---------------DATUM AND ORIGIN SELECTION-----------------
            ed.WriteMessage(" By Grkmus | gorkemtosun@gmail.com | https://tr.linkedin.com/in/gorkemtosun");
            PromptEntityOptions datumTextOpt = new PromptEntityOptions("\nPlease select the datum text");
            datumTextOpt.SetRejectMessage("\nsadece Text");
            datumTextOpt.AddAllowedClass(typeof(MText), true);
            datumTextOpt.AddAllowedClass(typeof(DBText), true);
            PromptEntityResult datumTextRes = ed.GetEntity(datumTextOpt);
            if (datumTextRes.Status != PromptStatus.OK) return;
            ObjectId datumTextId = datumTextRes.ObjectId;

            PromptPointOptions originPointOpt = new PromptPointOptions("\nPlease select the origin point");
            PromptPointResult originPointRes = ed.GetPoint(originPointOpt);
            if (originPointRes.Status != PromptStatus.OK) return;
            Point3d originPoint = originPointRes.Value;
            double originX = originPoint.X;
            double originY = originPoint.Y;

            //---------------MTEXT SELECTION-----------------
            PromptEntityOptions mTextOpt = new PromptEntityOptions("\nSelect the base Mtext Object");
            mTextOpt.SetRejectMessage("\nText for Elevation");
            mTextOpt.AddAllowedClass(typeof(MText), true);
            mTextOpt.AddAllowedClass(typeof(DBText), true);
            PromptEntityResult mTextRes = ed.GetEntity(mTextOpt);
            if (mTextRes.Status != PromptStatus.OK) return;

            //mtext for distance
            PromptEntityOptions mTextDist = new PromptEntityOptions("\nSelect the base Mtext Object for distance");
            mTextDist.SetRejectMessage("\nText for Distance");
            mTextDist.AddAllowedClass(typeof(MText), true);
            mTextDist.AddAllowedClass(typeof(DBText), true);
            PromptEntityResult mTextDistRes = ed.GetEntity(mTextOpt);
            if (mTextDistRes.Status != PromptStatus.OK) return;
            double ofset = 1;
            double elevation = 2;
            try
            {
                Boolean loopControl = true;
                while (loopControl)
                {
                    using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                    {
                        //---------------BLOCKTABLE---------------//
                        BlockTable acBlkTbl;
                        acBlkTbl = ts.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = ts.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        TextStyleTable tst = ts.GetObject(acCurDb.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                        //-----------------------POINT------------------//
                        PromptPointOptions prOp = new PromptPointOptions("\nSelect the points that you want to label");
                        PromptPointResult pr = ed.GetPoint(prOp);
                        if (pr.Status == PromptStatus.Cancel) return;
                        Point3d pnt = pr.Value;
                        //--------------------CREATING TEXTS-----------//
                        MText textElevation = new MText();
                        DBText textElevation2 = new DBText();
                        MText textDistance = new MText();
                        DBText textDistance2 = new DBText();
                        //---------------DATUM AND ORIGIN-----------------
                        switch (datumTextId.ObjectClass.Name)
                        {
                            case "AcDbMText":
                                MText datumText = datumTextId.GetObject(OpenMode.ForRead) as MText;
                                double datum = Double.Parse(datumText.Contents);
                                ofset = pnt.X - originX;
                                elevation = pnt.Y - originY + datum;
                                break;
                            case "AcDbText":
                                DBText datumText2 = datumTextId.GetObject(OpenMode.ForRead) as DBText;
                                double datum2 = Double.Parse(datumText2.TextString);
                                ofset = pnt.X - originX;
                                elevation = pnt.Y - originY + datum2;
                                break;
                        }
                        //-------------------LINE-------------------//
                        Line line = new Line();
                        line.StartPoint = new Point3d(pnt.X, pnt.Y, pnt.Z);
                        line.EndPoint = new Point3d(pnt.X, pnt.Y - (pnt.Y - originY), pnt.Z);
                        LinetypeTable lineTypeTable = acCurDb.LinetypeTableId.GetObject(OpenMode.ForRead) as LinetypeTable;
                        if (lineTypeTable.Has("DASHED") == true)
                        {
                            line.Linetype = "DASHED";
                        }
                        line.ColorIndex = 8;
                        line.LineWeight = (LineWeight)0.13;
                        //--------------------MTEXT------------------//
                        switch (mTextRes.ObjectId.ObjectClass.Name)
                        {
                            case "AcDbMText":
                                MText baseObject = mTextRes.ObjectId.GetObject(OpenMode.ForRead) as MText;
                                textElevation.SetPropertiesFrom(baseObject);
                                textElevation.Contents = elevation.ToString("F2");
                                textElevation.Location = new Point3d(pnt.X, baseObject.Location.Y, pnt.Z);
                                textElevation.Rotation = baseObject.Rotation;
                                textElevation.TextHeight = baseObject.TextHeight;
                                textElevation.Color = baseObject.Color;
                                textElevation.TextStyleId = baseObject.TextStyleId;
                                textElevation.Attachment = AttachmentPoint.MiddleCenter;
                                textElevation.Layer = baseObject.Layer;
                                acBlkTblRec.AppendEntity(textElevation);
                                ts.AddNewlyCreatedDBObject(textElevation, true);
                                break;
                            case "AcDbText":
                                DBText baseObject2 = mTextRes.ObjectId.GetObject(OpenMode.ForRead) as DBText;
                                textElevation2.VerticalMode = TextVerticalMode.TextVerticalMid;
                                //textElevation2.HorizontalMode = TextHorizontalMode.TextLeft;
                                textElevation2.AlignmentPoint = new Point3d(pnt.X, baseObject2.Position.Y, pnt.Z);
                                textElevation2.Height = baseObject2.Height;
                                textElevation2.Rotation = baseObject2.Rotation;
                                textElevation2.TextString = elevation.ToString("F2");
                                acBlkTblRec.AppendEntity(textElevation2);
                                ts.AddNewlyCreatedDBObject(textElevation2, true);
                                break;
                        }
                        switch (mTextDistRes.ObjectId.ObjectClass.Name)
                        {
                            case "AcDbMText":
                                MText baseTextDist = mTextDistRes.ObjectId.GetObject(OpenMode.ForRead) as MText;
                                textDistance.SetDatabaseDefaults();
                                textDistance.SetPropertiesFrom(baseTextDist);
                                textDistance.Contents = ofset.ToString("F2");
                                textDistance.Location = new Point3d(pnt.X, baseTextDist.Location.Y, pnt.Z);
                                textDistance.Rotation = baseTextDist.Rotation;
                                textDistance.TextHeight = baseTextDist.TextHeight;
                                textDistance.Color = baseTextDist.Color;
                                textDistance.TextStyleId = baseTextDist.TextStyleId;
                                textDistance.Attachment = AttachmentPoint.MiddleCenter;
                                textDistance.Layer = baseTextDist.Layer;
                                acBlkTblRec.AppendEntity(textDistance);
                                ts.AddNewlyCreatedDBObject(textDistance, true);
                                break;
                            case "AcDbText":
                                DBText baseTextDist2 = mTextDistRes.ObjectId.GetObject(OpenMode.ForRead) as DBText;
                                textDistance2.SetDatabaseDefaults();
                                textDistance2.SetPropertiesFrom(baseTextDist2);
                                textDistance2.Justify = AttachmentPoint.BaseMid;
                                textDistance2.Height = baseTextDist2.Height;
                                textDistance2.Rotation = baseTextDist2.Rotation;
                                textDistance2.TextString = elevation.ToString("F2");
                                textDistance2.Position = new Point3d(pnt.X, baseTextDist2.Position.Y, pnt.Z);
                                acBlkTblRec.AppendEntity(textDistance2);
                                ts.AddNewlyCreatedDBObject(textDistance2, true);
                                break;
                        }
                        //------------APPENDING-----------------------//
                        acBlkTblRec.AppendEntity(line);
                        ts.AddNewlyCreatedDBObject(line, true);
                        ts.Commit();
                    }
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public void SampleLines()
        {
            Alignment algn = GetAlignment();
            ObjectIdCollection slgIds = algn.GetSampleLineGroupIds();
            using (Transaction ts = Dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    // sample line groups
                    foreach (ObjectId item in slgIds)
                    {
                        SampleLineGroup slg = ts.GetObject(item, OpenMode.ForRead) as SampleLineGroup;

                        //sample lines
                        ObjectIdCollection smpLinesIds = slg.GetSampleLineIds();
                        foreach (ObjectId smpLineId in smpLinesIds)
                        {
                            SampleLine sl = ts.GetObject(smpLineId, OpenMode.ForRead) as SampleLine;
                            // her bir sampleline da örneklenmiş sectionlar döndürülür.
                            ObjectIdCollection sectionIds = sl.GetSectionIds();

                            foreach (ObjectId sectionId in sectionIds)
                            {
                                Autodesk.Civil.DatabaseServices.Section sction = ts.GetObject(sectionId, OpenMode.ForRead) as
                                    Autodesk.Civil.DatabaseServices.Section;

                            }

                        }
                    }

                }
                catch (System.Exception)
                {
                    throw;
                }
            }

        }

        public void ExportGeoJSON()
        {
            //select the objects that want to export as geojson
            List<BaseC3dObject> featureCollection = GetObjects("Export etmek istediğin objeleri seç");

            //defining the variables for coordinate transformation
            string src1 = "+proj=tmerc +lat_0=0 +lon_0=30 +k=1 +x_0=500000 +y_0=0 +ellps=GRS80 +units=m +no_defs";
            DotSpatial.Projections.ProjectionInfo info = ProjectionInfo.FromProj4String(src1);
            DotSpatial.Projections.ProjectionInfo info2 = ProjectionInfo.FromEpsgCode(4326);
            double[] zet = new double[] { 0, 0 };
            StringBuilder vertexCoordinates = new StringBuilder();
            string center = "";
            //seperating the object type of selected objects for specific action
            using (Transaction ts = Dwg.TransactionManager.StartTransaction())
            {
                foreach (BaseC3dObject item in featureCollection)
                {
                    List<double[]> pnts = new List<double[]>();
                    if (item.Name == "LWPOLYLINE")
                    {

                        Autodesk.AutoCAD.DatabaseServices.Polyline lwp = ts.GetObject(item.Id, OpenMode.ForWrite)
                            as Autodesk.AutoCAD.DatabaseServices.Polyline;

                        int vn = lwp.NumberOfVertices;
                        double[] fromPoint = new double[vn * 2];
                        int syc = 0;
                        for (int i = 0; i < vn; i++)
                        {
                            double east = lwp.GetPoint2dAt(i).X;
                            double nort = lwp.GetPoint2dAt(i).Y;
                            fromPoint[syc] = east;
                            fromPoint[syc + 1] = nort;
                            syc += 2;
                        }
                        syc = 0;
                        Reproject.ReprojectPoints(fromPoint, zet, info, info2, 0, vn);

                        for (int i = 0; i < vn; i++)
                        {
                            string str = string.Format("[{0},{1}]", fromPoint[syc + 1], fromPoint[syc]);
                            vertexCoordinates.Append(str);
                            syc += 2;
                            if (i != vn - 1)
                            {
                                vertexCoordinates.Append(",");
                            }
                            center = str;
                        }

                        // polyline kapalı ise son kısmına ilk noktayı tekrar atmamız gerekiyor.
                        if (lwp.Closed)
                        {
                            vertexCoordinates.Append(",");
                            string str = string.Format("[{0},{1}]", fromPoint[1], fromPoint[0]);
                            vertexCoordinates.Append(str);
                        }
                    }
                    string geo = GeoObject("LineString", vertexCoordinates.ToString());

                    //WritingToHTML(geo, center);
                }
            }
        }
        public string GeoObject(string type, string list)
        {

            string geoObjectFormat = @"var myGeoObject = new ymaps.GeoObject({
            geometry: {
                type: ""ARGUMENT1"",
                coordinates: [ARGUMENT2]
            },
            properties:{
                hintContent: ""I'm a geo object"",
                balloonContent: ""You can drag me""
            }
        }, {
            draggable: true,
            strokeColor: ""#FFFF00"",
            strokeWidth: 2
        });";
            StringBuilder str = new StringBuilder(geoObjectFormat);

            str.Replace("ARGUMENT1", type);
            str.Replace("ARGUMENT2", list);
            string geoObject = str.ToString();
            return geoObject;
        }


        public string[] OpenFiles()
        {

            OpenFileDialog ofd = new OpenFileDialog("XML dosyasını seçiniz..", "xml", "xml", "dialog", OpenFileDialog.OpenFileDialogFlags.AllowMultiple);



            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return null;
            }
            Ed.WriteMessage("\nFile selected was \"{0}\".",
                ofd.Filename
            );
            return ofd.GetFilenames();
        }

        public void CreateLayer(string layerName)
        {
            using (Transaction ts = Db.TransactionManager.StartTransaction())
            {
                LayerTable lt = ts.GetObject(Db.LayerTableId, OpenMode.ForWrite) as LayerTable;
                try
                {
                    // Validate the provided symbol table name

                    SymbolUtilityServices.ValidateSymbolName(layerName, false);
                    // Only set the layer name if it isn't in use

                    if (lt.Has(layerName))
                        Ed.WriteMessage(
                          "\nA layer with this name already exists."
                        );
                    else
                    {
                        LayerTableRecord ltr = new LayerTableRecord();
                        ltr.Name = layerName;
                        lt.UpgradeOpen();
                        ObjectId ltId = lt.Add(ltr);
                        ts.AddNewlyCreatedDBObject(ltr, true);
                        ts.Commit();
                    }
                }
                catch
                {
                    // An exception has been thrown, indicating the
                    // name is invalid

                    Ed.WriteMessage(
                      "\nInvalid layer name."
                    );
                }
            }

        }
        public void LandXmlHandling()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            int pntListCount = 0;
            int profileCount = 0;
            int pviCount = 0;
            string[] fileNames = OpenFiles();
            foreach (string fileName in fileNames)
            {
                XmlReader reader = XmlReader.Create(fileName, settings);
                List<string> alignmentCollection = new List<string>();
                List<List<double>> pntListCollection = new List<List<double>>();
                List<double> pviList = new List<double>();
                try
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement() == true)
                        {
                            switch (reader.Name)
                            {

                                case "PntList2D":
                                    pntListCount++;
                                    reader.Read();
                                    Console.WriteLine("{0}.PntList", pntListCount);
                                    Console.WriteLine(reader.Value);
                                    Console.ReadLine();
                                    List<double> pntList = new List<double>();
                                    string[] pntListStr = reader.Value.Split(' ');
                                    for (int i = 0; i < pntListStr.Length; i += 2)
                                    {
                                        pntList.Add(Convert.ToDouble(pntListStr[i + 1]));
                                        pntList.Add(Convert.ToDouble(pntListStr[i]));
                                        Console.WriteLine("{0}", pntList[i]);
                                    }
                                    pntListCollection.Add(pntList);
                                    break;


                                case "ProfAlign":
                                    if (pntListCollection.Count == 0)
                                        break;
                                    profileCount++;
                                    Console.WriteLine("{0}.Profile", profileCount);
                                    Console.WriteLine(reader.Value);
                                    Console.ReadLine();
                                    alignmentCollection.Add(reader.GetAttribute("name"));
                                    break;

                                case "PVI":
                                    if (pntListCollection.Count == 0)
                                        break;
                                    
                                    pviCount++;
                                    reader.Read();
                                    Console.WriteLine(reader.Value);
                                    Console.ReadLine();
                                    string[] pviListStr = reader.Value.Split(' ');
                                    pviList.Add(Convert.ToDouble(pviListStr[1]));
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                //----------------------------------------------------------//
                //---------------MERGING PVI LIST TO THE POINT LIST---------//
                //----------------------------------------------------------//
                int k = 0;
                while (k < pviList.Count)
                {
                    foreach (List<double> item in pntListCollection)
                    {
                        int i = 2;
                        do
                        {
                            item.Insert(i, pviList[k]);
                            i += 3;
                            k++;
                        } while (i <= item.Count);
                    }
                }

                //----------------------------------------------------------//
                //---------------CREEATING LAYERS----------------------------//
                //----------------------------------------------------------//
                foreach (string alignment in alignmentCollection)
                {
                    CreateLayer(alignment);
                }


                //---------------CREATING 3DPOLYLINE-----------//
                using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    acBlkTbl = ts.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = ts.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    int sayac = 0;
                    foreach (List<double> item in pntListCollection)
                    {
                        Polyline3d poly = new Polyline3d();
                        acBlkTblRec.AppendEntity(poly);

                        for (int i = 0; i < item.Count; i += 3)
                        {

                            Point3d pnt = new Point3d(item[i], item[i + 1], item[i + 2]);
                            PolylineVertex3d vertex = new PolylineVertex3d(pnt);
                            poly.AppendVertex(vertex);
                            ts.AddNewlyCreatedDBObject(vertex, true);
                        }
                        poly.Layer = alignmentCollection[sayac];
                        sayac++;
                        ts.AddNewlyCreatedDBObject(poly, true);
                    }

                    ts.Commit();
                }
            }
        }
        
        public void WritingToHTML(string geoObject, string center)
        {
            string html = @"<!DOCTYPE html>
<html>
<head>
    <title>Examples. Polylines</title>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <!-- If you are using the API locally, you must specify the protocol in the standard form (https://...) in the resource URL.-->
    <script src=""https://api-maps.yandex.ru/2.1/?lang=en_US"" type=""text/javascript""></script>
	<style>
        html, body, #map {
            width: 100%; height: 100%; padding: 0; margin: 0;
        }
    </style>
</head>
<body>
<div id=""map""></div>
<script>ymaps.ready(init);

function init() {
    // Creating the map.
    var myMap = new ymaps.Map(""map"", {
            center: CENTER,
            zoom: 18,
            type: ""yandex#satellite""
        }, {
            searchControlProvider: 'yandex#search'
        });

    // Creating a polyline using the GeoObject class.
   GEOOBJECT

    

    // Adding lines to the map.
    myMap.geoObjects
        .add(myGeoObject)
}
</script>
</body>
</html>
";
            StringBuilder htmlBuilder = new StringBuilder(html);
            htmlBuilder.Replace("GEOOBJECT", geoObject);
            htmlBuilder.Replace("CENTER", center);

            using (var writer = new StreamWriter(@"C:\\Users\\Grkm\\Documents\\_GRKM\\test.html"))
            {

                writer.Write(htmlBuilder.ToString());
                writer.Close();
            }
            System.Diagnostics.Process.Start(@"C:\\Users\\Grkm\\Documents\\_GRKM\\test.html");
        }
    }

}


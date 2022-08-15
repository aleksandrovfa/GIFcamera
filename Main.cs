using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using View = Autodesk.Revit.DB.View;
using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;
using Aspose.Imaging.FileFormats.Apng;
using Aspose.Imaging.Sources;
using Aspose.Imaging.FileFormats.Png;
using System.IO;

namespace GIFcamera
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            message = "Выберите камеру!!!";
            Reference selectedCamera = uidoc.Selection.PickObject(ObjectType.Element, message);
            var camera = doc.GetElement(selectedCamera);


            message = "Выберите ось!!!";
            Reference selectedOs = uidoc.Selection.PickObject(ObjectType.Element, message);
            var os = doc.GetElement(selectedOs);


            Line line = (os.Location as LocationCurve).Curve as Line;

            double countRotate = 72;
            double angle = 360 / countRotate * 3.14 / 180;


            //Выбор папки
            string path = "";
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            //dialog. = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //path = dialog.SelectedPath + "\\ExportPIC.jpg";
                path = dialog.SelectedPath + "\\";
            }
            else
            {
                return Result.Succeeded;
            }


            ImageExportOptions imgOptions = new ImageExportOptions
            {
                ZoomType = ZoomFitType.FitToPage,
                PixelSize = 1024,
                FilePath = path,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ImageResolution = ImageResolution.DPI_600,
                ExportRange = ExportRange.SetOfViews,
            };


            Transaction tr = new Transaction(doc, "Поворот");
            tr.Start();

            List<ElementId> views = new List<ElementId>();
            ICollection<BuiltInCategory> categories = new List<BuiltInCategory>();
            categories.Add(BuiltInCategory.OST_Views);
            ElementMulticategoryFilter multiFilter = new ElementMulticategoryFilter(categories);


            while (countRotate > 0)
            {
                List<ElementId> fds = ElementTransformUtils.CopyElement(doc, camera.Id, new XYZ()).ToList();

                List<ElementId> elems = doc.GetElement(fds.First()).GetDependentElements(multiFilter).ToList();
                views.Add(elems.First());
                camera.Location.Rotate(line, angle);

                //views.Add(ElementTransformUtils.CopyElement(doc, camera.Id, new XYZ()).First());
                countRotate--;

            }
            tr.Commit();

            Transaction tr1 = new Transaction(doc, "Экспорт");
            tr1.Start();


           
            imgOptions.SetViewsAndSheets(views);
            doc.ExportImage(imgOptions);

            tr1.RollBack();
            //NG4HW - VH26C - 733KW - K6F98 - J8CK4


            var allFiles = Directory.GetFiles(path);

            const int AnimationDuration = 1000; // 1 s
            const int FrameDuration = 70; // 70 ms
            using (RasterImage sourceImage = (RasterImage)Image.Load(allFiles[0]))
            {
                ApngOptions createOptions = new ApngOptions
                {
                    Source = new FileCreateSource(allFiles[1], false),
                    DefaultFrameTime = (uint)FrameDuration,
                    ColorType = PngColorType.GrayscaleWithAlpha,
                };

                using (ApngImage apngImage = (ApngImage)Image.Create(
                    createOptions,
                    sourceImage.Width,
                    sourceImage.Height))
                {
                    //int numOfFrames = AnimationDuration / FrameDuration;
                    //int numOfFrames2 = numOfFrames / 2;

                    apngImage.RemoveAllFrames();

                    // добавить первый кадр
                    apngImage.AddFrame(sourceImage, FrameDuration);

                    // добавить промежуточные кадры
                    for (int frameIndex = 2; frameIndex < allFiles.Length; frameIndex++)
                    {
                        //    for (int frameIndex = 1; frameIndex < numOfFrames - 1; ++frameIndex)
                        //{
                        RasterImage Image11 = (RasterImage)Image.Load(allFiles[frameIndex]);
                        apngImage.AddFrame(Image11, FrameDuration);
                        //ApngFrame lastFrame = (ApngFrame)apngImage.Pages[apngImage.PageCount - 1];
                        //float gamma = frameIndex >= numOfFrames2 ? numOfFrames - frameIndex - 1 : frameIndex;
                        //lastFrame.AdjustGamma(gamma);
                    }

                    // добавить последний кадр
                    apngImage.AddFrame(sourceImage, FrameDuration);

                    apngImage.Save(path+"elephant.png.gif", new GifOptions());
                }
            }


            return Result.Succeeded;

        }


    }


}

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Application = System.Windows.Forms.Application;
using Font = System.Drawing.Font;



namespace SigPadPrototype
{
   public partial class Form1 : Form
   {
       private Bitmap _sign, _ok, _clear, _please;
       private string _imgFile;

      public Form1()
      {
         InitializeComponent();
      }

      private void cmdStart_Click(object sender, EventArgs e)
      {
         //BMP images to the LCD screen
         _sign = new Bitmap(Application.StartupPath + "\\Sign.bmp");
         _ok = new Bitmap(Application.StartupPath + "\\OK.bmp");
         _clear = new Bitmap(Application.StartupPath + "\\CLEAR.bmp");
         _please = new Bitmap(Application.StartupPath + "\\please.bmp");

        //Determines port to use for signature pad
          string[] ports = SerialPort.GetPortNames();
          for (int i = 1; i < ports.Length; i++)
          {
              if (sigPlusNET1.IsAccessible)
              {
                 sigPlusNET1.SetTabletComPort(i);
                 break;
              }
          }

         sigPlusNET1.SetTabletState(1); //Turns tablet on
         sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
         sigPlusNET1.SetTranslateBitmapEnable(false);

         //Images sent to the background
         sigPlusNET1.LCDSendGraphic(1, 2, 0, 20, _sign);
         sigPlusNET1.LCDSendGraphic(1, 2, 207, 4, _ok);
         sigPlusNET1.LCDSendGraphic(1, 2, 15, 4, _clear);

         //Get LCD size in pixels.
         sigPlusNET1.LCDGetLCDSize();

         new Font("Arial", 9.0F, FontStyle.Regular);

         sigPlusNET1.ClearTablet();

         sigPlusNET1.LCDSetWindow(0, 0, 1, 1);
         sigPlusNET1.SetSigWindow(1, 0, 0, 1, 1); //Sets the signing area
         sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds


         //Shows signature window with clear and ok buttons

         sigPlusNET1.ClearSigWindow(1);
         sigPlusNET1.LCDRefresh(1, 16, 45, 50, 15); //Refresh LCD to indicate that the option has been sucessfully chosen
         sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64); //Brings background image into foreground
         sigPlusNET1.ClearTablet();
         sigPlusNET1.KeyPadClearHotSpotList();
         sigPlusNET1.KeyPadAddHotSpot(2, 1, 10, 5, 53, 17); //CLEAR button
         sigPlusNET1.KeyPadAddHotSpot(3, 1, 197, 5, 19, 17); //OK button
         sigPlusNET1.LCDSetWindow(2, 22, 236, 40);
         sigPlusNET1.SetSigWindow(1, 0, 22, 240, 40); //Sets the signing area
      }
      private void sigPlusNET1_PenDown(object sender, EventArgs e)
      {
      }
      private void sigPlusNET1_PenUp(object sender, EventArgs e)
      {
          sigPlusNET1.SetLCDCaptureMode(2);

         if (sigPlusNET1.KeyPadQueryHotSpot(2) > 0) //If CLEAR hotspot is tapped...
         {
            sigPlusNET1.ClearSigWindow(1);
            sigPlusNET1.LCDRefresh(1, 10, 0, 53, 17); //Refresh LCD
            sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64); //Brings background image into foreground
            sigPlusNET1.ClearTablet();
         }
         else if (sigPlusNET1.KeyPadQueryHotSpot(3) > 0) //If OK hotspot is tapped...
         {
            sigPlusNET1.ClearSigWindow(1);

            sigPlusNET1.GetSigString();
            sigPlusNET1.LCDRefresh(1, 210, 3, 14, 14); //Refresh LCD

            if (sigPlusNET1.NumberOfTabletPoints() > 0)
            {
               sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
               var f = new Font("Arial", 9.0F, FontStyle.Regular);

               sigPlusNET1.SetImageXSize(500);
               sigPlusNET1.SetImageYSize(100);
               var myImg = sigPlusNET1.GetSigImage();
               myImg.Save(Application.StartupPath + "\\test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    var openFileDialog1 = new OpenFileDialog()
                {
                    Filter = @"PDF File|*.pdf",
                    Title = @"Open PDF File"
                };
                openFileDialog1.ShowDialog();

                    PdfRectangle rect = new PdfRectangle(0, 0, 0, 0);
                    RectangleF rectF = new RectangleF();
                string path = Application.StartupPath + "\\test.pdf";

                    if (openFileDialog1.FileName != "")
                {
                    PdfReader reader = new PdfReader(openFileDialog1.FileName);
                    PdfStamper stamper = new PdfStamper(reader, new System.IO.FileStream(path, System.IO.FileMode.Create));
                    PdfContentByte underContent = stamper.GetUnderContent(1);

                    AcroFields form = stamper.AcroFields;
                    float[] fieldPositions = reader.AcroFields.GetFieldPositions("Digital Signature");
                    Console.WriteLine("FIELD POSITION : " + fieldPositions);
                    if (fieldPositions == null || ((ICollection) fieldPositions).Count <= 0) throw new ApplicationException("Error locating field");

                        if (fieldPositions != null)
                        {
                            //add rectangle - this is where the signature will be
                        }

                        try
                        {
                        // Add signature image to the document

                        sigPlusNET1.SetJustifyY(20);
                        sigPlusNET1.SetJustifyX(20);
                        sigPlusNET1.SetImageFileFormat(0); //0=bmp, 4=jpg, 6=tif

                        int minX, maxX, minY, maxY, aX, aY, ratio, aIndex, bIndex, fixedY;

                        minX = sigPlusNET1.GetPointXValue(0, 1);
                        maxX = sigPlusNET1.GetPointXValue(0, 1);
                        minY = sigPlusNET1.GetPointYValue(0, 1);
                        maxY = sigPlusNET1.GetPointYValue(0, 1);

                        for (aIndex = 0; aIndex < sigPlusNET1.GetNumberOfStrokes(); aIndex++)
                        {
                            for (bIndex = 0; bIndex < sigPlusNET1.GetNumPointsForStroke(aIndex); bIndex++)
                            {
                                aX = sigPlusNET1.GetPointXValue(aIndex, bIndex);
                                aY = sigPlusNET1.GetPointYValue(aIndex, bIndex);

                                if (aX < minX)
                                {
                                    minX = aX;
                                }

                                if (aX > maxX)
                                {
                                    maxX = aX;
                                }

                                if (aY < minY)
                                {
                                    minY = aY;
                                }

                                if (aY > maxY)
                                {
                                    maxY = aY;
                                }

                            }
                        }

                        ratio = ((maxX - minX) / (maxY - minY));
                        fixedY = 200;
                        sigPlusNET1.SetImagePenWidth((int) (fixedY * 0.5));
                        sigPlusNET1.SetJustifyMode(5);

                        sigPlusNET1.SetImageXSize((int) ((ratio * fixedY) * 1.5));
                        sigPlusNET1.SetImageYSize((int) (fixedY * 1.5));
                        sigPlusNET1.SetAntiAliasLineScale(0.4f);
                        sigPlusNET1.SetAntiAliasSpotSize(0.25f);

                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(myImg, System.Drawing.Imaging.ImageFormat.Bmp);

                        image.Transparency = new int[] {255, 255};
                        image.SetAbsolutePosition(fieldPositions[0],fieldPositions[1]);
                        image.ScalePercent(60);

                        underContent.AddImage(image);

                        stamper.Close();
                        reader.Close();
                    }
                    catch (DocumentException de)
                    {
                            Console.WriteLine(de.Message);
                        }
                    catch (IOException ioe)
                    {
                            Console.WriteLine(ioe.Message);
                        }

                    }
          #region
                    //     //Opens save file dialog
                    //     var saveFileDialog1 =
                    //     new SaveFileDialog
                    //     {
                    //         Filter = @"Bitmap Image|*.bmp|GIF Image|*.Gif|JPeg Image|*.jpg|PNG Image|*.Png",
                    //         Title = @"Save Signature Image"
                    //     };
                    // saveFileDialog1.ShowDialog();

                    //if (saveFileDialog1.FileName != "")
                    //{
                    //   var fs = (FileStream)saveFileDialog1.OpenFile();

                    //    if (saveFileDialog1.FilterIndex == 1)
                    //    {
                    //        myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                    //        _imgFile = saveFileDialog1.FileName;
                    //    }
                    //    else if (saveFileDialog1.FilterIndex == 2)
                    //    {
                    //        myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Gif);
                    //        _imgFile = saveFileDialog1.FileName;
                    //    }
                    //    else if (saveFileDialog1.FilterIndex == 3)
                    //    {
                    //        myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //        _imgFile = saveFileDialog1.FileName;
                    //    }
                    //    else if (saveFileDialog1.FilterIndex == 4)
                    //    {
                    //        myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                    //        _imgFile = saveFileDialog1.FileName;
                    //    }
                    //    fs.Close();
                    //}
#endregion
               WindowState = FormWindowState.Minimized;
               Process.Start(Application.StartupPath + "\\test.pdf");
               sigPlusNET1.LCDWriteString(0, 2, 35, 25, f, "Signature capture complete.");
               Thread.Sleep(1000);
               Application.Exit();
            }
            else
            {
               sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
               sigPlusNET1.LCDSendGraphic(0, 2, 4, 20, _please);
               sigPlusNET1.ClearTablet();
               sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64);
               sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds
            }
         }
         sigPlusNET1.ClearSigWindow(1);
      }
      private void Form1_FormClosing(object sender, FormClosingEventArgs e)
      {
         //reset LCD
         sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
         sigPlusNET1.LCDSetWindow(0, 0, 240, 64);
         sigPlusNET1.SetSigWindow(1, 0, 0, 240, 64);
         sigPlusNET1.KeyPadClearHotSpotList();

         var blank = new Bitmap(240, 64);
         sigPlusNET1.LCDSendGraphic(1, 0, 0, 0, blank);

         sigPlusNET1.SetLCDCaptureMode(1);
         sigPlusNET1.SetTabletState(0);
      }

   }
}

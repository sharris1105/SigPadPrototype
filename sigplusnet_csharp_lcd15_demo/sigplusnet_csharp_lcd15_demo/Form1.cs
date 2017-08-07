using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Application = System.Windows.Forms.Application;


namespace sigplusnet_csharp_lcd15_demo
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


               //Opens save file dialog
                var saveFileDialog1 =
                    new SaveFileDialog
                    {
                        Filter = @"Bitmap Image|*.bmp|GIF Image|*.Gif|JPeg Image|*.jpg|PNG Image|*.Png",
                        Title = @"Save Signature Image"
                    };
                saveFileDialog1.ShowDialog();

               if (saveFileDialog1.FileName != "")
               {
                  var fs = (FileStream)saveFileDialog1.OpenFile();

                   if (saveFileDialog1.FilterIndex == 1)
                   {
                       myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                       _imgFile = saveFileDialog1.FileName;
                   }
                   else if (saveFileDialog1.FilterIndex == 2)
                   {
                       myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Gif);
                       _imgFile = saveFileDialog1.FileName;
                   }
                   else if (saveFileDialog1.FilterIndex == 3)
                   {
                       myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                       _imgFile = saveFileDialog1.FileName;
                   }
                   else if (saveFileDialog1.FilterIndex == 4)
                   {
                       myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                       _imgFile = saveFileDialog1.FileName;
                   }
                   fs.Close();
               }
               WindowState = FormWindowState.Minimized;
               Process.Start(_imgFile);
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

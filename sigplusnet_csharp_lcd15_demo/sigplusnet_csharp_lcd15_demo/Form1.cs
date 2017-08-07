using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using System.Linq;
using Application = System.Windows.Forms.Application;


namespace sigplusnet_csharp_lcd15_demo
{
   public partial class Form1 : Form
   {
      Bitmap sign, ok, clear, please;
      int lcdX, lcdY;
      uint lcdSize;
      string imgFile;

      public Form1()
      {
         InitializeComponent();
      }

      private void cmdStart_Click(object sender, EventArgs e)
      {
         //The following code will write BMP images out to the LCD 1X5 screen

         sign = new System.Drawing.Bitmap(Application.StartupPath + "\\Sign.bmp");
         ok = new System.Drawing.Bitmap(Application.StartupPath + "\\OK.bmp");
         clear = new System.Drawing.Bitmap(Application.StartupPath + "\\CLEAR.bmp");
         please = new System.Drawing.Bitmap(Application.StartupPath + "\\please.bmp");

         sigPlusNET1.SetTabletComPort(3);
         sigPlusNET1.SetTabletState(1); //Turns tablet on to collect signature
         sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
         sigPlusNET1.SetTranslateBitmapEnable(false);

         //Images sent to the background
         sigPlusNET1.LCDSendGraphic(1, 2, 0, 20, sign);
         sigPlusNET1.LCDSendGraphic(1, 2, 207, 4, ok);
         sigPlusNET1.LCDSendGraphic(1, 2, 15, 4, clear);

         //Get LCD size in pixels.
         lcdSize = sigPlusNET1.LCDGetLCDSize();
         lcdX = (int)(lcdSize & 0xFFFF);
         lcdY = (int)((lcdSize >> 16) & 0xFFFF);

         Font f = new System.Drawing.Font("Arial", 9.0F, System.Drawing.FontStyle.Regular);

         sigPlusNET1.ClearTablet();

         sigPlusNET1.LCDSetWindow(0, 0, 1, 1);
         sigPlusNET1.SetSigWindow(1, 0, 0, 1, 1); //Sets the area where ink is permitted in the SigPlus object
         sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds


         //Shows signature window with clear and ok buttons

         sigPlusNET1.ClearSigWindow(1);
         sigPlusNET1.LCDRefresh(1, 16, 45, 50, 15); //Refresh LCD at 'Continue' to indicate to user that this option has been sucessfully chosen
         sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64); //Brings the background image already loaded into foreground
         sigPlusNET1.ClearTablet();
         sigPlusNET1.KeyPadClearHotSpotList();
         sigPlusNET1.KeyPadAddHotSpot(2, 1, 10, 5, 53, 17); //For CLEAR button
         sigPlusNET1.KeyPadAddHotSpot(3, 1, 197, 5, 19, 17); //For OK button
         sigPlusNET1.LCDSetWindow(2, 22, 236, 40);
         sigPlusNET1.SetSigWindow(1, 0, 22, 240, 40); //Sets the area where ink is permitted in the SigPlus object
      }
      private void sigPlusNET1_PenDown(object sender, EventArgs e)
      {
      }
      private void sigPlusNET1_PenUp(object sender, EventArgs e)
      {
         string strSig;
         sigPlusNET1.SetLCDCaptureMode(2);

         if (sigPlusNET1.KeyPadQueryHotSpot(2) > 0) //If the CLEAR hotspot is tapped, then...
         {
            sigPlusNET1.ClearSigWindow(1);
            sigPlusNET1.LCDRefresh(1, 10, 0, 53, 17); //Refresh LCD at 'CLEAR' to indicate to user that this option has been sucessfully chosen
            sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64); //Brings the background image already loaded into foreground
            sigPlusNET1.ClearTablet();
         }
         else if (sigPlusNET1.KeyPadQueryHotSpot(3) > 0) //If the OK hotspot is tapped, then...
         {
            sigPlusNET1.ClearSigWindow(1);

            strSig = sigPlusNET1.GetSigString();
            sigPlusNET1.LCDRefresh(1, 210, 3, 14, 14); //Refresh LCD at 'OK' to indicate to user that this option has been sucessfully chosen

            if (sigPlusNET1.NumberOfTabletPoints() > 0)
            {
               sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
               Font f = new System.Drawing.Font("Arial", 9.0F, System.Drawing.FontStyle.Regular);

               sigPlusNET1.SetImageXSize(500);
               sigPlusNET1.SetImageYSize(100);
               System.Drawing.Image myImg = sigPlusNET1.GetSigImage();


               //Opens save file dialog
               SaveFileDialog saveFileDialog1 = new SaveFileDialog();
               saveFileDialog1.Filter = "Bitmap Image|*.bmp|GIF Image|*.Gif|JPeg Image|*.jpg|PNG Image|*.Png";
               saveFileDialog1.Title = "Save Signature Image";
               saveFileDialog1.ShowDialog();

               if (saveFileDialog1.FileName != "")
               {
                  System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();

                  switch (saveFileDialog1.FilterIndex)
                  {
                     case 1:
                        myImg.Save(fs,System.Drawing.Imaging.ImageFormat.Bmp);
                        imgFile = saveFileDialog1.FileName;
                        break;

                     case 2:
                        myImg.Save(fs,System.Drawing.Imaging.ImageFormat.Gif);
                        imgFile = saveFileDialog1.FileName; 
                        break;

                     case 3:
                        myImg.Save(fs,System.Drawing.Imaging.ImageFormat.Jpeg);
                        imgFile = saveFileDialog1.FileName; 
                        break;

                     case 4:
                        myImg.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        imgFile = saveFileDialog1.FileName;
                        break;
                  }
                  fs.Close();
               }
               this.WindowState = FormWindowState.Minimized;
               Process.Start(imgFile);
               sigPlusNET1.LCDWriteString(0, 2, 35, 25, f, "Signature capture complete.");
               Thread.Sleep(1000);
               Application.Exit();
            }
            else
            {
               sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
               sigPlusNET1.LCDSendGraphic(0, 2, 4, 20, please);
               sigPlusNET1.ClearTablet();
               sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64);
               sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds
            }
         }
         sigPlusNET1.ClearSigWindow(1);
      }
      private void Form1_FormClosing(object sender, FormClosingEventArgs e)
      {
         //reset hardware
         sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
         sigPlusNET1.LCDSetWindow(0, 0, 240, 64);
         sigPlusNET1.SetSigWindow(1, 0, 0, 240, 64);
         sigPlusNET1.KeyPadClearHotSpotList();

         Bitmap blank = new System.Drawing.Bitmap(240, 64);
         sigPlusNET1.LCDSendGraphic(1, 0, 0, 0, blank);

         sigPlusNET1.SetLCDCaptureMode(1);
         sigPlusNET1.SetTabletState(0);
      }
   }
}

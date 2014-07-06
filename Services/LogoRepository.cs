using DVBViewerServer;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace DVBViewerController.Services
{
    public class LogoRepository
    {
        public MemoryStream GetImage(string channelName)
        {
            string fileName = GetFileName(channelName);

            if (!fileName.Equals(""))
            {
                MemoryStream mStream = new MemoryStream();
                Bitmap image = ResizeToLongSide(Image.FromFile(fileName), 200);
                image.Save(mStream, ImageFormat.Png);
                mStream.Position = 0;

                return mStream;
            }
            else
            {
                return null;
            }
        }

        private string GetFileName(string channelName)
        {
            DVBViewer dvb;
            string filename = "";
            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");


                IDataManager data = dvb.DataManager;

                string search = channelName;
                search = search.ToLower();

                string pattern = "\\s\\(.+\\)";
                Regex rgx = new Regex(pattern);
                search = rgx.Replace(search, "");

                pattern = "\\.";
                rgx = new Regex(pattern);
                search = rgx.Replace(search, " ");

                string appfolder = data.get_Value("#appfolder") + "Images\\Logos\\";

                String[] logoFiles = Directory.GetFiles(appfolder, search + "*.png", SearchOption.AllDirectories);
                if (logoFiles.Length != 0)
                {
                    filename = logoFiles[0];
                }

            }
            catch (Exception ex)
            {
            }

            return filename;
        }

        private Bitmap ResizeToLongSide(Image src, int size)
        {
            Bitmap dst;

            int oldWidth = src.Size.Width;
            int oldHeight = src.Size.Height;

            if (oldWidth == oldHeight)
                dst = new Bitmap(size, size);
            else if (oldWidth > oldHeight)
            {
                dst = new Bitmap(size, (size * oldHeight / oldWidth));
            }
            else
            {
                dst = new Bitmap((size * oldWidth / oldHeight), size);
            }

            using (Graphics g = Graphics.FromImage(dst))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //MessageBox.Show(dst.Width.ToString() + "x" + dst.Height.ToString());
                g.DrawImage(src, 0, 0, dst.Width, dst.Height);
            }

            return dst;
        }
    }
}

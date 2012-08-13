using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Accessor;

namespace Engine
{
    public class ThumbnailEngine
    {

        public string TestMe()
        {
            return "success";
        }
        public Bitmap CreateThumbnail(Stream input, int displayWidth, int displayHeight)
        {
            int width = displayWidth;
            int height = displayHeight;

            decimal Dwidth = Decimal.Parse(width.ToString());
            decimal Dheight = Decimal.Parse(height.ToString());

            decimal aspectRatio = Dwidth / Dheight;

            try
            {
                input.Seek(0, SeekOrigin.Begin);
                var originalImage = new Bitmap(input);
                if (originalImage.Width > displayWidth || originalImage.Height > displayHeight)
                {
                    if (originalImage.Width / aspectRatio > originalImage.Height)
                    {
                        height = (int)Math.Ceiling(Decimal.Parse(originalImage.Height.ToString()) * Decimal.Parse(displayWidth.ToString()) / Decimal.Parse(originalImage.Width.ToString()));
                    }
                    else
                    {
                        width = (int)Math.Ceiling(Decimal.Parse(originalImage.Width.ToString()) * Decimal.Parse(displayHeight.ToString()) / Decimal.Parse(originalImage.Height.ToString()));
                    }
                    var thumbnailImage = new Bitmap(width, height);

                    using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }
                    return thumbnailImage;
                }
                return originalImage;
            }
            catch (Exception e)
            {
                LogAccessor logAccessor = new LogAccessor();
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
        }

        public Bitmap CreateProfileThumbnail(Stream input, int displayWidth, int displayHeight)
        {
            int width = displayWidth;
            int height = displayHeight;
            decimal Dwidth = Decimal.Parse(width.ToString());
            decimal Dheight = Decimal.Parse(height.ToString());
            decimal aspectRatio = Dwidth / Dheight;

            try
            {
                input.Seek(0, SeekOrigin.Begin);
                var originalImage = new Bitmap(input);
                decimal originalAspect = (Decimal.Parse(originalImage.Width.ToString()) / Decimal.Parse(originalImage.Height.ToString()));
                if (originalImage.Width < width || originalImage.Height < height)
                {
                    //picture does not meet size requirements
                }
                if (originalImage.Width < originalImage.Height)
                {
                    //width = displayWidth
                    height = (int)Math.Ceiling(width / originalAspect);

                    var thumbnailImage = new Bitmap(width, height);
                    using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }
                    int xCoordinate = 0;
                    int yCoordinate = ((height - displayHeight) / 2);
                    Rectangle rect = new Rectangle(xCoordinate, yCoordinate, displayWidth, displayHeight);
                    Bitmap bmpCrop = thumbnailImage.Clone(rect, thumbnailImage.PixelFormat);
                    return bmpCrop;
                }
                else if (originalImage.Height < originalImage.Width)
                {
                    //height = displayHeight
                    width = (int)Math.Ceiling(originalAspect * Dheight);

                    var thumbnailImage = new Bitmap(width, height);
                    using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }
                    int yCoordinate = 0;
                    int xCoordingate = ((width - displayWidth) / 2);
                    Rectangle rect = new Rectangle(xCoordingate, yCoordinate, displayWidth, displayHeight);
                    Bitmap bmpCrop = thumbnailImage.Clone(rect, thumbnailImage.PixelFormat);
                    return bmpCrop;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogAccessor logAccessor = new LogAccessor();
                logAccessor.CreateLog(DateTime.Now, "Thumbnail Engine - CreateProfileThumbnail", ex.StackTrace);
                return null;
            }

        }
    }
}

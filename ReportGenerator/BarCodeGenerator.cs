using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;
using ReportGenerator.FunDbApi;
using Sandwych.Reporting;

namespace ReportGenerator
{
    public class BarCodeGenerator
    {
        public ImageBlob Generate(BarCodeType codeType, string text)
        {
            var image = codeType switch
            {
                BarCodeType.QrCode => GenerateQrCode(text),
                BarCodeType.BarCode => GenerateBarCode(text),
                _ => throw new Exception("Unknown code type"),
            };
            var bytes = ImageToByteArray(image);
            var blob = new ImageBlob("bmp", bytes);
            return blob;
        }

        private byte[] ImageToByteArray(Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        private Image GenerateBarCode(string text)
        {
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            var image = b.Encode(BarcodeLib.TYPE.CODE11, text, Color.Black, Color.White, 290, 120);
            return image;
        }

        private Image GenerateQrCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var image = qrCode.GetGraphic(20);
            return image;
        }
    }
}

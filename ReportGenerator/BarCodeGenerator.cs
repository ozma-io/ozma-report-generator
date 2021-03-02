using System;
using System.Drawing;
using QRCoder;
using ReportGenerator.FunDbApi;

namespace ReportGenerator
{
    public class BarCodeGenerator
    {
        public Image Generate(BarCodeType codeType, string text)
        {
            var image = codeType switch
            {
                BarCodeType.QrCode => GenerateQrCode(text),
                BarCodeType.Itf14 => GenerateBarCode(BarcodeLib.TYPE.ITF14, text),
                BarCodeType.Ean13 => GenerateBarCode(BarcodeLib.TYPE.EAN13, text),
                BarCodeType.Code39 => GenerateBarCode(BarcodeLib.TYPE.CODE39, text),
                _ => throw new Exception("Unknown code type"),
            };
            return image;
        }
        
        private Image GenerateBarCode(BarcodeLib.TYPE type, string text)
        {
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            var image = b.Encode(type, text, Color.Black, Color.White, 290, 120);
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

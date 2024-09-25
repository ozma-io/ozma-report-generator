using System;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.SkiaSharp;
using ReportGenerator.OzmaDBApi;

namespace ReportGenerator
{
    public class BarCodeGenerator
    {
        public SKImage Generate(BarCodeType codeType, string text)
        {
            var image = codeType switch
            {
                BarCodeType.QrCode => GenerateQrCode(text),
                BarCodeType.Itf14 => GenerateBarCode(BarcodeFormat.ITF, text),
                BarCodeType.Ean13 => GenerateBarCode(BarcodeFormat.EAN_13, text),
                BarCodeType.Code39 => GenerateBarCode(BarcodeFormat.CODE_39, text),
                _ => throw new Exception("Unknown code type"),
            };
            return image;
        }

        private SKImage GenerateBarCode(BarcodeFormat type, string text)
        {
            var options = new EncodingOptions()
            {
                Height = 120,
                Width = 290,
                PureBarcode = true,
            };

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };
            return SKImage.FromBitmap(writer.Write(text));
        }

        private SKImage GenerateQrCode(string text)
        {
            var options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
            };

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };
            return SKImage.FromBitmap(writer.Write(text));
        }
    }
}

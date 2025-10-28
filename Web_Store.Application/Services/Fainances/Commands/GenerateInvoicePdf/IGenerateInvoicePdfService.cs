using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;
using Web_store.Common.Dto;
using Microsoft.AspNetCore.Hosting;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayDetail;

namespace Web_Store.Application.Services.Fainances.Commands.GenerateInvoicePdf
{
    public interface IGenerateInvoicePdfService
    {
        ResultDto<byte[]> Execute(long requestPayId);
    }

    public class GenerateInvoicePdfService : IGenerateInvoicePdfService
    {
        private readonly IGetRequestPayDetailService _getRequestPayDetailService;
        private readonly IWebHostEnvironment _environment;

        public GenerateInvoicePdfService(
            IGetRequestPayDetailService getRequestPayDetailService,
            IWebHostEnvironment environment)
        {
            _getRequestPayDetailService = getRequestPayDetailService;
            _environment = environment;
        }

        public ResultDto<byte[]> Execute(long requestPayId)
        {
            try
            {
                var result = _getRequestPayDetailService.Execute(requestPayId);
                if (!result.IsSuccess)
                    return new ResultDto<byte[]> { IsSuccess = false, Message = result.Message };

                var invoiceData = result.Data;

                using (var memoryStream = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 30, 30, 30, 30);
                    var writer = PdfWriter.GetInstance(document, memoryStream);

                    document.Open();

                    // ایجاد فونت
                    BaseFont baseFont = CreatePersianFont();
                    var titleFont = new Font(baseFont, 18, Font.BOLD, new BaseColor(0, 91, 150));
                    var headerFont = new Font(baseFont, 14, Font.BOLD, BaseColor.BLACK);
                    var normalFont = new Font(baseFont, 11, Font.NORMAL, BaseColor.DARK_GRAY);
                    var smallFont = new Font(baseFont, 10, Font.NORMAL, BaseColor.GRAY);

                    // اضافه کردن محتوا با متن‌های معکوس شده
                    AddInvoiceHeader(document, invoiceData, titleFont, normalFont);
                    AddCustomerInfo(document, invoiceData, headerFont, normalFont);
                    AddProductsTable(document, invoiceData, headerFont, normalFont, smallFont);
                    AddPaymentSummary(document, invoiceData, headerFont, normalFont);

                    document.Close();

                    return new ResultDto<byte[]>
                    {
                        IsSuccess = true,
                        Data = memoryStream.ToArray()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResultDto<byte[]>
                {
                    IsSuccess = false,
                    Message = "خطا در تولید فاکتور: " + ex.Message
                };
            }
        }

        private BaseFont CreatePersianFont()
        {
            var fontPath = Path.Combine(_environment.WebRootPath, "Fonts", "BNAZANIN.ttf");

            if (File.Exists(fontPath))
            {
                return BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            else
            {
                // استفاده از فونت پیشفرض
                return BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.EMBEDDED);
            }
        }

        // متد برای معکوس کردن متن فارسی
        private string ReversePersianText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // بررسی آیا متن حاوی کاراکتر فارسی است
            bool hasPersian = text.Any(c => c >= 0x0600 && c <= 0x06FF);

            if (hasPersian)
            {
                char[] charArray = text.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            return text;
        }

        private void AddInvoiceHeader(Document document, RequestPayDetailDto invoiceData, Font titleFont, Font normalFont)
        {
            // هدر اصلی
            var title = new Paragraph(ReversePersianText("فاکتور فروشگاه باگتو"), titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            // اطلاعات فاکتور
            var infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 1, 1 });

            // سمت راست: اطلاعات فاکتور
            var rightCell = new PdfPCell();
            rightCell.Border = Rectangle.NO_BORDER;
            rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;

            var rightPhrase = new Phrase();
            rightPhrase.Add(new Chunk(ReversePersianText("شماره فاکتور: ") + invoiceData.Id + "\n", normalFont));
            rightPhrase.Add(new Chunk(ReversePersianText("تاریخ صدور: ") + invoiceData.InsertTime.ToString("yyyy/MM/dd HH:mm") + "\n", normalFont));
            rightPhrase.Add(new Chunk(ReversePersianText("وضعیت: ") + (invoiceData.IsPay ? ReversePersianText("پرداخت شده") : ReversePersianText("در انتظار پرداخت")), normalFont));

            rightCell.AddElement(rightPhrase);
            infoTable.AddCell(rightCell);

            // سمت چپ: اطلاعات شرکت
            var leftCell = new PdfPCell();
            leftCell.Border = Rectangle.NO_BORDER;
            leftCell.HorizontalAlignment = Element.ALIGN_LEFT;

            var leftPhrase = new Phrase();
            leftPhrase.Add(new Chunk(ReversePersianText("فروشگاه باگتو") + "\n", normalFont));
            leftPhrase.Add(new Chunk(ReversePersianText("تلفن: 021-12345678") + "\n", normalFont));
            leftPhrase.Add(new Chunk(ReversePersianText("آدرس: تهران، خیابان نمونه"), normalFont));

            leftCell.AddElement(leftPhrase);
            infoTable.AddCell(leftCell);

            document.Add(infoTable);

            // خط جداکننده
            document.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, 0f)));
            document.Add(new Paragraph(" "));
        }

        private void AddCustomerInfo(Document document, RequestPayDetailDto invoiceData, Font headerFont, Font normalFont)
        {
            var customerTitle = new Paragraph(ReversePersianText("اطلاعات مشتری"), headerFont);
            customerTitle.SpacingBefore = 15f;
            customerTitle.SpacingAfter = 10f;
            document.Add(customerTitle);

            var customerTable = new PdfPTable(2);
            customerTable.WidthPercentage = 100;
            customerTable.SetWidths(new float[] { 1, 2 });

            AddInfoRow(customerTable, ReversePersianText("نام کامل:"), invoiceData.UserName, normalFont);
            AddInfoRow(customerTable, ReversePersianText("ایمیل:"), invoiceData.UserEmail, normalFont);
            AddInfoRow(customerTable, ReversePersianText("کد کاربر:"), invoiceData.UserId.ToString(), normalFont);

            document.Add(customerTable);
            document.Add(new Paragraph(" "));
        }

        private void AddInfoRow(PdfPTable table, string label, string value, Font font)
        {
            // سلول برچسب
            var labelCell = new PdfPCell(new Phrase(label, font));
            labelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.Padding = 8f;
            labelCell.BackgroundColor = new BaseColor(248, 249, 250);
            table.AddCell(labelCell);

            // سلول مقدار
            var valueCell = new PdfPCell(new Phrase(value, font));
            valueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.Padding = 8f;
            table.AddCell(valueCell);
        }

        private void AddProductsTable(Document document, RequestPayDetailDto invoiceData, Font headerFont, Font normalFont, Font smallFont)
        {
            var productsTitle = new Paragraph(ReversePersianText("لیست محصولات"), headerFont);
            productsTitle.SpacingBefore = 20f;
            productsTitle.SpacingAfter = 10f;
            document.Add(productsTitle);

            foreach (var order in invoiceData.Orders)
            {
                var orderHeader = new Paragraph(
                    ReversePersianText($"سفارش شماره: {order.OrderId} - وضعیت: {order.OrderState}"),
                    normalFont
                );
                orderHeader.SpacingBefore = 15f;
                orderHeader.SpacingAfter = 10f;
                document.Add(orderHeader);

                var productsTable = new PdfPTable(4);
                productsTable.WidthPercentage = 100;
                productsTable.SetWidths(new float[] { 4, 2, 2, 2 });

                // هدر جدول
                AddTableHeader(productsTable, ReversePersianText("نام محصول"), headerFont);
                AddTableHeader(productsTable, ReversePersianText("قیمت واحد"), headerFont);
                AddTableHeader(productsTable, ReversePersianText("تعداد"), headerFont);
                AddTableHeader(productsTable, ReversePersianText("جمع"), headerFont);

                // ردیف‌های محصولات
                foreach (var item in order.OrderItems)
                {
                    AddTableRow(productsTable, ReversePersianText(item.ProductName), smallFont);
                    AddTableRow(productsTable, item.Price.ToString("N0"), smallFont);
                    AddTableRow(productsTable, item.Count.ToString(), smallFont);
                    AddTableRow(productsTable, item.TotalPrice.ToString("N0"), smallFont);
                }

                // جمع سفارش
                var orderTotal = order.OrderItems.Sum(oi => oi.TotalPrice);
                AddTableTotalRow(productsTable,
                    ReversePersianText($"جمع سفارش {order.OrderId}:"),
                    orderTotal.ToString("N0") + " " + ReversePersianText("تومان"),
                    headerFont
                );

                document.Add(productsTable);
                document.Add(new Paragraph(" "));
            }
        }

        private void AddTableHeader(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.BackgroundColor = new BaseColor(0, 91, 150);
            cell.BorderColor = BaseColor.WHITE;
            cell.Padding = 10f;
            cell.BorderWidth = 1f;
            table.AddCell(cell);
        }

        private void AddTableRow(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 8f;
            cell.BorderColor = new BaseColor(220, 220, 220);
            cell.BorderWidth = 0.5f;
            table.AddCell(cell);
        }

        private void AddTableTotalRow(PdfPTable table, string label, string value, Font font)
        {
            // سلول برچسب
            var labelCell = new PdfPCell(new Phrase(label, font));
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
            labelCell.Colspan = 3;
            labelCell.BackgroundColor = new BaseColor(240, 240, 240);
            labelCell.Padding = 10f;
            labelCell.BorderColor = new BaseColor(220, 220, 220);
            labelCell.BorderWidth = 0.5f;
            table.AddCell(labelCell);

            // سلول مقدار
            var valueCell = new PdfPCell(new Phrase(value, font));
            valueCell.HorizontalAlignment = Element.ALIGN_CENTER;
            valueCell.BackgroundColor = new BaseColor(240, 240, 240);
            valueCell.Padding = 10f;
            valueCell.BorderColor = new BaseColor(220, 220, 220);
            valueCell.BorderWidth = 0.5f;
            table.AddCell(valueCell);
        }

        private void AddPaymentSummary(Document document, RequestPayDetailDto invoiceData, Font headerFont, Font normalFont)
        {
            var summaryTable = new PdfPTable(2);
            summaryTable.WidthPercentage = 50;
            summaryTable.HorizontalAlignment = Element.ALIGN_LEFT;

            // جمع کل
            AddSummaryRow(summaryTable,
                ReversePersianText("جمع کل فاکتور:"),
                invoiceData.Amount.ToString("N0") + " " + ReversePersianText("تومان"),
                headerFont
            );

            if (invoiceData.IsPay)
            {
                AddSummaryRow(summaryTable,
                    ReversePersianText("تاریخ پرداخت:"),
                    invoiceData.PayDate?.ToString("yyyy/MM/dd HH:mm"),
                    normalFont
                );
                AddSummaryRow(summaryTable,
                    ReversePersianText("کد پیگیری:"),
                    invoiceData.RefId.ToString(),
                    normalFont
                );
            }

            document.Add(summaryTable);

            // پیام پایانی
            var footer = new Paragraph(ReversePersianText("با تشکر از خرید شما از فروشگاه باگتو"), normalFont);
            footer.SpacingBefore = 25f;
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);

            // اطلاعات تماس
            var contact = new Paragraph(
                ReversePersianText("تلفن پشتیبانی: 021-12345678 | آدرس: تهران، خیابان نمونه"),
                new Font(CreatePersianFont(), 9, Font.NORMAL, BaseColor.GRAY)
            );
            contact.Alignment = Element.ALIGN_CENTER;
            contact.SpacingBefore = 10f;
            document.Add(contact);
        }

        private void AddSummaryRow(PdfPTable table, string label, string value, Font font)
        {
            var labelCell = new PdfPCell(new Phrase(label, font));
            labelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.Padding = 6f;
            table.AddCell(labelCell);

            var valueCell = new PdfPCell(new Phrase(value, font));
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.Padding = 6f;
            table.AddCell(valueCell);
        }
    }
}
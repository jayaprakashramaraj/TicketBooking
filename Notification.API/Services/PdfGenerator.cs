using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace Notification.API.Services
{
    public interface IPdfGenerator
    {
        byte[] GenerateTicket(string customerEmail, string showName, DateTime showTime, List<string> seats, decimal amount);
    }

    public class PdfGenerator : IPdfGenerator
    {
        public byte[] GenerateTicket(string customerEmail, string showName, DateTime showTime, List<string> seats, decimal amount)
        {
            // QuestPDF requires a license configuration for newer versions, 
            // but for this demo, we'll assume it's configured or using the Community version.
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Ticket Booking Confirmation").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(20);
                        x.Item().Text($"Customer: {customerEmail}");
                        x.Item().Text($"Movie: {showName}");
                        x.Item().Text($"Time: {showTime:f}");
                        x.Item().Text($"Seats: {string.Join(", ", seats)}");
                        x.Item().Text($"Total Amount: {amount:C}");
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}

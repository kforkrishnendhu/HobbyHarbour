using HobbyHarbour.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Razorpay.Api;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Order = HobbyHarbour.Models.Order;

public class PdfGenerator
{
    public byte[] GenerateInvoice(Order invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));

                page.Header()
                    .Text($"Invoice #{invoice.InvoiceNumber}")
                    .SemiBold().FontSize(15).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // step 2
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#");
                            header.Cell().Element(CellStyle).Text("Product");
                            header.Cell().Element(CellStyle).AlignRight().Text("Unit price");
                            header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                            header.Cell().Element(CellStyle).AlignRight().Text("Total");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });
                        int i = 0;
                        var cultureInfo = new CultureInfo("en-IN"); 
                        var formatInfo = cultureInfo.NumberFormat;
                        foreach (var item in invoice.LineItems)
                        {
                            table.Cell().Element(CellStyle).Text(++i);
                            table.Cell().Element(CellStyle).Text(item.Product.ProductName);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.UnitPrice);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity);
                            table.Cell().Element(CellStyle).AlignRight().Text((item.UnitPrice * item.Quantity));

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        }

                        table.Cell().ColumnSpan(3).Text("Shipping charge:").FontSize(14);
                        table.Cell().ColumnSpan(2).AlignRight().Text("10.00").FontSize(14);
                        table.Cell().ColumnSpan(3).Text("Total Amount:");
                        table.Cell().ColumnSpan(2).AlignRight().Text(invoice.TotalAmount);

                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateSalesReport(List<Order> orders)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Background(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));

                page.Header()
                    .Text("Sales Report")
                    .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Order ID");
                            header.Cell().Element(CellStyle).AlignRight().Text("Order Date");
                            header.Cell().Element(CellStyle).AlignRight().Text("Total Amount");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });
                        var cultureInfo = new CultureInfo("en-IN");
                        var formatInfo = cultureInfo.NumberFormat;
                        foreach (var order in orders)
                        {
                            table.Cell().Element(CellStyle).Text(order.OrderID.ToString());
                            table.Cell().Element(CellStyle).Text(order.OrderDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).AlignRight().Text(order.TotalAmount);

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }

                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        return document.GeneratePdf();
    }
}

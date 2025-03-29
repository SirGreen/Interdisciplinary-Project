using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using DADN.Controllers;

public class PdfExportService
{
    public byte[] GenerateGearboxPdf(TechnicalData content) {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var stream = new MemoryStream();
        
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(16));
                
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text("Design Report").FontSize(20).Bold();
                        column.Item().PaddingTop(10);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Property column
                                columns.RelativeColumn(3); // Value column
                            });
                            
                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(2).Padding(8).Text("Thông số").Bold();
                                header.Cell().BorderBottom(2).Padding(8).Text("Giá trị").Bold();
                            });
                            
                            // Simple properties
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Overload Factor");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.OverloadFactor.ToString("F2"));
                            
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Overall Efficiency");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.OverallEfficiency.ToString("P2"));
                            
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Required Motor Efficiency");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.RequiredMotorEfficiency.ToString("F2"));
                            
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Required Motor Speed");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.RequiredMotorSpeed.ToString("F2"));
                            
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("NSB Speed");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.NsbSpeed.ToString("F2"));
                            
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Un");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.Un.ToString("F2"));
                        });
                        
                        // Torque specifications section
                        column.Item().PaddingTop(20).Text("Công suất tốc độ quay và momen").FontSize(16).Bold();
                        column.Item().PaddingTop(10);
                        
                        // Parse and display torque data as a table
                        if (!string.IsNullOrEmpty(content.MomenSoVongQuay))
                        {
                            column.Item().Table(torqueTable =>
                            {
                                torqueTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // Shaft
                                    columns.RelativeColumn(); // Power
                                    columns.RelativeColumn(); // Speed
                                    columns.RelativeColumn(); // Torque
                                });
                                
                                // Torque table header
                                torqueTable.Header(header =>
                                {
                                    header.Cell().Text("Shaft").Bold();
                                    header.Cell().Text("Power (kW)").Bold();
                                    header.Cell().Text("Speed (rpm)").Bold();
                                    header.Cell().Text("Torque (N.mm)").Bold();
                                });
                                
                                var torqueLines = content.MomenSoVongQuay.Split('\n');
                                foreach (var line in torqueLines)
                                {
                                    if (string.IsNullOrWhiteSpace(line)) continue;
                                    
                                    var parts = line.Split(':');
                                    var shaft = parts[0].Trim();
                                    var values = parts[1].Split(',');
                                    
                                    // Shaft column
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(shaft);
                                    
                                    // Power column
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(values[0].Split('=')[1].Trim());
                                    
                                    // Speed column
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(values[1].Split('=')[1].Trim());
                                    
                                    // Torque column
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(values[2].Split('=')[1].Trim());
                                }
                            });
                        }
                        
                        column.Item().Text($"Generated on {DateTime.Now}");
                    });
            });
        })
        .GeneratePdf(stream);
        
        stream.Position = 0;
        
        // Return the file
        Console.WriteLine("Returning bytes");
        return stream.ToArray();
    }
    // public byte[] GenerateGearboxPdf(Dictionary<string, object> calculationResults)
    // {
        
        
    // }
}
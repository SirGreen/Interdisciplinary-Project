using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using DADN.Controllers;

public class PdfExportService
{
    public byte[] GenerateGearboxPdf(TechnicalData content)
    {
        // Debug output for TechnicalData
        Console.WriteLine("--- TechnicalData Debug Output ---");
        Console.WriteLine($"OverloadFactor: {content.OverloadFactor}");
        Console.WriteLine($"OverallEfficiency: {content.OverallEfficiency}");
        Console.WriteLine($"RequiredMotorEfficiency: {content.RequiredMotorEfficiency}");
        Console.WriteLine($"RequiredMotorSpeed: {content.RequiredMotorSpeed}");
        Console.WriteLine($"NsbSpeed: {content.NsbSpeed}");
        Console.WriteLine($"Un: {content.Un}");
        Console.WriteLine($"MomenSoVongQuay: {content.MomenSoVongQuay ?? "null"}");

        // Debug motor details if present
        if (content.Motor != null)
        {
            Console.WriteLine("--- Motor Data ---");
            Console.WriteLine($"Technology: {content.Motor.Technology}");
            Console.WriteLine($"Power: {content.Motor.Power}");
            Console.WriteLine($"Model: {content.Motor.Model}");
            Console.WriteLine($"FrameSize: {content.Motor.FrameSize}");
            Console.WriteLine($"Speed: {content.Motor.Speed}");
            Console.WriteLine($"Standard: {content.Motor.Standard}");
            Console.WriteLine($"Voltage: {content.Motor.Voltage}");
            Console.WriteLine($"MountingType: {content.Motor.MountingType}");
            Console.WriteLine($"Material: {content.Motor.Material}");
            Console.WriteLine($"Protection: {content.Motor.Protection}");
            Console.WriteLine($"ShaftDiameter: {content.Motor.ShaftDiameter}");
            Console.WriteLine($"URL: {content.Motor.URL}");

            if (content.Motor.Image != null && content.Motor.Image.Any())
            {
                Console.WriteLine($"Image Count: {content.Motor.Image.Count()}");
                for (int i = 0; i < content.Motor.Image.Count(); i++)
                {
                    Console.WriteLine($"Image {i + 1} Length: {content.Motor.Image.ElementAt(i)?.Length ?? 0}");
                }
            }
            else
            {
                Console.WriteLine("No motor images available");
            }
        }
        else
        {
            Console.WriteLine("No motor data available");
        }

        Console.WriteLine("--- End of Debug Output ---");
        QuestPDF.Settings.License = LicenseType.Community;
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ",";

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

                        column.Item().Text("Design Blueprint").Bold().AlignLeft();
                        column.Item().Image("wwwroot/assets/images/designdraw/hopgiamtoc.jpg");

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

                                    try
                                    {
                                        // Split by colon and check if we have at least 2 parts
                                        var parts = line.Split(':', 2);
                                        if (parts.Length < 2) continue;
                                        
                                        var shaft = parts[0].Trim();
                                        var dataSection = parts[1].Trim();

                                        // Extract values using regex pattern for more reliability
                                        var powerMatch = System.Text.RegularExpressions.Regex.Match(dataSection, @"Power\s*=\s*([0-9,.]+)");
                                        var speedMatch = System.Text.RegularExpressions.Regex.Match(dataSection, @"Speed\s*=\s*([0-9,.]+)");
                                        var torqueMatch = System.Text.RegularExpressions.Regex.Match(dataSection, @"Torque\s*=\s*([0-9,.]+)");

                                        // Shaft column
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(shaft);

                                        // Power column
                                        var powerText = powerMatch.Success ? 
                                            Math.Ceiling(double.Parse(powerMatch.Groups[1].Value, culture) * 100) / 100 : 0;
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(powerText.ToString());

                                        // Speed column
                                        var speedText = speedMatch.Success ? 
                                            Math.Ceiling(double.Parse(speedMatch.Groups[1].Value, culture) * 100) / 100 : 0;
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(speedText.ToString());

                                        // Torque column
                                        var torqueText = torqueMatch.Success ? 
                                            Math.Ceiling(double.Parse(torqueMatch.Groups[1].Value, culture) * 100) / 100 : 0;
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(torqueText.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        // Log the error but continue processing
                                        Console.WriteLine($"Error parsing line: {line}. Error: {ex.Message}");
                                        
                                        // Fill in blank cells to maintain table structure
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Error");
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                                        torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                                    }
                                }
                            });
                        }
                        column.Item().PaddingTop(30);
                        column.Item().Text("Motor").Bold().AlignLeft();
                        if (content.Motor != null)
                        {
                            MotorCatalog motorCatalog = content.Motor;
                            column.Item().Table(table =>
                            {
                                // Đặt cột
                                table
                                    .ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                    });
                                table
                                    .Header(header =>
                                    {
                                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Attribute").Bold();
                                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Value").Bold();
                                    });

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Technology");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Technology);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Power");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Power);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Model");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Model);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Frame Size");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.FrameSize);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Speed");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Speed);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Standard");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Standard);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Voltage");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Voltage);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Mounting Type");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.MountingType);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Material");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Material);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Protection");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.Protection);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Shaft Diameter");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.ShaftDiameter);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("URL");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Hyperlink(motorCatalog.URL).Text("Xem").FontColor(Colors.Blue.Lighten2);
                            });

                            // In danh sách hình ảnh
                            column.Item().Text("Engine Images:");

                            if (content.Motor.Image != null && content.Motor.Image.Any())
                            {
                                foreach (var imageData in content.Motor.Image)
                                {
                                    try
                                    {
                                        byte[] imageBytes = Convert.FromBase64String(imageData);
                                        column.Item().Width(2, Unit.Inch).Image(imageBytes);
                                    }
                                    catch
                                    {
                                        column.Item().Text("Không tải được ảnh");
                                    }
                                }
                            }
                            else
                            {
                                column.Item().Text("Không có ảnh");
                            }
                        }
                        column.Item().PaddingTop(30);
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
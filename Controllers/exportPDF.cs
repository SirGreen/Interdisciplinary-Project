using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using DADN.Controllers;
using DADN.Models;
using System.Net;
using System.Text.RegularExpressions;

public class PdfExportService
{
    public byte[] GenerateGearboxPdf(TechnicalData content)
    {
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

                                    var parts = line.Split(':');
                                    var shaft = parts[0].Trim();
                                    var values = parts[1].Split(',');

                                    // Shaft column
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(shaft);

                                    // Power column
                                    var pow = double.Parse(values[0].Split('=')[1].Trim().Split(" ")[0]);
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text((Math.Ceiling(pow * 100) / 100).ToString());

                                    // Speed column
                                    var spd = double.Parse(values[1].Split('=')[1].Trim().Split(" ")[0]);
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text((Math.Ceiling(spd * 100) / 100).ToString());

                                    // Torque column
                                    var tor = double.Parse(values[2].Split('=')[1].Trim().Split(" ")[0]);
                                    torqueTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text((Math.Ceiling(tor * 100) / 100).ToString());
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

                                // Basic Information
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Brand");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.brand);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Category");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.category);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Product Name");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.product_name);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Motor Type");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.motor_type);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Frame Size");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.frame_size);

                                // Power and Speed
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Output Power (kW)");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.output_kw);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Output Power (HP)");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.output_hp);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Full Load RPM");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.full_load_rpm);

                                // Current Ratings
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Current at 380V");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.current_380v);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Current at 400V");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.current_400v);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Current at 415V");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.current_415v);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("LRC Current");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.current_lrc);

                                // Efficiency
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Efficiency at 1/2 Load");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.efficiency_1_2);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Efficiency at 3/4 Load");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.efficiency_3_4);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Efficiency at Full Load");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.efficiency_full);

                                // Power Factor
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Power Factor at 1/2 Load");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.power_factor_1_2);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Power Factor at 3/4 Load");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.power_factor_3_4);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Power Factor at Full Load");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.power_factor_full);

                                // Torque
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Breakdown Torque");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.torque_break_down);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Full Load Torque");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.torque_full);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Locked Rotor Torque");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.torque_locked_rotor);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Pull-up Torque");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.torque_pull_up);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Rotor GD²");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.torque_rotor_gd2);

                                // Additional Information
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Weight (kg)");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(motorCatalog.weight_kg);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("URL");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Hyperlink(motorCatalog.url).Text("Xem").FontColor(Colors.Blue.Lighten2);
                            });


                            // In danh sách hình ảnh
                            column.Item().Text("Engine Images:");
                            Console.WriteLine("[DEBUG] ===== Engine Image Processing =====");
                            Console.WriteLine($"[DEBUG] motorCatalog.Image is null? {motorCatalog.Image == null}");
                            Console.WriteLine($"[DEBUG] motorCatalog.Image length: {(motorCatalog.Image?.Length ?? 0)}");
                            Console.WriteLine($"[DEBUG] motorCatalog.Image first 100 chars: {(motorCatalog.Image?.Substring(0, Math.Min(100, motorCatalog.Image.Length)) ?? "null")}...");

                            if (!string.IsNullOrEmpty(motorCatalog.Image))
                            {
                                try
                                {
                                    Console.WriteLine("[DEBUG] Found image in motor catalog");
                                    byte[] imageBytes = Convert.FromBase64String(motorCatalog.Image);
                                    Console.WriteLine($"[DEBUG] Successfully converted base64 to image bytes, length: {imageBytes.Length}");
                                    column.Item().Width(5, Unit.Inch).Image(imageBytes);
                                    Console.WriteLine("[DEBUG] Added engine image to PDF");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[DEBUG] Error processing engine image: {ex.Message}");
                                    Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                                    column.Item().Text("Không tải được ảnh");
                                }
                            }
                            else
                            {
                                Console.WriteLine("[DEBUG] No engine image found in motor catalog");
                                column.Item().Text("Không có ảnh");
                            }

                            column.Item().Text("Sectional Images:");
                            Console.WriteLine("[DEBUG] Processing sectional images section");

                            var imagePath = FindMatchingImage(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets/images/CatalogImg"), motorCatalog.product_name, motorCatalog.frame_size);
                            Console.WriteLine($"[DEBUG] Looking for sectional image with:");
                            Console.WriteLine($"[DEBUG] - Product name: {motorCatalog.product_name}");
                            Console.WriteLine($"[DEBUG] - Frame size: {motorCatalog.frame_size}");

                            if (imagePath != null)
                            {
                                Console.WriteLine($"[DEBUG] Found matching sectional image at: {imagePath}");
                                try
                                {
                                    column.Item().Width(5, Unit.Inch).Image(imagePath);
                                    Console.WriteLine("[DEBUG] Successfully added sectional image to PDF");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[DEBUG] Error adding sectional image to PDF: {ex.Message}");
                                    Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                                    column.Item().Text("Không tải được ảnh");
                                }
                            }
                            else
                            {
                                Console.WriteLine("[DEBUG] No matching sectional image found");
                                column.Item().Text("Không tìm thấy ảnh");
                            }

                        }

                        column.Item().PaddingTop(30);
                        column.Item().PaddingTop(20).Text("Thông số bộ truyền xích").FontSize(16).Bold();
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
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Bước Xích ");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.pitch.ToString("F2"));

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Khoảng cách trục");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.shaftDistance.ToString("F2"));

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Kết luận an toàn xích");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.chainSafe.ToString());

                            // table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Đường kính đĩa xích");
                            // table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.diskDiameterCalc.ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Độ bền tiếp xúc");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.contactStrength.ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Lực tác dụng lên trục");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.shaftForce.ToString("F2"));
                        });

                        column.Item().PaddingTop(30);
                        column.Item().PaddingTop(20).Text("Thông số bánh răng truyền nhỏ").FontSize(16).Bold();
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
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Vật liệu");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh nhỏ"]["Vật liệu"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Ob");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh nhỏ"]["Ob"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Och");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh nhỏ"]["Och"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("HB");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh nhỏ"]["HB"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Kích thước");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh nhỏ"]["Kích thước S"].ToString());
                        });

                        column.Item().PaddingTop(20).Text("Thông số bánh răng truyền lớn").FontSize(16).Bold();
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
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Vật liệu");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh lớn"]["Vật liệu"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Ob");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh lớn"]["Ob"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Och");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh lớn"]["Och"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("HB");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh lớn"]["HB"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Kích thước");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.vatLieuBoTruyen["Bánh lớn"]["Kích thước"].ToString());
                        });

                        column.Item().PaddingTop(30);
                        column.Item().PaddingTop(20).Text("Thông số ứng xuất").FontSize(16).Bold();
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
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Ohlim1");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Ohlim1"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Ohlim2");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Ohlim2"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Sh");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Sh"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Oflim1");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Oflim1"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Oflim2");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Oflim2"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Sf");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Sf"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("HB1");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["HB1"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("HB2");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["HB2"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Nho1");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Nho1"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Nho2");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Nho2"].ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Nfo");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(content.dauVaoUngSuat["Nfo"].ToString());
                        });

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

    public static string? FindMatchingImage(string imagesDirectory, string productCode, string frameSize)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Starting FindMatchingImage with:");
            Console.WriteLine($"[DEBUG] - imagesDirectory: {imagesDirectory}");
            Console.WriteLine($"[DEBUG] - productCode: {productCode}");
            Console.WriteLine($"[DEBUG] - frameSize: {frameSize}");

            // Tách lấy tên sản phẩm từ product code (bỏ phần pole)
            string productName = productCode.Split(' ')[0];
            Console.WriteLine($"[DEBUG] Extracted product name: {productName}");

            var files = Directory.GetFiles(imagesDirectory, "*.png");
            Console.WriteLine($"[DEBUG] Found {files.Length} PNG files in directory");

            int userFrame = ExtractFrameNumber(frameSize);
            Console.WriteLine($"[DEBUG] Extracted user frame number: {userFrame}");

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine($"[DEBUG] Processing file: {fileName}");

                string[] parts = fileName.Split('-');
                Console.WriteLine($"[DEBUG] Split parts count: {parts.Length}");

                // Kiểm tra đúng format: <chân đế>-<tên 1>-<tên 2>-<tên 3>-<start_frame>-<end_frame>
                if (parts.Length != 6)
                {
                    Console.WriteLine($"[DEBUG] Skipping file - incorrect format (need 6 parts)");
                    continue;
                }

                // Lấy 3 tên từ file
                string name1 = parts[1];
                string name2 = parts[2];
                string name3 = parts[3];
                string startFrame = parts[4];
                string endFrame = parts[5];

                Console.WriteLine($"[DEBUG] File details:");
                Console.WriteLine($"[DEBUG] - Name 1: {name1}");
                Console.WriteLine($"[DEBUG] - Name 2: {name2}");
                Console.WriteLine($"[DEBUG] - Name 3: {name3}");
                Console.WriteLine($"[DEBUG] - Start frame: {startFrame}");
                Console.WriteLine($"[DEBUG] - End frame: {endFrame}");

                // Kiểm tra xem tên sản phẩm có khớp với một trong ba tên không
                bool nameMatches = string.Equals(name1, productName, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(name2, productName, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(name3, productName, StringComparison.OrdinalIgnoreCase);

                if (!nameMatches)
                {
                    Console.WriteLine($"[DEBUG] Skipping file - product name doesn't match any of the three names");
                    continue;
                }

                int start = ExtractFrameNumber(startFrame);
                int end = ExtractFrameNumber(endFrame);
                Console.WriteLine($"[DEBUG] Frame range: {start} to {end}");

                if (userFrame >= start && userFrame <= end)
                {
                    Console.WriteLine($"[DEBUG] Found matching file: {file}");
                    return file;
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Frame {userFrame} not in range {start}-{end}");
                }
            }

            Console.WriteLine("[DEBUG] No matching file found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error finding matching image: {ex.Message}");
            Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
        }

        return null;
    }

    private static int ExtractFrameNumber(string frame)
    {
        Console.WriteLine($"[DEBUG] Extracting frame number from: {frame}");
        var match = Regex.Match(frame, @"\d+");
        int result = match.Success ? int.Parse(match.Value) : -1;
        Console.WriteLine($"[DEBUG] Extracted frame number: {result}");
        return result;
    }
}
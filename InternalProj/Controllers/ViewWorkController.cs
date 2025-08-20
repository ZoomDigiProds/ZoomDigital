using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize("ADMIN")]
    public class ViewWorkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ViewWorkController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(bool isPartial = false)
        {
            var branchIdStr = HttpContext.Session.GetString("BranchId");
            if (string.IsNullOrEmpty(branchIdStr) || !int.TryParse(branchIdStr, out int branchId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new WorkOrderViewModel
            {
                StudioList = _context.CustomerRegs
                    .Where(c => c.BranchId == branchId)
                    .Select(c => c.StudioName)
                    .Distinct()
                    .ToList(),

                WorkTypes = _context.WorkTypes.ToList(),
                Results = new List<WorkOrderMaster>(),
                CurrentPage = 1,
                TotalPages = 0
            };

            if (isPartial)
            {
                return PartialView("_WorkOrderReportPartial", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId, int currentPage = 1, bool isPartial = false)
        {
            var branchIdStr = HttpContext.Session.GetString("BranchId");
            if (string.IsNullOrEmpty(branchIdStr) || !int.TryParse(branchIdStr, out int branchId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            const int pageSize = 10;

            var query = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .Where(w => w.Active == "Y" &&
                            w.BranchId == branchId &&
                            w.Customer != null &&
                            w.Customer.BranchId == branchId);


            if (!string.IsNullOrEmpty(studio))
            {
                query = query.Where(w => w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower());
            }

            if (fromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(w => w.Wdate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(w => w.Wdate <= toUtc);
            }

            if (workTypeId.HasValue && workTypeId.Value > 0)
            {
                query = query.Where(w => w.WorkTypeId == workTypeId);
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var results = query
                .OrderBy(w => w.Wdate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new WorkOrderViewModel
            {
                StudioList = _context.CustomerRegs
                    .Where(c => c.BranchId == branchId)
                    .Select(c => c.StudioName)
                    .Distinct()
                    .ToList(),

                WorkTypes = _context.WorkTypes.ToList(),
                Results = results,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                StudioFilter = studio,
                FromDateFilter = fromDate,
                ToDateFilter = toDate,
                WorkTypeFilter = workTypeId
            };

            if (isPartial)
            {
                return PartialView("_WorkOrderReportPartial", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DownloadExcel(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId)
        {
            var branchIdStr = HttpContext.Session.GetString("BranchId");
            if (string.IsNullOrEmpty(branchIdStr) || !int.TryParse(branchIdStr, out int branchId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var query = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .Where(w => w.Active == "Y" && w.BranchId == branchId && w.Customer != null && w.Customer.BranchId == branchId);


            if (!string.IsNullOrEmpty(studio))
            {
                query = query.Where(w => w.Customer != null &&
                                             w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower() &&
                                             w.Customer.BranchId == branchId);
            }

            if (fromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(w => w.Wdate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(w => w.Wdate <= toUtc);
            }

            if (workTypeId.HasValue && workTypeId.Value > 0)
            {
                query = query.Where(w => w.WorkTypeId == workTypeId);
            }

            var results = query.ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("WorkOrders");

            // Header and titles
            ws.Cell(1, 1).Value = "Work Order Report";
            ws.Range(1, 1, 1, 8).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            string fromStr = fromDate?.ToString("dd-MM-yyyy") ?? "N/A";
            string toStr = toDate?.ToString("dd-MM-yyyy") ?? "N/A";
            ws.Cell(2, 1).Value = $"Date Range: {fromStr} to {toStr}";
            ws.Range(2, 1, 2, 8).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(3, 1).Value = "Sl No";
            ws.Cell(3, 2).Value = "Work Order No";
            ws.Cell(3, 3).Value = "Studio";
            ws.Cell(3, 4).Value = "Work Type";
            ws.Cell(3, 5).Value = "Date";
            ws.Cell(3, 6).Value = "Amount";
            ws.Cell(3, 7).Value = "Advance";
            ws.Cell(3, 8).Value = "Balance";
            ws.Range(3, 1, 3, 8).Style.Font.SetBold();

            int row = 4, slno = 1;
            decimal totalBalance = 0;

            foreach (var item in results)
            {
                decimal balance = (decimal)item.SubTotal - (decimal)(item.Advance ?? 0);
                totalBalance += balance;

                ws.Cell(row, 1).Value = slno++;
                ws.Cell(row, 2).Value = item.WorkOrderNo ?? "";
                ws.Cell(row, 3).Value = item.Customer?.StudioName ?? "";
                ws.Cell(row, 4).Value = item.WorkType?.TypeName ?? "";
                ws.Cell(row, 5).Value = item.Wdate?.ToLocalTime().ToString("dd-MM-yyyy") ?? "";
                ws.Cell(row, 6).Value = item.SubTotal;
                ws.Cell(row, 7).Value = item.Advance ?? 0;
                ws.Cell(row, 8).Value = balance;
                row++;
            }

            // Add total row
            ws.Cell(row, 7).Value = "Total:";
            ws.Cell(row, 7).Style.Font.SetBold();
            ws.Cell(row, 8).Value = totalBalance;
            ws.Cell(row, 8).Style.Font.SetBold();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            string fileName = $"WorkOrders_{DateTime.Now:dd-MM-yyyy}.xlsx";
            return File(ms.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",fileName);
        }

        [HttpPost]
        public IActionResult DownloadPdf(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId)
        {
            var branchIdStr = HttpContext.Session.GetString("BranchId");
            if (string.IsNullOrEmpty(branchIdStr) || !int.TryParse(branchIdStr, out int branchId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var query = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .Where(w => w.Active == "Y" && w.BranchId == branchId && w.Customer != null && w.Customer.BranchId == branchId);

            if (!string.IsNullOrEmpty(studio))
            {
                query = query.Where(w => w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower());
            }

            if (fromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(w => w.Wdate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(w => w.Wdate <= toUtc);
            }

            if (workTypeId.HasValue && workTypeId.Value > 0)
            {
                query = query.Where(w => w.WorkTypeId == workTypeId);
            }

            var results = query.ToList();

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            doc.Add(new Paragraph("Work Order Report")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetMarginBottom(10));

            //Date Range
            string fromStr = fromDate?.ToString("dd-MM-yyyy") ?? "N/A";
            string toStr = toDate?.ToString("dd-MM-yyyy") ?? "N/A";
            doc.Add(new Paragraph($"Date Range: {fromStr} to {toStr}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Table header
            var table = new Table(8).UseAllAvailableWidth();
            string[] headers = { "Sl No", "Work Order No", "Studio", "Work Type", "Date", "Amount", "Advance", "Balance" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(boldFont)));
            }

            // Table rows
            int slno = 1;
            decimal totalBalance = 0;

            foreach (var item in results)
            {
                decimal balance = (decimal)item.SubTotal - (decimal)(item.Advance ?? 0);
                totalBalance += balance;


                table.AddCell(new Paragraph((slno++).ToString()).SetFont(normalFont));
                table.AddCell(new Paragraph(item.WorkOrderNo ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.Customer?.StudioName ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.WorkType?.TypeName ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.Wdate?.ToLocalTime().ToString("dd-MM-yyyy") ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.SubTotal.ToString("N2")).SetFont(normalFont));
                table.AddCell(new Paragraph(item.Advance?.ToString("N2") ?? "0.00").SetFont(normalFont));
                table.AddCell(new Paragraph(balance.ToString("N2")).SetFont(normalFont));
            }

            // Add total row
            table.AddCell(new Cell(1, 7) // merge 7 cells
                .Add(new Paragraph("Total:").SetFont(boldFont))
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
            table.AddCell(new Paragraph(totalBalance.ToString("N2")).SetFont(boldFont));

            doc.Add(table);
            doc.Close();

            string fileName = $"WorkOrders_{DateTime.Now:dd-MM-yyyy}.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }
    }
}

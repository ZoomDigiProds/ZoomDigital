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
        public IActionResult Index()
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

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId, int currentPage = 1)
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
            foreach (var item in results)
            {
                ws.Cell(row, 1).Value = slno++;
                ws.Cell(row, 2).Value = item.WorkOrderNo ?? "";
                ws.Cell(row, 3).Value = item.Customer?.StudioName ?? "";
                ws.Cell(row, 4).Value = item.WorkType?.TypeName ?? "";
                ws.Cell(row, 5).Value = item.Wdate?.ToLocalTime().ToString("dd-MM-yyyy") ?? "";
                ws.Cell(row, 6).Value = item.SubTotal;
                ws.Cell(row, 7).Value = item.Advance ?? 0;
                ws.Cell(row, 8).Value = item.SubTotal - (item.Advance ?? 0);
                row++;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WorkOrders.xlsx");
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


            var studioList = _context.CustomerRegs
                .Where(c => c.BranchId == branchId)
                .Select(c => c.StudioName)
                .Distinct()
                .ToList();

            if (!string.IsNullOrEmpty(studio))
            {
                query = query.Where(w => w.Customer != null &&
                             w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower() &&
                             w.Customer.BranchId == branchId);

            }

            if (fromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
                query = query.Where(w => w.Wdate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
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

            doc.Add(new Paragraph("Work Order Report").SetFont(boldFont).SetFontSize(16).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetMarginBottom(10));

            string rangeText = fromDate.HasValue || toDate.HasValue
                ? $"From: {fromDate?.ToString("dd-MM-yyyy") ?? "N/A"} To: {toDate?.ToString("dd-MM-yyyy") ?? "N/A"}"
                : "";

            if (!string.IsNullOrEmpty(rangeText))
            {
                doc.Add(new Paragraph(rangeText).SetFont(normalFont).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetMarginBottom(20));
            }

            Table table = new Table(8).UseAllAvailableWidth();
            string[] headers = { "Sl No", "Work Order No", "Studio", "Work Type", "Date", "Amount", "Advance", "Balance" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(boldFont)));
            }

            int slno = 1;
            foreach (var item in results)
            {
                table.AddCell(new Paragraph((slno++).ToString()).SetFont(normalFont));
                table.AddCell(new Paragraph(item.WorkOrderNo ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.Customer?.StudioName ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.WorkType?.TypeName ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.Wdate?.ToLocalTime().ToString("dd-MM-yyyy") ?? "").SetFont(normalFont));
                table.AddCell(new Paragraph(item.SubTotal.ToString("N2")).SetFont(normalFont));
                table.AddCell(new Paragraph(item.Advance?.ToString("N2") ?? "0.00").SetFont(normalFont));
                table.AddCell(new Paragraph((item.SubTotal - (item.Advance ?? 0)).ToString("N2")).SetFont(normalFont));
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(), "application/pdf", "WorkOrders.pdf");
        }
    }
}

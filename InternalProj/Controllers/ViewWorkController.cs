using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
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
    [DepartmentAuthorize()]
    public class ViewWorkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ViewWorkController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId, int currentPage = 1, bool isSearch = false)
        {
            const int pageSize = 10;

            bool hasAnyFilter = !string.IsNullOrEmpty(studio) || fromDate.HasValue || toDate.HasValue || (workTypeId.HasValue && workTypeId.Value > 0);
            bool shouldShowResults = isSearch || hasAnyFilter;

            var query = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .Include(w => w.AlbumSize)
                .Where(w => w.Active == "Y");

            if (shouldShowResults)
            {
                if (!string.IsNullOrWhiteSpace(studio))
                    query = query.Where(w => w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower());

                if (fromDate.HasValue)
                    query = query.Where(w => w.Wdate >= DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc));

                if (toDate.HasValue)
                    query = query.Where(w => w.Wdate <= DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc));

                if (workTypeId.HasValue && workTypeId.Value > 0)
                    query = query.Where(w => w.WorkTypeId == workTypeId.Value);
            }
            else
            {
                query = query.Where(w => false);
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var results = query
                .OrderBy(w => w.Wdate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(w =>
                {
                    var partialPayments = _context.Receipts
                        .Where(r => r.WorkOrderId == w.WorkOrderId)
                        .OrderBy(r => r.ReceiptDate)
                        .Select(r => new PartialPaymentDto
                        {
                            ReceiptDate = r.ReceiptDate,
                            Amount = r.CurrentAmount
                        })
                        .ToList();

                    double totalPaid = partialPayments.Sum(p => p.Amount);

                    return new WorkOrderSummaryViewModel
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        StudioName = w.Customer?.StudioName,
                        Size = w.AlbumSize?.Size ?? "N/A",
                        Advance = w.Advance ?? 0,
                        SubTotal = w.SubTotal,
                        TotalPaid = totalPaid,
                        Balance = Math.Max(0, w.SubTotal - (w.Advance ?? 0) - totalPaid),
                        WorkTypeName = w.WorkType?.TypeName,
                        Wdate = w.Wdate,
                        PartialPayments = partialPayments
                    };
                }).ToList();

            var viewModel = new WorkOrderViewModel
            {
                StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList(),
                WorkTypes = _context.WorkTypes.ToList(),
                ResultsSummary = results,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                StudioFilter = studio,
                FromDateFilter = fromDate,
                ToDateFilter = toDate,
                WorkTypeFilter = workTypeId
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WorkOrderResultsPartial", viewModel);
            }

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Search(string studio, DateTime? fromDate, DateTime? toDate, int? WorkTypeFilter, int currentPage = 1)
        {
            return RedirectToAction("Index", new
            {
                studio,
                fromDate = fromDate?.ToString("yyyy-MM-dd"),
                toDate = toDate?.ToString("yyyy-MM-dd"),
                workTypeId = WorkTypeFilter,
                currentPage,
                isSearch = true,
            });
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

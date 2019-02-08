using iTextSharp.text;
using iTextSharp.text.pdf;
using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Classes {
    public class ReportOperations : ReportPDFBase {

        private static int pdfReportRecordCount = 35;

        public byte[] GenerateReportForTotalDocumentsDownloaded(string physicalPath) {
            GenerateReportBase();
            l1.Add(HeaderLogo(physicalPath));
            l1.Add(SubjectBlock(new Paragraph("Report Name: Total Documents Downloaded with Dates and Times")));
            PdfPTable table = new PdfPTable(3);
            table.AddCell(CellHeader("User Name"));
            table.AddCell(CellHeader("Document Name"));
            table.AddCell(CellHeader("Date Time"));
            List<FilesDownloadAuditTrail> filesDownloadedList = (new AuditTrailOperations()).GetTotalFilesDownloadedAuditTrails();
            int recordsCount = 0;
            foreach (FilesDownloadAuditTrail item in filesDownloadedList) {
                table.AddCell(CellData(item.UserName));
                table.AddCell(CellData(item.FileName));
                table.AddCell(CellData(item.DateTimeDownloaded.ToString()));
                if (recordsCount >= pdfReportRecordCount) { break; }
                recordsCount++;
            }
            l1.Add(table);
            FooterLines.Add("DateTime: " + DateTime.Now.ToString());
            l1.Close();
            DocumentBytes = PDFStream.GetBuffer();
            return DocumentBytes;
        }

        public byte[] GenerateReportForDocumentsDownloadedBySpecificUser(string physicalPath, string userName) {
            GenerateReportBase();
            l1.Add(HeaderLogo(physicalPath));
            l1.Add(SubjectBlock(new Paragraph("Report Name: Documents Downloaded with Dates and Times")));
            l1.Add(UserName(new Paragraph("User Name:  " + userName)));
            PdfPTable table = new PdfPTable(2);
            table.AddCell(CellHeader("Document Name"));
            table.AddCell(CellHeader("Date Time"));
            List<FilesDownloadAuditTrail> filesDownloadedList = (new AuditTrailOperations()).GetFilesDownloadedAuditTrailsBySpecificUser(userName);
            int recordsCount = 0;
            foreach (FilesDownloadAuditTrail item in filesDownloadedList) {
                table.AddCell(CellData(item.FileName));
                table.AddCell(CellData(item.DateTimeDownloaded.ToString()));
                if (recordsCount >= pdfReportRecordCount) { break; }
                recordsCount++;
            }
            l1.Add(table);
            FooterLines.Add("DateTime: " + DateTime.Now.ToString());
            l1.Close();
            DocumentBytes = PDFStream.GetBuffer();
            return DocumentBytes;
        }

        public byte[] GenerateReportForUserActivity(string physicalPath, string userName) {
            GenerateReportBase();
            l1.Add(HeaderLogo(physicalPath));
            l1.Add(SubjectBlock(new Paragraph("Report Name: User Activity with Dates and Times")));
            l1.Add(UserName(new Paragraph("User Name:  " + userName)));
            PdfPTable table = new PdfPTable(2);
            table.AddCell(CellHeader("User Name"));
            table.AddCell(CellHeader("Date Time"));
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            List<UserLoginAuditTrail> userActivityAuditTrails = aNaOps.GetUserActivityAuditTrailsBySpecificUser(aNaOps.GetUserIDByUserName(userName));
            int recordsCount = 0;
            foreach (UserLoginAuditTrail item in userActivityAuditTrails) {
                table.AddCell(CellData(item.UserName));
                table.AddCell(CellData(item.DateTimeLogged.ToString()));
                if (recordsCount >= pdfReportRecordCount) { break; }
                recordsCount++;
            } 
            l1.Add(table);
            FooterLines.Add("DateTime: " + DateTime.Now.ToString());
            l1.Close();
            DocumentBytes = PDFStream.GetBuffer();
            return DocumentBytes;
        }

        public HashSet<String> GetUsersInAuditTrails() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                HashSet<String> hashArr = new HashSet<string>();
                List<String> userLoginList = (from p in dockerEntities.UserLoginAuditTrails
                                                       select p.UserName).ToList();
                List<String> FileDownloadList = (from p in dockerEntities.FilesDownloadAuditTrails
                                              select p.UserName).ToList();
                foreach (String name in userLoginList) { hashArr.Add(name); }
                foreach (String name in FileDownloadList) { hashArr.Add(name); }
                return hashArr;
            }

        }
    }
}
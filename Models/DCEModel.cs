using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {
    public class UploadedTemplates {
        public String Name { get; set; }
        public String DocumentType { get; set; }
        public String VirtualPath { get; set; }
        public String DateTimeUploaded { get; set; }
        public String Description { get; set; }
        public Guid FileID { get; set; }
    }

    public class InputtedKeyword {
        public String UserName { get; set; }
        public String Keyword { get; set; }
        public Decimal Rank { get; set; }
        public Decimal GeneratedTFRate { get; set; }
        public bool AddedFlag { get; set; }
    }

    public class TemplateKeywords {
        public UploadedTemplates Template { get; set; }
        public String SkipPages { get; set; } = "";
        [Display(Name = "Template OCR Content")]
        public String TemplateOCRContent { get; set; }
        public IEnumerable<InputtedKeyword> Keywords { get; set; }
        public bool GeneratedBySystem { get; set; } = false;
    }

    public class IdentificationDocTypeCalculation {
        public String DocumentType { get; set; }
        public int KeywordMatchesAccumulation { get; set; }
        public int KeywordsTotalCount { get; set; }
        public float RankAccumulation { get; set; }
        public float RankTotal { get; set; }
        public float KeywordAppearanceRate { get { return ((KeywordMatchesAccumulation / KeywordsTotalCount) * 100); } }
        public float RankComputeRate { get { return ((RankAccumulation / RankTotal) * 100); } }
    }

    #region For Json Downloads
    public class DCETemplateKeywordsInJson {
        public String DocType { get; set; }
        public Decimal Score { get; set; }
        public IEnumerable<DCEKeysList> KeysList { get; set; }
    }

    public class DCEKeysList {
        public String Keyword { get; set; }
        public Decimal Rank { get; set; }
    }
    #endregion

    public class DocClassification {
        public String InputFilePath { get; set; }
        public String DocClassifiedOutcome { get; set; } = "";
        public String DocClassifiedMsg { get; set; } = "";
    }


}
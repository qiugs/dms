using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RMA_Docker.Classes {
    public class DCEOperations {
        
        public void InsertTemplate(String virtualPath, String docType, String description, String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.Files
                             where p.VirtualPath == virtualPath
                             select p).First();
                DCE_Templates template = new DCE_Templates();
                template.FileID = file.ID;
                template.Name = file.Name;
                template.VirtualPath = file.VirtualPath;
                template.DateTimeUploaded = DateTime.Now;
                template.Description = description;
                template.UserID = (from a in dockerEntities.aspnet_Users where a.UserName == userName select a.UserId).First();
                template.DocumentType = docType;
                try {
                    dockerEntities.DCE_Templates.Add(template);
                    dockerEntities.SaveChanges();
                } catch (DbEntityValidationException e) {
                    foreach (var eve in e.EntityValidationErrors) {
                        System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors) {
                            System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                }
            }
        }

        public void InsertOCRContents(String fileName, String fileContents, String skipPages) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                DCE_Templates dCE_Templates = (from p in dockerEntities.DCE_Templates
                                               where p.Name == fileName
                                               select p).First();
                dCE_Templates.DocumentOCRContent = fileContents;
                dCE_Templates.SkipPages = skipPages;
                dockerEntities.SaveChanges();
            }
        }

        public static String GetOCRContent(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String ocrContent = (from p in dockerEntities.DCE_Templates
                                       where p.FileID == fileID
                                       select p.DocumentOCRContent).FirstOrDefault();
                return ocrContent;
            }
        }

        public static String[] GetAllOCRContentsExcludeFileID(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String[] ocrContents = (from p in dockerEntities.DCE_Templates
                                        where p.FileID != fileID
                                        select p.DocumentOCRContent).ToArray();
                return ocrContents;
            }
        }

        public List<DCE_Templates> getAllUploadedTemplates() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<DCE_Templates> allUploadedTemplates = (from p in dockerEntities.DCE_Templates
                                                            select p).ToList();
                return allUploadedTemplates;
            }
        }

        public void DeleteUploadedTemplates(String virtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<DCE_Templates> listDCE_Template = (from p in dockerEntities.DCE_Templates
                                                        where p.VirtualPath == virtualPath
                                                        select p).ToList();
                foreach (DCE_Templates template in listDCE_Template) {
                    dockerEntities.DCE_Templates.Remove(template);
                }
                dockerEntities.SaveChanges();
            }
        }

        public void DeleteUploadedTemplateKeywords(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<DCE_Keywords> listKeywords = (from p in dockerEntities.DCE_Keywords
                                                   where p.FileID == fileID
                                                   select p).ToList();
                foreach (DCE_Keywords keyword in listKeywords) {
                    dockerEntities.DCE_Keywords.Remove(keyword);
                }
                dockerEntities.SaveChanges();
            }
        }

        public Guid GetFileIDFromTemplateByVirtualPath(String virtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                Guid fileID = (from p in dockerEntities.DCE_Templates
                               where p.VirtualPath == virtualPath
                               select p.FileID).FirstOrDefault();
                if (fileID != null) { return fileID; }
                else { return Guid.Empty; }
            }
        }

        public DCE_Templates GetTemplateByVirtualPath(String virtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                DCE_Templates template = (from p in dockerEntities.DCE_Templates
                                          where p.VirtualPath == virtualPath
                                          select p).FirstOrDefault();
                if (template != null) { return template; }
                else { return null; }
            }
        }

        public List<DCE_Templates> GetTemplatesFromDB() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<DCE_Templates> templates = (from p in dockerEntities.DCE_Templates
                                                 select p).ToList();
                return templates;
            }
        }

        public bool InsertKeyword(Guid fileID, Guid userID, String keyword, double rank) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                DCE_Keywords dCE_Keywords = (from p in dockerEntities.DCE_Keywords
                                             where p.FileID == fileID && p.Keyword == keyword && p.UserID == userID
                                             select p).FirstOrDefault();
                if (dCE_Keywords == null) {
                    dockerEntities.DCE_Keywords.Add(new DCE_Keywords() {
                        FileID = fileID,
                        UserID = userID,
                        Keyword = keyword,
                        Rank = (float)rank });
                    dockerEntities.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public bool DeleteKeyword(Guid fileID, Guid userID, String keyword) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                DCE_Keywords dCE_Keywords = (from p in dockerEntities.DCE_Keywords
                                             where p.FileID == fileID && p.Keyword == keyword
                                             select p).FirstOrDefault();
                if (dCE_Keywords != null) {
                    dockerEntities.DCE_Keywords.Remove(dCE_Keywords);
                    dockerEntities.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public List<DCE_Keywords> GetKeywordsByFileID(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<DCE_Keywords> keywords = (from p in dockerEntities.DCE_Keywords
                                               where p.FileID == fileID
                                               select p).ToList();
                if (keywords.Count > 0) { return keywords; }
                else { return null; }
            }
        }

        public List<InputtedKeyword> GetKeywordsGeneratedBySystem(UploadedTemplates template) {
            DCESystemOperations dceSysOps = new DCESystemOperations();
            return dceSysOps.GenerateKeywordsBySystem(template);
        }

        public List<InputtedKeyword> GetKeywordByInput(UploadedTemplates template, String inputKeyword) {
            return (new DCESystemOperations().GenerateKeywordByInput(template, inputKeyword));
        }

        public void UpdateRankByKeywordNFileID(Guid fileID, String keyword, double newRankVal) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                DCE_Keywords dCE_Keywords = (from p in dockerEntities.DCE_Keywords
                                             where p.FileID == fileID && p.Keyword == keyword
                                             select p).First();
                dCE_Keywords.Rank = (float)newRankVal;
                dockerEntities.SaveChanges();
            }
        }

        public int GetTotalTemplatesCount() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                int count = (from p in dockerEntities.DCE_Templates
                             select p).Count();
                return count;
            }
        }

        public List<OCRFileExtension> GetFileExtensionsAllowToOCR() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<OCRFileExtension> extList = (from p in dockerEntities.OCRFileExtensions
                                                    select p).ToList();
                return extList;
            }
        }

        public String RegexTextToString(String text) {
            Regex reg_exp = new Regex("[^a-zA-Z0-9]");
            return reg_exp.Replace(text, " ");
        }

        private static bool CheckKeywordExists(Guid fileID, String keyword) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String keywordExists = (from p in dockerEntities.DCE_Keywords
                                        where p.Keyword == keyword && p.FileID == fileID
                                        select p.Keyword).FirstOrDefault();
                if (keywordExists != null) {
                    if (keywordExists.Trim() == keyword.Trim()) { return true; }
                }
                return false;
            }
        }

        private class DCESystemOperations {

            public List<InputtedKeyword> GenerateKeywordsBySystem(UploadedTemplates template) {
                ListUniqueKeywords listUniqueKeyword = new ListUniqueKeywords();
                DictionarySpellCheck spellCheck = new DictionarySpellCheck();
                String textOCR = GetOCRContent(template.FileID);
                List<String> ListWords = listUniqueKeyword.GetSingleKeywordsFromString(textOCR);
                List<String> listSentences = listUniqueKeyword.GetShortSentencesFromString(textOCR);
                ComputeKeywordTermFrequency keywordTF = new ComputeKeywordTermFrequency(textOCR, GetAllOCRContentsExcludeFileID(template.FileID));
                List<InputtedKeyword> keywords = new List<InputtedKeyword>();
                foreach (String word in ListWords) {
                    double rate = keywordTF.GetKeywordFrequency(word);
                    if (rate >= 0.0f && (spellCheck.IsEnglishWord(word) || spellCheck.IsNumber(word))) {
                        if (word.Trim().Length < 4) { continue; }
                        keywords.Add(new InputtedKeyword {
                            Keyword = word,
                            GeneratedTFRate = GetRateRound(rate),
                            AddedFlag = CheckKeywordExists(template.FileID, word)
                        });
                    }
                }
                foreach (String line in listSentences) {
                    double rate = keywordTF.GetKeywordFrequency(line);
                    if (rate >= 0.0f && spellCheck.IsEnglishSentenceOrNumbers(line)) {
                        keywords.Add(new InputtedKeyword {
                            Keyword = line,
                            GeneratedTFRate = GetRateRound(rate),
                            AddedFlag = CheckKeywordExists(template.FileID, line)
                        });
                    }
                }
                return keywords;
            }

            public List<InputtedKeyword> GenerateKeywordByInput(UploadedTemplates template, String inputKeyword) {
                ComputeKeywordTermFrequency keywordTF = new ComputeKeywordTermFrequency(GetOCRContent(template.FileID), GetAllOCRContentsExcludeFileID(template.FileID));
                List<InputtedKeyword> keywords = new List<InputtedKeyword>();
                double rate = keywordTF.GetKeywordFrequency(inputKeyword);
                if (rate == 0.0f) { rate = 1.0f; }
                keywords.Add(new InputtedKeyword {
                    Keyword = inputKeyword,
                    GeneratedTFRate = GetRateRound(rate),
                    AddedFlag = CheckKeywordExists(template.FileID, inputKeyword)
                });
                return keywords;
            }

            private decimal GetRateRound(double rate) {
                if (rate > 0.0f) { rate = (Math.Round(rate, 2) * 100); } else { rate = 0.0f; }
                return (decimal)rate;
            }

        }


    }

    class ListUniqueKeywords {
        public List<String> GetSingleKeywordsFromString(String text) {
            text = (new DCEOperations()).RegexTextToString(text);
            String[] words = text.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            var word_query = (from String word in words
                              orderby word
                              select ToProperCase(word)).Distinct();
            return word_query.ToList();
        }

        public List<String> GetShortSentencesFromString(String text) {
            text = (new DCEOperations()).RegexTextToString(text);
            String[] sentences = Regex.Split(text, @"\s{2,}");
            var sentence_query = (from String sentence in sentences
                                  orderby sentence
                                  select FilterSentenceWithMoreThanOneSpace(ToProperCase(sentence))
                                    ).Distinct();
            return sentence_query.ToList();
        }

        private String ToProperCase(String word) {
            String result = "";
            for (int i = 1; i < word.Length; i++) {
                char ch = word[i];
                if (char.IsUpper(ch)) result += "";
                result += ch.ToString();
            }
            return result;
        }

        private String FilterSentenceWithMoreThanOneSpace(String sentence) {
            if (!sentence.Trim().Contains(" ")) { sentence = ""; }
            return sentence;
        }
    }

    class ComputeKeywordTermFrequency {
        private List<String> contentsBase = new List<String>();
        private String thisContent = String.Empty;

        public ComputeKeywordTermFrequency() { }
        public ComputeKeywordTermFrequency(String currentDocument, String[] docuementBase) {
            UploadDocumentBase(currentDocument, docuementBase);
        }

        public void UploadDocumentBase(String currentDocument, String[] docuementBase) {
            thisContent = (new DCEOperations()).RegexTextToString(currentDocument);
            //thisContent = currentDocument;
            foreach (String content in docuementBase) {
                String docTextAfterRegex = (new DCEOperations()).RegexTextToString(content);
                if (docTextAfterRegex.Trim().Length > 0) { contentsBase.Add(docTextAfterRegex); }
            }
        }

        public double GetKeywordFrequency(String keyword) {
            double IDFScore = Math.Log((contentsBase.Count + 1) / (ComputeKeywordDFRateOnDocuments(keyword) + 1));
            double retScore = ((ComputeKeywordTFRateOnThisDocument(keyword) * IDFScore) / (contentsBase.Count + 1));
            if (Double.IsNaN(retScore) || Double.IsInfinity(retScore)) { return 0.0f; }
            return retScore;
        }

        //TF = frequency of keyword in Document
        private int ComputeKeywordTFRateOnThisDocument(String keyword) {
            int TFCount = 0;
            TFCount += Regex.Matches(thisContent, keyword).Count;
            return TFCount;
        }

        //DF = number of documents containing keyword
        private int ComputeKeywordDFRateOnDocuments(String keyword) {
            int DFCount = 0;
            foreach (String content in contentsBase) { DFCount += Regex.Matches(content, keyword).Count; }
            return DFCount;
        }

    }

    class DictionarySpellCheck {
        private NetSpell.SpellChecker.Spelling spellChecker;
        private NetSpell.SpellChecker.Dictionary.WordDictionary wordDictionary;

        public DictionarySpellCheck() {
            this.wordDictionary = new NetSpell.SpellChecker.Dictionary.WordDictionary();
            this.wordDictionary.DictionaryFile = GetServerMapPath("DictionarySources\\en-US.dic");
            this.wordDictionary.Initialize();
            this.spellChecker = new NetSpell.SpellChecker.Spelling();
            this.spellChecker.Dictionary = this.wordDictionary;
        }

        public bool IsEnglishWord(String wordToCheck) {
            return this.spellChecker.TestWord(wordToCheck);
        }

        public bool IsEnglishSentence(String sentencesToCheck) {
            bool sentenceCheck = true;
            String[] lineWords = sentencesToCheck.Split(' ');
            foreach (String word in lineWords) {if (!this.spellChecker.TestWord(word)) { sentenceCheck = false; }}
            return sentenceCheck;
        }

        public bool IsEnglishSentenceOrNumbers(String sentencesToCheck) {
            bool sentenceCheck = true;
            String[] lineWords = sentencesToCheck.Split(' ');
            foreach (String word in lineWords) { if (!IsEnglishWord(word) && !IsNumber(word)) { sentenceCheck = false; }}
            return sentenceCheck;
        }

        public bool IsNumber(String inputToCheck) {
            return int.TryParse(inputToCheck, out int result);
        }

        private static String GetServerMapPath(String path) {
            String currentPath = (HttpContext.Current.Server.MapPath(path)).Replace("RMA_DCE", "DockerFiles");
            return currentPath;
        }

    }

}
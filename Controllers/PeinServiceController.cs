using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using Sprep.Soo.Pein.Bl;
using Sprep.Soo.Pein.Bl.Common;

namespace SPREPREGIONAL.Web.Controllers {
    public class PeinServiceController : ApiController {

        protected enum IdType {
            Rid,
            Id
        }

        #region Main

        /// <summary>
        /// Get PEIN Items (only 10 Sample records. Useful for testing)
        /// </summary>
        /// <returns>SearchResult containing a list of results and additional information</returns>
        public SearchResult GetAll() {
            var sr = BL.GetPeinItemBySearch("", 0, 10);
            sr.Info.Text = "Get PEIN Items (only 10 Sample records with all available fields. Useful for testing)";

            return sr;
        }

        /// <summary>
        /// PEIN Catalogue Full-Text search. Accepts AND OR statements. See Presto User Guide for full searching wildcards
        /// </summary>
        /// <param name="q">Search query. Type "all" to find all available records</param>
        /// <param name="start">Display from row</param>
        /// <param name="end">Finish displaying at row</param>
        /// <returns></returns>
        public SearchResult GetBySearch(string q, int start, int end) {

            if (q == "all")
                q = string.Empty;

            var sr = BL.GetPeinItemBySearch(q, start, end);
            //sr.Info.Text = "Use the {controller}/find/all to get all records. <br/>" +
            //               "Use the {controller}/find/all/10/20 to get all records from row 10 to row 20. ";

            return sr;
        }

        /// <summary>
        /// PEIN Catalogue Full-Text search. Accepts AND OR statements. See Presto User Guide for full searching wildcards
        /// 
        /// Returns only ContentItemIds (useful for paging)
        /// </summary>
        /// <param name="q">Search query. Type "all" to find all available records</param>
        /// <returns></returns>
        public SearchResult GetIdsBySearch(string q) {

            if (q == "all")
                q = string.Empty;

            var ss = BL.GetPeinIdsBySearch(q);
            SearchResult sr = new SearchResult { Info = new SearchResult.ResultInfo() };

            if (ss.Any()) {
                //dump int array into list<peininfo>
                sr.Result = (from x in ss
                             select new PeinInfo {
                                 ContentItemId = x
                             }).ToList();

                int count = ss.Count();
                sr.Info = new SearchResult.ResultInfo{
                    Showing = count,
                    StartRow = 0,
                    EndRow = count,
                    TotalFound = count
                };
            }

            return sr;
        }

        /// <summary>
        /// Get PEIN item by Content Item Id
        /// </summary>
        /// <param name="id">Content Item Id</param>
        /// <returns>PeinInfo</returns>
        public PeinInfo GetById(int id) {
            var sr = RetByIds(new int[] { id });

            if (sr.Result.Any())
                return sr.Result.SingleOrDefault();

            return new PeinInfo();
        }

        /// <summary>
        /// Get PEIN items either by Content Item Id or Record Id
        /// </summary>
        /// <param name="ids">array of ids (json eg [1,2,3])</param>
        /// <param name="type">Use 'id' for Content Item Id OR 'rid' for Record Id</param>
        /// <returns></returns>
        public SearchResult RetByIds(int[] ids, string type) {
            switch (type) {
                case "id":
                    return RetByIds(ids);
                case "rid":
                    //default:
                    return RetByRecordIds(ids);
            }

            return new SearchResult { Info = new SearchResult.ResultInfo { Text = "No items found" } };
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Get PEIN items by Array of Content Item Ids
        /// </summary>
        /// <param name="ids">Array of Content Item Ids to retrieve (json eg [1,2,3])</param>
        /// <returns>SearchResult</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected SearchResult RetByIds(int[] ids) {
            return RetByIdsLogic(ids, IdType.Id);
        }

        /// <summary>
        /// Get Pein Items by Array of Record Ids
        /// </summary>
        /// <param name="rids"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected SearchResult RetByRecordIds(int[] rids) {
            return RetByIdsLogic(rids, IdType.Rid);
        }

        /// <summary>
        /// Logic for Get Items by Id
        /// </summary>
        /// <param name="ids">ids to retrieve</param>
        /// <param name="type">type of item</param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected SearchResult RetByIdsLogic(int[] ids, IdType type) {

            if (ids == null) {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }

            List<PeinInfo> rr = new List<PeinInfo>();

            if (type == IdType.Id)
                rr = BL.GetPeinItemByIds(ids);
            else
                rr = BL.GetPeinItemByRecordIds(ids);

            var sr = new SearchResult {
                Result = rr
            };

            if (rr.Any()) {
                int count = rr.Count();
                sr.Info = new SearchResult.ResultInfo {
                    TotalFound = count,
                    Showing = count,
                    StartRow = 0,
                    EndRow = count
                };
            } else {
                sr.Info = new SearchResult.ResultInfo { Text = "No matches found" };
            }

            return sr;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using Sprep.Soo.Pein.Bl;
using Sprep.Soo.Pein.Bl.Common;
using SPREPREGIONAL.Web.Components;

namespace SPREPREGIONAL.Web.Controllers {
    public class PeinServiceController : ApiController {

        public const string NO_RESULTS = "No results found";

        protected enum IdType {
            Rid,
            Id
        }

        static public string APP_URL {
            get { return System.Configuration.ConfigurationManager.AppSettings["AppUrl"]; }
            //set { _API_CALLER_USERNAME = value; }
        }

        #region Main

        /// <summary>
        /// Get PEIN Items (only 10 Sample records. Useful for testing)
        /// </summary>
        /// <returns>SearchResult containing a list of results and additional information</returns>
        public IWebApiResponse GetAll() {
            var sr = BL.GetPeinItemBySearch("", 0, 10);
            sr.Info.Text = "Get PEIN Items (only 10 Sample records with all available fields. Useful for testing)";

            var response = new WebApiResponse<SearchResult>(sr);
            response.ApiSuccess = true;
            return response;
        }

        /// <summary>
        /// PEIN Catalogue Full-Text search. Accepts AND OR statements. See Presto User Guide for full searching wildcards
        /// </summary>
        /// <param name="q">Search query. Type "all" to find all available records</param>
        /// <param name="pageNo">Page Number</param>
        /// <param name="itemCount">Number of items per page</param>
        /// <returns></returns>
        public IWebApiResponse GetBySearch(string q, int pageNo, int itemCount) {
            var response = new WebApiResponse<SearchResult>(new SearchResult());
            if (q == "all")
                q = string.Empty;

            try {
                var sr = BL.GetPeinItemBySearch(q, pageNo, itemCount);
                sr.Result = FormatDetailUrl(sr.Result);
                response = new WebApiResponse<SearchResult>(sr);
            } catch (Exception ex) {
                response = new WebApiResponse<SearchResult>("", ex.Message, null);
            }

            return response;
        }

        ///// <summary>
        ///// PEIN Catalogue Full-Text search. Accepts AND OR statements. See Presto User Guide for full searching wildcards
        ///// 
        ///// Returns only ContentItemIds (useful for paging)
        ///// </summary>
        ///// <param name="q">Search query. Type "all" to find all available records</param>
        ///// <returns></returns>
        //public IWebApiResponse GetIdsBySearch(string q) {
        //    var response = new WebApiResponse<SearchResult>(new SearchResult());

        //    if (q == "all")
        //        q = string.Empty;

        //    try {

        //        var ss = BL.GetPeinIdsBySearch(q);
        //        SearchResult sr = new SearchResult { Info = new SearchResult.ResultInfo() };

        //        if (ss.Any()) {
        //            //dump int array into list<peininfo>
        //            sr.Result = (from x in ss
        //                         select new PeinInfo {
        //                             ContentItemId = x
        //                         }).ToList();

        //            int count = ss.Count();
        //            sr.Info = new SearchResult.ResultInfo {
        //                Showing = count,
        //                StartRow = 0,
        //                EndRow = count,
        //                TotalFound = count
        //            };

        //            response = new WebApiResponse<SearchResult>(sr);
        //        } else {
        //            response.ApiError = NO_RESULTS;
        //        }
        //    } catch (Exception ex) {

        //        response = new WebApiResponse<SearchResult>("", ex.Message, new SearchResult());
        //    }

        //    return response;
        //}

        /// <summary>
        /// Get PEIN item by Content Item Id
        /// </summary>
        /// <param name="id">Content Item Id</param>
        /// <returns>PeinInfo</returns>
        public IWebApiResponse GetByContentId(int id) {

            var response = new WebApiResponse<PeinInfo>(new PeinInfo());

            var sr = RetByIds(new int[] { id }) as WebApiResponse<SearchResult>;

            if (sr.ResponseValue.Result.Any()) {
                var r = sr.ResponseValue.Result.SingleOrDefault();
                response = new WebApiResponse<PeinInfo>(r);
            } else {
                response.ApiError = NO_RESULTS;
            }

            return response;
        }

        /// <summary>
        /// Get PEIN items either by Content Item Id or Record Id
        /// </summary>
        /// <param name="ids">array of ids (json eg 1|2|3)</param>
        /// <param name="idType">Use 'id' for Content Item Id OR 'rid' for Record Id</param>
        /// <returns></returns>
        public IWebApiResponse GetByIds(string ids, string idType)
        {
            var int32Ids = ids.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToArray();

            switch (idType) {
                case "id":
                    return RetByIds(int32Ids);
                case "rid":
                    //default:
                    return RetByRecordIds(int32Ids);
            }

            var r = new SearchResult { Info = new SearchResult.ResultInfo { Text = "No items found" } };
            return new WebApiResponse<SearchResult>(r);
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Get PEIN items by Array of Content Item Ids
        /// </summary>
        /// <param name="ids">Array of Content Item Ids to retrieve (json eg [1,2,3])</param>
        /// <returns>SearchResult</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected IWebApiResponse RetByIds(int[] ids) {
            return RetByIdsLogic(ids, IdType.Id);
        }

        /// <summary>
        /// Get Pein Items by Array of Record Ids
        /// </summary>
        /// <param name="rids"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected IWebApiResponse RetByRecordIds(int[] rids) {
            return RetByIdsLogic(rids, IdType.Rid);
        }

        /// <summary>
        /// Logic for Get Items by Id
        /// </summary>
        /// <param name="ids">ids to retrieve</param>
        /// <param name="type">idType of item</param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected IWebApiResponse RetByIdsLogic(int[] ids, IdType type) {
            var response = new WebApiResponse<SearchResult>(new SearchResult());

            try {

                if (ids == null) {
                    throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                }

                List<PeinInfo> rr = new List<PeinInfo>();

                if (type == IdType.Id)
                    rr = BL.GetPeinItemByIds(ids);
                else
                    rr = BL.GetPeinItemByRecordIds(ids);

                var sr = new SearchResult {
                    Result = FormatDetailUrl(rr)
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

                response = new WebApiResponse<SearchResult>(sr);
            } catch (Exception ex) {
                response.ApiSuccess = false;
                response.ApiStackTrace = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Format the Detail Url properly
        /// </summary>
        /// <param name="pList"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected List<PeinInfo> FormatDetailUrl(List<PeinInfo> pList)
        {
            var result = new List<PeinInfo>();
            foreach (var p in pList)
            {
                p.DetailUrl = string.Format("{0}{1}", APP_URL, p.DetailUrl);
                result.Add(p);
            }

            return result;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPREPREGIONAL.Web.Components {
    public class WebApiResponse<T> : IWebApiResponse {

        private bool _success;
        private string _apiError;
        private string _apiStackTrace;
        private T _responseValue;
        /// <summary>
        /// Constructor for successful save
        /// </summary>
        /// <param name="responseValue">The object or object graph to be returned in the response</param>
        public WebApiResponse(T responseValue) {
            _responseValue = responseValue;
            _success = true;
        }
        /// <summary>
        /// Constructor for failed save
        /// </summary>
        /// <param name="apiError"></param>
        /// <param name="apiStackTrace"></param>
        /// <param name="responseValue"></param>
        public WebApiResponse(string apiError, string apiStackTrace, T responseValue) {
            _success = false;
            _apiError = apiError;
            _apiStackTrace = apiStackTrace;
            _responseValue = responseValue;
        }

        public T ResponseValue {
            get {
                return _responseValue;
            }
        }

        #region interface properties
        public bool ApiSuccess {
            get {
                return _success;
            }
            set {
                _success = value;
            }
        }

        public string ApiError {
            get {
                return _apiError;
            }
            set {
                _apiError = value;
            }
        }

        public string ApiStackTrace {
            get {
                return _apiStackTrace;
            }
            set {
                _apiStackTrace = value;
            }
        }
        #endregion
    }
}
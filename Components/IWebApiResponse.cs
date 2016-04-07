using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPREPREGIONAL.Web.Components {
    public interface IWebApiResponse {
        bool ApiSuccess { get; set; }
        string ApiError { get; set; }
        string ApiStackTrace { get; set; }
    }
}

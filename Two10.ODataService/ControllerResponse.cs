using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Two10.ODataService
{
    class ControllerResponse
    {
        public ControllerResponse()
        { }

        public ControllerResponse(string view, dynamic model)
        {
            this.View = view;
            this.Model = model;
            this.ContentType = "text/html";
        }

        public ControllerResponse(string view, dynamic model, string contentType)
        {
            this.View = view;
            this.Model = model;
            this.ContentType = contentType;
        }

        public string View { get; set; }

        public dynamic Model { get; set; }

        public string ContentType { get; set; }
        
    }
}

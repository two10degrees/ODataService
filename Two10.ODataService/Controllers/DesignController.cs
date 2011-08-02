using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data;
using System.Dynamic;

namespace Two10.ODataService.Controllers
{
    class DesignController : IController
    {

        // Expecting: /Design/{Action}/{Id}
        public ControllerResponse Execute(ControllerRequest request)
        {
            if (request.Path.Length < 2)
            {
                return Index();
            }

            int segmentId = 0;
            if (request.Path.Length >= 3)
            {
                segmentId = int.Parse(request.Path[2]);
            }

            switch (request.Path[1])
            { 
                case "Edit":
                    return Edit(segmentId);
                case "Delete":
                    return Delete(segmentId);
                case "EditPostback":
                    return EditPostback(request);
                case "New":
                    return New();
            }

            return Index();
        }


        private ControllerResponse Index()
        {
            var model = Database.Open().Segment.All().ToArray();
            return new ControllerResponse("Default", model);
        }

        private ControllerResponse New()
        {
            dynamic model = new ExpandoObject();
            model.SegmentId = 0;
            model.Name = string.Empty;
            model.ConnectionString = string.Empty;
            model.Query = string.Empty;
            return new ControllerResponse("Edit", model);
        }

        private ControllerResponse Delete(int segmentId)
        {
            Database.Open().Segment.DeleteBySegmentId(segmentId);
            return Index();
        }

        private ControllerResponse EditPostback(ControllerRequest request)
        {
            if (request.Query["SegmentId"] == "0")
            {
                Database.Open().Segment.Insert(
                    Name: request.Query["Name"],
                    ConnectionString: request.Query["ConnectionString"],
                    Query: request.Query["Query"]);

                return Index();
            }

            Database.Open().Segment.UpdateBySegmentId(
                SegmentId: int.Parse(request.Query["SegmentId"]), 
                Name: request.Query["Name"], 
                ConnectionString: request.Query["ConnectionString"], 
                Query: request.Query["Query"]);

            return Index();
        }

        private ControllerResponse Edit(int segmentId)
        {
            var model = Database.Open().Segment.FindBySegmentId(segmentId);
            return new ControllerResponse("Edit", model);
        }

    }
}

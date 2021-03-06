﻿using System;
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

        private bool CheckName(string name, int segmentId, dynamic database)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            dynamic[] match = (database.Segment.FindAllByName(name)).ToArray();
            return !(from s in match where s.SegmentId != segmentId select s).Any();
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
            dynamic database = Database.Open();

            if (!this.CheckName(request.Query["Name"], int.Parse(request.Query["SegmentId"]), database))
            {
                dynamic model = new ExpandoObject();
                model.SegmentId = int.Parse(request.Query["SegmentId"]);
                model.Name = request.Query["Name"];
                model.ConnectionString = request.Query["ConnectionString"];
                model.Query = request.Query["Query"];
                if (string.IsNullOrWhiteSpace(request.Query["Name"]))
                {
                    model.Message = "Please enter a name for this data service";
                }
                else
                {
                    model.Message = "Please choose a different name, the name is already in use";
                }
                return new ControllerResponse("Edit", model);
            }

            if (request.Query["SegmentId"] == "0")
            {
                database.Segment.Insert(
                    Name: request.Query["Name"],
                    ConnectionString: request.Query["ConnectionString"],
                    Query: request.Query["Query"]);

                return Index();
            }

            database.Segment.UpdateBySegmentId(
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace Two10.ODataService.Controllers
{
    class DataController : IController
    {
        // /Data/{SegmentName(Id)}/Property/$value/$count?$filter=a lt b
        public ControllerResponse Execute(ControllerRequest request)
        {
            if (request.Path.Length <= 1)
            {
                return Collections();
            }

            var items = request.Path[1].Split('(', ')');
            string segmentName = items[0];
            string id = items.Length > 1 ? items[1] : null;

            var db = Database.Open();
            var segment = db.Segment.FindByName(segmentName);
            if (null == segment)
            {
                return null;
            }

            return Index(segment, id, request);
        }


        public ControllerResponse Index(dynamic segment, string id, ControllerRequest request)
        {
            dynamic model = new ExpandoObject();
            model.Name = segment.Name;
            //model.BaseAddress = .Url.OriginalString.Replace(this.Request.Url.PathAndQuery, string.Empty);

            if (null == segment)
            {
                return null;
            }

            Tuple<int, string> pkInfo = GetPK(segment);
            model.PKIndex = pkInfo.Item1;
            model.BaseAddress = Program.BaseAddress;


            string fields = "*";
            string orderby = null;
            int top = -1;
            bool json = false;
            List<Tuple<string, string, string>> where = new List<Tuple<string, string, string>>();
            string view = null;
            bool singleResult = false;



            foreach (string key in request.Query.Keys)
            {
                string value = request.Query[key] ?? "";
                switch (key.ToLower().Trim())
                {
                    
                    case "$filter":
                        string[] whereString = value.Split(',');
                        foreach (string str in whereString)
                        {
                            string[] whereItems = str.Split(' ');
                            if (whereItems.Length == 3)
                            {
                                where.Add(new Tuple<string, string, string>(whereItems[0].Trim(), whereItems[1].Trim(), whereItems[2].Trim()));
                            }
                        }
                        break;

                    case "$format":
                        json = (0 == string.Compare(value, "json", true));
                        break;
                    case "$inlinecount": { break; }
                    case "$orderby":
                        orderby = value;
                        break;
                    case "$select":
                        fields = value;
                        break;
                    case "$skip": { break; }
                    case "$skiptoken": { break; }
                    case "$top":
                        int.TryParse(value, out top);
                        break;
                }
            }
            if (null != id)
            {
                view = "Entry";
                where.Add(new Tuple<string, string, string>(pkInfo.Item2, "eq", id));
                singleResult = true;

                string property = (from p in request.Path.Skip(2) where !p.StartsWith("$") select p).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(property))
                {
                    // the name of a property has been passed in
                    view = request.Path.Contains("$value") ? "Value" : "Property";
                    fields = property;
                }
            }
            else
            {
                view = "Collection";
            }

            if (request.Path.Contains("$count"))
            {
                fields = "count(*) as count";
                singleResult = true;
                view = "Value";
            }

            string query = DatabaseUtils.SelectWhere(segment.Query, top, fields, orderby, where.ToArray());

            if (request.Query.Keys.Contains("$sql"))
            {
                model.Value = query;
                return new ControllerResponse("Value", model, "text/plain");
            }

            model.Reader = DatabaseUtils.ExecuteReader(query, segment.ConnectionString);
            if (singleResult)
            {
                (model.Reader as SqlDataReader).Read();
            }

            if ("Value" == view)
            {
                model.Value = (model.Reader as SqlDataReader).GetValue(0);
                return new ControllerResponse("Value", model, "text/plain");
            }
            else
            {
                return new ControllerResponse(view, model, "application/rss+xml");
            }

        }

        public ControllerResponse Collections()
        {
            dynamic model = new ExpandoObject();
            var db = Database.Open();
            model.Segments = db.Segment.All().ToArray();
            model.BaseAddress = Program.BaseAddress;
            return new ControllerResponse("Collections", model, "application/rss+xml");
        }

        public static Tuple<int, string> GetPK(dynamic segment)
        {
            using (var emptyRecords = DatabaseUtils.ExecuteReader(DatabaseUtils.SelectTop(0, segment.Query), segment.ConnectionString))
            {
                return DatabaseUtils.GetPK(emptyRecords);
            }
        }
    }
}

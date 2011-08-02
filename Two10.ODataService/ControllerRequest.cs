using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Two10.ODataService
{
    class ControllerRequest
    {
        public Dictionary<string, string> Query { get; set; }

        public string[] Path { get; set; }

    }
}

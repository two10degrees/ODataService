using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Two10.ODataService
{
    interface IController
    {
        ControllerResponse Execute(ControllerRequest request);
    }
}

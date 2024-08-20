using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEETEK_EMS_DB;
using SEETEK_EMS_DB.Models;
using System.Collections.Specialized;

namespace Seetek_EMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectOperationController : ControllerBase
    {
        [HttpPost("GetRoutingMap")]
        public IActionResult GetRoutingMap()
        {

            using (var context = new DBContext())
            {

                var routings = context.RoutingMaps.Select(routing => routing.Name).ToList().Distinct();
                return Ok(routings);

            }
        }


        [HttpPost("GetResources")]
        public IActionResult GetResoures([FromBody] ResourceRequest resourceRequest)
        {

            using (var context = new DBContext())
            {

                var resources = context.RoutingMaps
                     .Join(context.ResourceGroups,
                     routing => routing.ResourceGroupID,
                     resource => resource.ID,
                     (routing, resource) => new { Resource = resource, Routing = routing })
                     .Where(joined => joined.Routing.Name.Equals(resourceRequest.RoutingMapName))
                     .Select(joined => joined.Resource.Name)
                     .ToList();

                return Ok(resources);
            }
        }



        [HttpPost("GetStation")]
        public IActionResult GetStation([FromBody] StationRequest request)
        {

            using (var context = new DBContext())
            {

                var resources = context.RoutingMaps
                    .Join(context.ResourceGroups,
                    routing => routing.ResourceGroupID,
                    resource => resource.ID,
                    (routing, resource) => new { Resource = resource, Routing = routing })
                    .Join(context.Stations,
                    combined => combined.Routing.StationID,
                    station => station.ID,
                    (combined, station) => new { combined, station })
                    .Where(final => final.combined.Routing.Name.Equals(request.RoutingMapName)
                    && final.combined.Resource.Name.Equals(request.ResourceGroupName))
                    .Select(final => final.station.Name).Distinct().ToList();


                return Ok(resources);
            }
        }

        [HttpPost("GetOperation")]
        public IActionResult GetOperation([FromBody] OperationRequest request)
        {
            using (var context = new DBContext())
            {

                var resources = context.RoutingMaps
                    .Join(context.ResourceGroups,
                    routing => routing.ResourceGroupID,
                    resource => resource.ID,
                    (routing, resource) => new { Resource = resource, Routing = routing })
                    .Join(context.Stations,
                    combined1 => combined1.Routing.StationID,
                    station => station.ID,
                    (combined1, station) => new { combined1, station })
                    .Join(context.Operations,
                    combined2 => combined2.combined1.Routing.OperationID,
                    operation => operation.ID,
                    (combined2, operation) => new { combined2, operation })
                    .Where(final => final.combined2.combined1.Routing.Name.Equals(request.RoutingMapName)
                    && final.combined2.combined1.Resource.Name.Equals(request.ResourceGroupName)
                    && final.combined2.station.Name.Equals(request.StationName))
                    .Select(final => final.operation.Name).Distinct().ToList();

                return Ok(resources);



            }

        }

    }




    public class ResourceRequest
    {
        public string RoutingMapName { get; set; }
    }
    public class StationRequest
    {
        public string RoutingMapName { get; set; }

        public string ResourceGroupName { get; set; }



    }

    public class OperationRequest
    {
        public string RoutingMapName { get; set; }

        public string ResourceGroupName { get; set; }
        public string StationName  { get; set; }
    }
}

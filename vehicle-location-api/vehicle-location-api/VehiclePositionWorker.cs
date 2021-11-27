using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using ProtoBuf;
using TransitRealtime;
using vehicle_location_api.protos.V1;

namespace vehicle_location_api
{
    public class VehiclePositionWorker : BackgroundService
    {
        private readonly IBus _bus;

        public VehiclePositionWorker(IBus bus)
        {
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var request = WebRequest.Create("http://track.ua-gis.com/gtfs/lviv/vehicle_position");
                var feed = Serializer.Deserialize<FeedMessage>((await request.GetResponseAsync()).GetResponseStream());

                List<VehicleLocationEvent> events = feed.Entities.Select(ToVehicleLocationEvent).ToList();

                var tasks = events.Select(e => _bus.Send(e, stoppingToken));

                await Task.WhenAll(tasks);
                
                await Task.Delay(1000, stoppingToken);
            }
        }

        private static VehicleLocationEvent ToVehicleLocationEvent(FeedEntity entity)
        {
            var vp = entity.Vehicle;
            var gpsEvent = new VehicleLocationEvent
            {
                Timestamp = DateTime.UtcNow,
                Bearing = vp.Position.Bearing,
                Latitude = vp.Position.Latitude,
                Longitude = vp.Position.Longitude,
                Speed = vp.Position.Speed,
                routeId = vp.Trip.RouteId,
                licensePlateNumber = vp.Vehicle.LicensePlate
            };

            return gpsEvent;
        }
    }
}
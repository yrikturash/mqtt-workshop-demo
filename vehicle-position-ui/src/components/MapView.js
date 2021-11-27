import React, { useState } from "react";
import { MapContainer, TileLayer } from "react-leaflet";
import MqttConnector from "./MqttConnector";
import RealtimeComponent from "./RealTime";
import "leaflet/dist/leaflet.css";

const lvivCenterPosition = { lat: 49.83457901256914, lng: 24.028424156611234 };
const MQTT_BROKER = "ws://localhost:15675/ws";
const selectedRouteId = "1004";

const MapView = (props) => {
  const [state, setState] = useState({
    currentLocation: lvivCenterPosition,
    zoom: 13,
  });

  return (
    <MapContainer center={state.currentLocation} zoom={state.zoom}>
      <TileLayer
        url="https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}"
        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        id="mapbox/streets-v11"
        accessToken="pk.eyJ1IjoiZGFudGVkcmVhbWVyIiwiYSI6ImNrcXIyNGpsajBrbTgybnEzazc5Y3RnNWgifQ.4YQxTUFcsxT3GjlX8-nuzg"
      />

      <MqttConnector brokerUrl={MQTT_BROKER}>
        <RealtimeComponent routeId={selectedRouteId} />
      </MqttConnector>
    </MapContainer>
  );
};

export default MapView;

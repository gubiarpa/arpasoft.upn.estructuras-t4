﻿using arpasoft.maps_calculator.infrastructure.Services;
using arpasoft.maps_calculator.winforms.Utils;

namespace arpasoft.maps_calculator.winforms.Map
{
    public class MapCoordinateService : MapService<Coordinate>
    {
        public override int? GetNodeIdByValue(Coordinate data, int error)
        {
            var matchNode = GetAllNodes()?.FirstOrDefault(node => node.IsNear(data, error));
            return matchNode?.ID;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Sledge.BspEditor.Grid;
using Sledge.Common.Transport;

namespace Sledge.BspEditor.Primitives.MapData
{
    public class GridData : IMapData
    {
        public bool SnapToGrid { get; set; }
        public IGrid Grid { get; set; }

        public GridData(IGrid grid)
        {
            Grid = grid;
            SnapToGrid = true;
        }

        public GridData(SerialisedObject obj)
        {
            SnapToGrid = obj.Get<bool>("SnapToGrid");
        }

        [Export(typeof(IMapElementFormatter))]
        public class GridDataFormatter : StandardMapElementFormatter<GridData> { }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Meh
        }

        public IMapData Clone()
        {
            return new GridData(Grid) {SnapToGrid = SnapToGrid};
        }

        public SerialisedObject ToSerialisedObject()
        {
            var so = new SerialisedObject("Grid");
            so.Set("SnapToGrid", SnapToGrid);
            return so;
        }
    }
}

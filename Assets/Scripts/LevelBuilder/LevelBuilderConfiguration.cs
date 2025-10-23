using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GridEditor
{
    public class LevelBuilderConfiguration
    {
        public int DimensionX, DimensionY;
        public string TilesFolder, PropsFolder, OriginName;
        public Vector3 Origin;
        public float TileSize;
        public LevelBuilderConfiguration() { }//for deserilization
        public LevelBuilderConfiguration(LevelBuilder levelBuilder) {
            DimensionX = levelBuilder.sldrDimensionX.value;
            DimensionY = levelBuilder.sldrDimensionY.value;
            TilesFolder = levelBuilder.txtTilesFolder.text;
            PropsFolder = levelBuilder.txtPropsFolder.text;
            Origin = levelBuilder.v3OriginLocation.value;
            OriginName = levelBuilder.txtOriginName.text;
            TileSize = levelBuilder.sldrTileSize.value;
        }
        public void ConfigureLevelBuilder(LevelBuilder levelBuilder) {
            levelBuilder.sldrDimensionX.value = DimensionX;
            levelBuilder.sldrDimensionY.value = DimensionY;
            levelBuilder.txtTilesFolder.value = TilesFolder;
            levelBuilder.txtPropsFolder.value = PropsFolder;
            levelBuilder.v3OriginLocation.value = Origin;
            levelBuilder.txtOriginName.value = OriginName;
            levelBuilder.sldrTileSize.value = TileSize;
        }
    }
}

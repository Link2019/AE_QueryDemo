using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE_QueryDemo
{
    class Global
    {
        public static string MxdPath = Application.StartupPath + @"\data\北部湾.mxd";
        public static string GdbPath = Application.StartupPath + @"\data\gdp.mdb";
        // public static string GDirectory = Application.StartupPath + @"\data";

        public static IMap pOriginalMap;
        public static IWorkspace pWorkspace1;
        public static IWorkspace pWorkspace2;

        public static DataGridView myDGV1 = null;
      //  public static List<string> CountyName;
        //public static ITopology GlobalTopology;


    }
}

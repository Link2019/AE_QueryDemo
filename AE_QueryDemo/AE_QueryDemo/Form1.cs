using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;

namespace AE_QueryDemo
{
    public partial class Form1 : Form
    {
        //用于判断空间查询的状态
        bool IsSpatialSearch = false;

        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Load主函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnSptialSearch.Visible = false;
            OpenMxd();
            Global.myDGV1 = dataGridView1;
            OpenGDB();
            Global.myDGV1.DataSource = frmAttribute.Search((axMapControl1.get_Layer(Get_Layer("北部湾")) as IFeatureLayer).FeatureClass, "");
        }
        /// <summary>
        /// 获得精确图层名下的index
        /// </summary>
        /// <param name="LayerName">图层名字</param>
        /// <returns></returns>
        private int Get_Layer(string LayerName)
        {
            //遍历主视图的图层
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                //如果图层索引对应的名字和用户输入的名字相同则返回索引
                if (axMapControl1.get_Layer(i).Name.Equals(LayerName))
                {
                    return i;
                }
            }
            return -1;//返回-1
        }

        /// <summary>
        /// 打开数据
        /// </summary>
        private void OpenGDB()
        {
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory() as IWorkspaceFactory;
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(Global.GdbPath, 0) as IWorkspace;
            Global.pWorkspace1 = pWorkspace;
        }

        /// <summary>
        /// 打开Mxd文档的方法
        /// </summary>
        private void OpenMxd()
        {
            try
            {
                IMapDocument pMapDocument = new MapDocumentClass();

                string filePath = Global.MxdPath;

                pMapDocument.Open(filePath, "");

                IMap pMap = pMapDocument.get_Map(0);
                //arcgis10若使用GDB保存mxd文件的图层要素类，在图层对应的要素类路径使用相对路径存放，无需再重新赋值
                Global.pOriginalMap = pMap;

                axMapControl1.Map = pMap;
                axMapControl1.Refresh();
                axTOCControl1.SetBuddyControl(axMapControl1.Object);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 打开属性查询窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLabel3_Click(object sender, EventArgs e)
        {
            frmAttribute frm = new frmAttribute();
            frm.Show();
        }

        private void toolStripLabel4_Click(object sender, EventArgs e)
        {
            IsSpatialSearch = true;
            btnSptialSearch.Visible = true;
        }
        /// <summary>
        /// 主视图的OnMouseDown事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axMapControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            //当空间查询的状态为真时
            if (IsSpatialSearch)
            {
                //获取精确图层
                ILayer pLayer = axMapControl1.get_Layer(Get_Layer("北部湾"));
                //将图层强转成要素图层
                IFeatureLayer pFtLayer = pLayer as IFeatureLayer;
                //将要素图层的图层类强转成要素类
                IFeatureClass pFtClass = pFtLayer.FeatureClass as IFeatureClass;
                //随着鼠标拖动得到一个矩形框
                IEnvelope pEnvelope = axMapControl1.TrackRectangle();
                //调用核心空间查询函数（采用空间相交的方法esriSpatialRelIntersects）
                dataGridView1.DataSource = SpatialSearch(pFtClass,"",
                    pEnvelope,esriSpatialRelEnum.esriSpatialRelIntersects);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }
        /// <summary>
        /// 核心空间查询函数
        /// </summary>
        /// <param name="pFtClass">查询要素类</param>
        /// <param name="pWhereClause">SQL语句</param>
        /// <param name="pGeometry">空间查询范围</param>
        /// <param name="pSpRel">空间关系</param>
        /// <returns></returns>
        private DataTable SpatialSearch(IFeatureClass pFtClass, string pWhereClause, IGeometry pGeometry, esriSpatialRelEnum pSpRel)
        {
            //定义空间查询过滤器对象
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            //设置sql查询语句
            pSpatialFilter.WhereClause = pWhereClause;
            //设置查询范围
            pSpatialFilter.Geometry = pGeometry;
            //给定范围与查询对象的空间关系
            pSpatialFilter.SpatialRel = pSpRel;

            //查询结果以游标的形式返回(下面与属性查询一样)
            IFeatureCursor pFtCursor = pFtClass.Search(pSpatialFilter, false);
            IFeature pFt = pFtCursor.NextFeature();
            DataTable DT = new DataTable();
            for (int i = 0; i < pFtCursor.Fields.FieldCount; i++)
            {
                DataColumn dc = new DataColumn(pFtCursor.Fields.get_Field(i).Name,
                    System.Type.GetType(ParseFieldType((pFtCursor.Fields.get_Field(i).Type))));
                DT.Columns.Add(dc);
            }
            while (pFt != null)
            {
                DataRow dr = DT.NewRow();
                for (int i = 0; i < pFt.Fields.FieldCount; i++)
                {
                    dr[i] = pFt.get_Value(i);
                }
                DT.Rows.Add(dr);
                pFt = pFtCursor.NextFeature();
            }
            return DT;
        }

        private string ParseFieldType(esriFieldType FieldType)
        {
            switch (FieldType)
            {
                case esriFieldType.esriFieldTypeInteger:
                    return "System.Int32";
                case esriFieldType.esriFieldTypeOID:
                    return "System.Int32";
                case esriFieldType.esriFieldTypeDouble:
                    return "System.Double";
                case esriFieldType.esriFieldTypeDate:
                    return "System.DateTime";
                default:
                    return "System.String";
            }
        }

        private void btnSptialSearch_Click(object sender, EventArgs e)
        {
            IsSpatialSearch = false;
            btnSptialSearch.Visible = false;
        }
    }
}

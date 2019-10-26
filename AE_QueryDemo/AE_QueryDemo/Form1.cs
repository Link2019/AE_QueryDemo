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

namespace AE_QueryDemo
{
    public partial class Form1 : Form
    {
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
            OpenMxd();
            Global.myDGV1 = dataGridView1;
            OpenGDB();
            Global.myDGV1.DataSource = frmAttribute.Search((axMapControl1.get_Layer(Get_Layer("北部湾")) as IFeatureLayer).FeatureClass, "");

        }
        /// <summary>
        /// 获得精确图层名下的index
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private int Get_Layer(string LayerName)
        {
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name.Equals(LayerName))
                {
                    return i;
                }
            }
            return -1;
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


    }
}
